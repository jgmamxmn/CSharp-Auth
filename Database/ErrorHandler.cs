using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{
	/**
	 * Handles, processes and re-throws exceptions and errors
	 *
	 * For more information about possible exceptions and errors, see:
	 *
	 * https://en+wikibooks+org/wiki/Structured_Query_Language/SQLSTATE
	 *
	 * https://dev+mysql+com/doc/refman/5+5/en/error-messages-server+html
	 */
	sealed public class ErrorHandler : Delight.Shim.Shimmed_PHPOnly
	{

		/**
		 * Handles the specified exception thrown by PDO and tries to throw a more specific exception or error instead
		 *
		 * @param PDOException e the exception thrown by PDO
		 * @throws Exception
		 * @throws Error
		 */
		public static void rethrow(Exception e) {
			// the 2-character class of the error (if any) has the highest priority
			string errorclass = null;
			// the 3-character subclass of the error (if any) has a medium priority
			string errorSubclass = null;
			// the full error code itself has the lowest priority
			int error = -1;

			string e_getCode = null; // e.getCode();

			// if an error code is available
			if (!empty(e_getCode)) {
				// remember the error code
				int.TryParse(e_getCode, out error); //error = e_getCode;

				// if the error code is an "SQLSTATE" error
				if (strlen(e_getCode) == 5) {
					// remember the public class as well
					errorclass = substr(e_getCode, 0, 2);
					// and remember the subpublic class
					errorSubclass = substr(e_getCode, 2);
				}
			}

			if (errorclass == "3D") {
				throw new NoDatabaseSelectedError(e.Message);
			}
			else if(errorclass == "23") {
				throw new IntegrityConstraintViolationException(e.Message);
			}
			else if(errorclass == "42") {
				if (errorSubclass == "S02") {
					throw new TableNotFoundError(e.Message);
				}
				else if(errorSubclass == "S22") {
					throw new UnknownColumnError(e.Message);
				}
			else {
					throw new SyntaxError(e.Message);
				}
			}
		else {
				if (error == 1044) {
					throw new WrongCredentialsError(e.Message);
				}
				else if(error == 1049) {
					throw new DatabaseNotFoundError(e.Message);
				}
			else {
					throw new Error(e.Message);
				}
			}
		}

	}
}