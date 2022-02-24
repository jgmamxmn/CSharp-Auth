using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Db
{
	public class BeginTransactionFailureException : TransactionFailureException { public BeginTransactionFailureException(string m) : base(m) { } }
	public class CommitTransactionFailureException : TransactionFailureException { public CommitTransactionFailureException(string m) : base(m) { } }
	/** Error that is thrown when the requested database cannot be found */
	public class DatabaseNotFoundError : Error { public DatabaseNotFoundError(string m) : base(m) { } }
	/** Error that is thrown when an empty list of values is provided where values are required */
	public class EmptyValueListError : Error { }
	/**
	 * Error that is thrown when an empty `WHERE` clause is provided
	 *
	 * Although technically perfectly valid, an empty list of criteria is often provided by mistake
	 *
	 * This is why, for some operations, it is deemed too dangerous and thus disallowed
	 *
	 * Usually, one can simply execute a manual statement instead to get rid of this restriction
	 */
	public class EmptyWhereClauseError : Error { }
	/** Base  class for all conditions that the application might not recover from and thus should not catch */
	public class Error : Exception 
	{
		public Error() : base() { }
		public Error(string message) : base(message) { }
	}
	/**
	 * Exception that is thrown when an integrity constraint is being violated
	 *
	 * Common constraints include "UNIQUE", "NOT NULL" and "FOREIGN KEY"
	 *
	 * Ambiguous column references constitute violations as well
	 */
	public class IntegrityConstraintViolationException : Exception { public IntegrityConstraintViolationException() : base() { } public IntegrityConstraintViolationException(string m) : base(m) { } }
	/** Error that is thrown when no database has been selected */
	public class NoDatabaseSelectedError : Error { public NoDatabaseSelectedError(string message) : base(message) { } }
	/** Exception that is thrown when a transaction cannot be rolled back successfully for some reason */
	public class RollBackTransactionFailureException : TransactionFailureException { public RollBackTransactionFailureException() : base() { } public RollBackTransactionFailureException(string s) : base(s) { } }
	/** Error that is thrown when there is invalid syntax or when an access rule is being violated */
	public class SyntaxError : Error { public SyntaxError() : base() { } public SyntaxError(string s) : base(s) { } }
	/** Error that is thrown when a table cannot be found */
	public class TableNotFoundError : Error { public TableNotFoundError() : base() { } public TableNotFoundError(string m) : base(m) { } }
	/** Base cls for all exceptions that occur with transaction controls */
	abstract public class TransactionFailureException : Exception { protected TransactionFailureException() : base() { } protected TransactionFailureException(string m) : base(m) { } }
	/** Error that is thrown when a column cannot be found */
	public class UnknownColumnError : Error { public UnknownColumnError() : base() { } public UnknownColumnError(string s) : base(s) { } }
	/** Error that is thrown when the supplied credentials cannot be used to access the database */
	public class WrongCredentialsError : Error { public WrongCredentialsError() : base() { } public WrongCredentialsError(string s) : base(s) { } }
}
