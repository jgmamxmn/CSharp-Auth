using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{

	/*use Delight\Db\Throwable\BeginTransactionFailureException;
	use Delight\Db\Throwable\CommitTransactionFailureException;
	use Delight\Db\Throwable\IntegrityConstraintViolationException;
	use Delight\Db\Throwable\RollBackTransactionFailureException;
	use Delight\Db\Throwable\TransactionFailureException;*/

	/** Safe and convenient SQL database access in a driver-agnostic way */
	public abstract class Database : Delight.Shim.Shimmed
	{
		public delegate void DgtOnConnectListener(PdoDatabase pdoDatabase);


		/**
		 * Selects from the database using the specified query and returns all rows and columns
		 *
		 * You should not include any dynamic input in the query
		 *
		 * Instead, pass `?` characters (without any quotes) as placeholders and pass the actual values in the second argument
		 *
		 * @param string query the SQL query to select with
		 * @param array|null bindValues (optional) the values to bind as replacements for the `?` characters in the query
		 * @return array|null the rows and columns returned by the server or `null` if no results have been found
		 */
		public abstract List<DatabaseResultRow> select(string query, BindValues bindValues = null);

		/**
		 * Selects from the database using the specified query and returns the value of the first column in the first row
		 *
		 * You should not include any dynamic input in the query
		 *
		 * Instead, pass `?` characters (without any quotes) as placeholders and pass the actual values in the second argument
		 *
		 * @param string query the SQL query to select with
		 * @param array|null bindValues (optional) the values to bind as replacements for the `?` characters in the query
		 * @return mixed|null the value of the first column in the first row returned by the server or `null` if no results have been found
		 */
		public abstract object selectValue(string query, BindValues bindValues = null);

		/**
		 * Selects from the database using the specified query and returns the first row
		 *
		 * You should not include any dynamic input in the query
		 *
		 * Instead, pass `?` characters (without any quotes) as placeholders and pass the actual values in the second argument
		 *
		 * @param string query the SQL query to select with
		 * @param array|null bindValues (optional) the values to bind as replacements for the `?` characters in the query
		 * @return array|null the first row returned by the server or `null` if no results have been found
		 */
		public abstract DatabaseResultRow selectRow(string query, BindValues bindValues = null);

		/**
		 * Selects from the database using the specified query and returns the first column
		 *
		 * You should not include any dynamic input in the query
		 *
		 * Instead, pass `?` characters (without any quotes) as placeholders and pass the actual values in the second argument
		 *
		 * @param string query the SQL query to select with
		 * @param array|null bindValues (optional) the values to bind as replacements for the `?` characters in the query
		 * @return array|null the first column returned by the server or `null` if no results have been found
		 */
		public abstract List<object> selectColumn(string query, BindValues bindValues = null);

		/**
		 * Inserts the given mapping between columns and values into the specified table
		 *
		 * @param string|string[] tableName the name of the table to insert into (or an array of components of the qualified name)
		 * @param array insertMappings the mappings between columns and values to insert
		 * @return int the number of inserted rows
		 * @throws IntegrityConstraintViolationException
		 */
		public abstract int insert(string _tableName, Dictionary<string,object> insertMappings);
		public abstract int insert(string _tableName, Dictionary<string, object> insertMappings, string returnColumnName, out object returnedColumn);

		/**
		 * Updates the specified table with the given mappings between columns and values
		 *
		 * @param string|string[] tableName the name of the table to update (or an array of components of the qualified name)
		 * @param array updateMappings the mappings between columns and values to update
		 * @param array whereMappings the mappings between columns and values to filter by
		 * @return int the number of updated rows
		 * @throws IntegrityConstraintViolationException
		 */
		public abstract int update(string tableName, Dictionary<string,object> updateMappings, Dictionary<string,object> whereMappings);

		/**
		 * Deletes from the specified table where the given mappings between columns and values are found
		 *
		 * @param string|string[] tableName the name of the table to delete from (or an array of components of the qualified name)
		 * @param array whereMappings the mappings between columns and values to filter by
		 * @return int the number of deleted rows
		 */
		public abstract int delete(string tableName, Dictionary<string,object> whereMappings);

		/**
		 * Executes an arbitrary statement and returns the number of affected rows
		 *
		 * This is especially useful for custom `INSERT`, `UPDATE` or `DELETE` statements
		 *
		 * You should not include any dynamic input in the statement
		 *
		 * Instead, pass `?` characters (without any quotes) as placeholders and pass the actual values in the second argument
		 *
		 * @param string statement the SQL statement to execute
		 * @param array|null bindValues (optional) the values to bind as replacements for the `?` characters in the statement
		 * @return int the number of affected rows
		 * @throws IntegrityConstraintViolationException
		 * @throws TransactionFailureException
		 */
		public abstract int exec(ExecType execType, string statement, BindValues bindValues = null);
		public abstract int exec(ExecType execType, string statement, BindValues bindValues, out object returnedColumn);

		public enum ExecType
		{
			Scalar, Reader, NonQuery
		}


		/**
		 * Starts a new transaction and turns off auto-commit mode
		 *
		 * Changes won"t take effect until the transaction is either finished via `commit` or cancelled via `rollBack`
		 *
		 * @throws BeginTransactionFailureException
		 */
		public abstract void beginTransaction();

		/**
		 * Alias of `beginTransaction`
		 *
		 * @throws BeginTransactionFailureException
		 */
		public abstract void startTransaction();

		/**
		 * Returns whether a transaction is currently active
		 *
		 * @return bool
		 */
		public abstract bool isTransactionActive();

		/**
		 * Finishes an existing transaction and turns on auto-commit mode again
		 *
		 * This makes all changes since the last commit or roll-back permanent
		 *
		 * @throws CommitTransactionFailureException
		 */
		public abstract void commit();

		/**
		 * Cancels an existing transaction and turns on auto-commit mode again
		 *
		 * This discards all changes since the last commit or roll-back
		 *
		 * @throws RollBackTransactionFailureException
		 */
		public abstract void rollBack();

		/**
		 * Returns the performance profiler currently used by this instance (if any)
		 *
		 * @return Profiler|null
		 */
		public abstract Profiler getProfiler();

		/**
		 * Sets the performance profiler used by this instance
		 *
		 * This should only be used during development and not in production
		 *
		 * @param Profiler|null profiler the profiler instance or `null` to disable profiling again
		 * @return static this instance for chaining
		 */
		public abstract Database setProfiler(Profiler profiler = null);

		/**
		 * Returns the name of the driver that is used for the current connection
		 *
		 * @return string
		 */
		public abstract string getDriverName();

		/**
		 * Quotes an identifier (e+g+ a table name or column reference)
		 *
		 * This allows for special characters and reserved keywords to be used in identifiers
		 *
		 * There is usually no need to call this method
		 *
		 * Identifiers should not be set from untrusted user input and in most cases not even from dynamic expressions
		 *
		 * @param string identifier the identifier to quote
		 * @return string the quoted identifier
		 */
		public abstract string  quoteIdentifier(string identifier);

		/**
		 * Quotes a table name
		 *
		 * This allows for special characters and reserved keywords to be used in table names
		 *
		 * There is usually no need to call this method
		 *
		 * Table names should not be set from untrusted user input and in most cases not even from dynamic expressions
		 *
		 * @param string|string[] tableName the table name to quote (or an array of components of the qualified name)
		 * @return string the quoted table name
		 */
		public abstract string quoteTableName(string tableName);
		public abstract string quoteTableName(string[] tableName);

		/**
		 * Quotes a literal value (e+g+ a string to insert or to use in a comparison) or an array thereof
		 *
		 * This allows for special characters to be used in literal values
		 *
		 * There is usually no need to call this method
		 *
		 * You should always use placeholders for literal values and pass the actual values to bind separately
		 *
		 * @param string literal the literal value to quote
		 * @return string the quoted literal value
		 */
		//public abstract string quoteLiteral(string literal);

		/**
		 * Adds a listener that will execute as soon as the database connection has been established
		 *
		 * If the database connection has already been active before, the listener will execute immediately
		 *
		 * @param callable onConnectListener the callback to execute
		 * @return static this instance for chaining
		 */
		public abstract Database addOnConnectListener(DgtOnConnectListener onConnectListener);

	}
}