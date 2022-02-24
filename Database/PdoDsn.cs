using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{

	/** DSN for use with PHP"s built-in PDO */
	sealed public class PdoDsn : Dsn
	{

		/** @var string the DSN as a string */
		private string dsn;
		/** @var string|null the username that complements the DSN */
		private string username;
		/** @var string|null the password that complements the DSN */
		private string password;

		/**
		 * Constructor
		 *
		 * @param string dsnStr the DSN as a string
		 * @param string username (optional) the username that complements the DSN
		 * @param string password (optional) the password that complements the DSN
		 */
		public PdoDsn(string _dsnStr, string _username = null, string _password = null) {
				this.dsn = _dsnStr;
				this.username = _username;
				this.password = _password;
		}

		/**
		 * Returns the DSN as a string
		 *
		 * @return string
		 */
		public string getDsn() {
			return this.dsn;
		}

		/**
		 * Returns the username that complements the DSN
		 *
		 * @return string|null
		 */
		public string getUsername() {
			return this.username;
		}

		/**
		 * Returns the password that complements the DSN
		 *
		 * @return string|null
		 */
		public string getPassword() {
			return this.password;
		}

	}
}