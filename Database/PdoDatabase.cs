using Delight.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight.Db
{
	/*use PDO;
	use PDOException;
	use PDOStatement;
	use Delight\Db\Throwable\BeginTransactionFailureException;
	use Delight\Db\Throwable\CommitTransactionFailureException;
	use Delight\Db\Throwable\EmptyValueListError;
	use Delight\Db\Throwable\EmptyWhereClauseError;
	use Delight\Db\Throwable\RollBackTransactionFailureException;*/

	public class BindValues : Dictionary<string,object>
	{

		public string Serialize()
		{
			return string.Join
			(
				"; ",
				this.Select(KVP => $"{KVP.Key} = {KVP.Value.ToString()}")
			);
		}

	}


	/** Database access using PHP"s built-in PDO */
	public sealed class PdoDatabase : Database 
	{

		/** @var array|null the old connection attributes to restore during denormalization */
		private Dictionary<ePDO, object> previousAttributes;
		/** @var array|null the new connection attributes to apply during normalization */
		private Dictionary<ePDO, object> attributes;
		/** @var PDO|null the connection that this public class operates on (may be lazily loaded) */
		private Shim.PDO pdo;
		/** @var PdoDsn|null the PDO-specific DSN that may be used to establish the connection */
		private PdoDsn dsn;
		/** @var string|null the name of the driver that is used for the current connection (may be lazily loaded) */
		private string driverName;
		/** @var Profiler|null the profiler that is used to analyze query performance during development */
		private Profiler profiler;
		/** @var callable[] the list of pending callbacks to execute when the connection has been established */
		private List<DgtOnConnectListener> onConnectListeners;

		public enum ePDO
		{
			ATTR_ERRMODE, ERRMODE_EXCEPTION,
			ATTR_DEFAULT_FETCH_MODE, FETCH_ASSOC, FETCH_COLUMN,
			ATTR_EMULATE_PREPARES,
			ATTR_CASE, CASE_NATURAL,
			ATTR_STRINGIFY_FETCHES,
			ATTR_ORACLE_NULLS, NULL_NATURAL,

			ATTR_CLIENT_VERSION,
			ATTR_SERVER_VERSION,

			ATTR_SERVER_INFO,
			ATTR_DRIVER_NAME
		}

		/**
		 * Constructor
		 *
		 * This is private to prevent direct usage
		 *
		 * Call one of the static factory methods instead
		 *
		 * @param PDO|null pdoInstance (optional) the connection that this public class operates on
		 * @param PdoDsn|null pdoDsn (optional) the PDO-specific DSN that may be used to establish the connection
		 * @param bool|null (optional) preserveOldState whether the old state of the connection should be preserved
		 */
		public PdoDatabase(Shim.PDO _pdoInstance = null, PdoDsn _pdoDsn = null, bool _preserveOldState = false) 
		{
			// if the old state of the connection must be stored somewhere
			if (_preserveOldState) {
				// prepare an array for that task
				this.previousAttributes = new Dictionary<ePDO, object>();
			}
			// if the old state of the connection doesn"t need to be tracked
			else {
				this.previousAttributes = null;
			}

			// track the new attributes that should be applied during normalization
			this.attributes = new Dictionary<ePDO, object>
			{
				// set the error mode for this connection to throw exceptions
				{ ePDO.ATTR_ERRMODE , ePDO.ERRMODE_EXCEPTION },
				// set the default fetch mode for this connection to use associative arrays
				{ ePDO.ATTR_DEFAULT_FETCH_MODE , ePDO.FETCH_ASSOC },
				// prefer native prepared statements over emulated ones
				{ ePDO.ATTR_EMULATE_PREPARES , false },
				// use lowercase and uppercase as returned by the server
				{ ePDO.ATTR_CASE , ePDO.CASE_NATURAL },
				// don"t convert numeric values to strings when fetching data
				{ ePDO.ATTR_STRINGIFY_FETCHES , false },
				// keep `null` values and empty strings as returned by the server
				{ ePDO.ATTR_ORACLE_NULLS , ePDO.NULL_NATURAL }
			};

			this.pdo = _pdoInstance;
			this.dsn = _pdoDsn;
			this.profiler = null;
			this.onConnectListeners = new List<DgtOnConnectListener>();
		}

		/**
		 * Creates and returns a new instance from an existing PDO instance
		 *
		 * @param PDO pdoInstance the PDO instance to use
		 * @param bool|null (optional) preserveOldState whether the old state of the PDO instance should be preserved
		 * @return static the new instance
		 */
		//public static PdoDatabase fromPdo(PDO pdoInstance, bool? preserveOldState = null) {
		//	return new PdoDatabase(pdoInstance, null, preserveOldState ?? false);
		//}

		/**
		 * Creates and returns a new instance from a PDO-specific DSN
		 *
		 * The connection will be lazily loaded, i+e+ it won"t be established before it"s actually needed
		 *
		 * @param PdoDsn pdoDsn the PDO-specific DSN to use
		 * @return static the new instance
		 */
		public static PdoDatabase fromDsn(PdoDsn pdoDsn) {
			return new PdoDatabase(null, pdoDsn);
		}

		/**
		 * Creates and returns a new instance from a data source described for use with PDO
		 *
		 * The connection will be lazily loaded, i+e+ it won"t be established before it"s actually needed
		 *
		 * @param PdoDataSource pdoDataSource the data source to use
		 * @return static the new instance
		 */
		public static PdoDatabase fromDataSource(PdoDataSource pdoDataSource) {
			return new PdoDatabase (null, pdoDataSource.toDsn());
		}

		public override List<DatabaseResultRow> select(string query, BindValues bindValues = null)
		{
			return this.selectInternal((stmt) =>
				/** @var PDOStatement stmt */
				stmt.fetchAll()
			, query, bindValues);
		}

		public override object selectValue(string query, BindValues bindValues = null)
		{
			return this.selectInternal((stmt) =>
				/** @var PDOStatement stmt */
				stmt.fetchColumn(0)
			, query, bindValues);
		}

		public override DatabaseResultRow selectRow(string query, BindValues bindValues = null)
		{
			return this.selectInternal((stmt) =>
						/** @var PDOStatement stmt */
						stmt.fetch(),
					query, bindValues);
		}

		public override List<object> selectColumn(string query, BindValues bindValues = null)
		{
			return this.selectInternal((stmt) =>
					/** @var PDOStatement stmt */
					stmt.fetchAll(ePDO.FETCH_COLUMN, 0),
				query, bindValues);
		}

		public override int insert(string _tableName, Dictionary<string, object> insertMappings)
		=> insert(_tableName, insertMappings, null, out object _);

		override public int insert(string _tableName, Dictionary<string,object> insertMappings, string returnColumnName, out object returnedColumn)
		{
			// if no values have been provided that could be inserted
			if (empty(insertMappings)) {
				// we cannot perform an insert here
				throw new EmptyValueListError();
			}

			// Make an equivalent param list with the @ symbol
			BindValues insertMappings2 = new BindValues();
			List<string> valuePlaceholders = new List<string>();
			foreach (var kvp in insertMappings)
			{
				string placeholder = "@" + kvp.Key;
				insertMappings2.Add(placeholder, kvp.Value);
				valuePlaceholders.Add(placeholder);
			}

			// escape the table name
			_tableName = this.quoteTableName(_tableName);
			// get the column names
			var columnNames = array_keys(insertMappings);
			// escape the column names
			columnNames = array_map(this.quoteIdentifier, columnNames);
			// build the column list
			var columnList = implode(", ", columnNames);
			// prepare the values (which are placeholders only)
			//var values = array_fill(0, count(insertMappings), "?");
			// build the value list
			var placeholderList = implode(", ", valuePlaceholders);
			// and finally build the full statement (still using placeholders)
			var statement = new StringBuilder().Append("INSERT INTO ").Append(_tableName)
				.Append(" (").Append(columnList).Append(") VALUES (").Append(placeholderList).Append(")");

			var ExecType = Database.ExecType.NonQuery;
			if (!string.IsNullOrEmpty(returnColumnName))
			{
				statement.Append(" RETURNING ").Append(returnColumnName);
				ExecType = ExecType.Scalar;
			}

			statement.Append(";");

			// execute the (parameterized) statement and supply the values to be bound to it
			return this.exec(ExecType, statement.ToString(), insertMappings2, out returnedColumn);
		}

		public override int update(string tableName, Dictionary<string, object> updateMappings, Dictionary<string, object> whereMappings)
		{
			// if no values have been provided that we could update to
			if (empty(updateMappings)) {
				// we cannot perform an update here
				throw new EmptyValueListError();
			}

			// if no values have been provided that we could filter by (which is possible but dangerous)
			if (empty(whereMappings)) {
				// we should not perform an update here
				throw new EmptyWhereClauseError();
			}

			// escape the table name
			tableName = this.quoteTableName(tableName);
			// prepare a list for the values to be bound (both by the list of new values and by the conditions)
			var bindValues = new BindValues(); // List<object>();
											   // prepare a list for the individual directives of the `SET` clause
			var setDirectives = new List<string>();

			int bindIdx = 0;

			// for each mapping of a column name to its respective new value
			foreach (var kvp in updateMappings)
			{

				var updateColumn = kvp.Key;
				var updateValue = kvp.Value;

				// create an individual directive with the column name and a placeholder for the value

				++bindIdx;
				string bindName = "@update" + bindIdx.ToString();

				setDirectives.Add(this.quoteIdentifier(updateColumn) + " = " + bindName);
				// and remember which value to bind here
				bindValues.Add(bindName, updateValue);
			}

			// prepare a list for the individual predicates of the `WHERE` clause
			var wherePredicates = new List<string>();

			// for each mapping of a column name to its respective value to filter by
			foreach (var kvp in whereMappings) {

				var whereColumn = kvp.Key;
				var whereValue = kvp.Value;

				++bindIdx;
				string bindName = "@where" + bindIdx.ToString();

				// create an individual predicate with the column name and a placeholder for the value
				wherePredicates.Add(this.quoteIdentifier(whereColumn) + " = " + bindName);
				// and remember which value to bind here
				bindValues.Add(bindName, whereValue);
			}

			// build the full statement (still using placeholders)
			var statement = "UPDATE " + tableName + " SET " + implode(", ", setDirectives) + " WHERE " + implode(" AND ", wherePredicates) + ";";

			// execute the (parameterized) statement and supply the values to be bound to it
			return this.exec(Database.ExecType.NonQuery, statement, bindValues);
		}

		public override int delete(string tableName, Dictionary<string, object> whereMappings)
		{
			// if no values have been provided that we could filter by (which is possible but dangerous)
			if (empty(whereMappings)) {
				// we should not perform a deletion here
				throw new EmptyWhereClauseError();
			}

			// escape the table name
			tableName = this.quoteTableName(tableName);
			// prepare a list for the values to be bound by the conditions
			var bindValues = new BindValues(); // List<object>();
			// prepare a list for the individual predicates of the `WHERE` clause
			var wherePredicates = new List<string>();

			int bindIdx = 0;

			// for each mapping of a column name to its respective value to filter by
			foreach (var kvp in whereMappings) {

				var whereColumn = kvp.Key;
				var whereValue = kvp.Value;

				string bindName = "@bind" + (++bindIdx).ToString();

				// create an individual predicate with the column name and a placeholder for the value
				wherePredicates.Add(this.quoteIdentifier(whereColumn) + " = " + bindName);
				// and remember which value to bind here
				bindValues.Add(bindName, whereValue);
			}

			// build the full statement (still using placeholders)
			var statement = "DELETE FROM " + tableName + " WHERE " + implode(" AND ", wherePredicates) + ";";

			// execute the (parameterized) statement and supply the values to be bound to it
			return this.exec(ExecType.NonQuery, statement, bindValues);
		}

		public override int exec(ExecType execType, string statement, BindValues bindValues = null)
			=> exec(execType, statement, bindValues, out object _);

		public override int exec(ExecType execType, string statement, BindValues bindValues, out object returnedColumn)
		{
			this.normalizeConnection();

			Shim.PDOStatement stmt=null;

			try {
				// create a prepared statement from the supplied SQL string
				stmt = this.pdo.prepare(statement);
			}
			catch (Exception e) {
				ErrorHandler.rethrow(e);
			}

			// if a performance profiler has been defined
			if (isset(this.profiler)) {
				this.profiler.beginMeasurement();
			}

			int affectedRows = 0;
			/** @var PDOStatement stmt */

			//try {
				// bind the supplied values to the statement and execute it

				switch(execType)
				{
					case ExecType.NonQuery:
						affectedRows = stmt.executeNonQuery(bindValues);
						returnedColumn = null;
						break;
					case ExecType.Reader:
						stmt.executeReader(bindValues);
						affectedRows = stmt.affectedRowCount();
						returnedColumn = null;
						break;
					case ExecType.Scalar:
						returnedColumn = stmt.executeScalar(bindValues);
						break;
					default:
						throw new Exception("Exec type should be NonQuery, Reader, or Scalar.");
				}
			//} catch (Exception e) { returnedColumn = null; ErrorHandler.rethrow(e); }

			// if a performance profiler has been defined
			if (isset(this.profiler)) {
				this.profiler.endMeasurement(statement, bindValues);
			}


			this.denormalizeConnection();

			return affectedRows;
		}


		public override void beginTransaction() {
			this.normalizeConnection();

			bool success;
			string err = null;

			try {
				success = this.pdo.beginTransaction();
			}
			catch (Exception e) {
				success = false;
				err = e.Message;
			}

			this.denormalizeConnection();

			if (!success) {
				throw new BeginTransactionFailureException(err);
			}
		}

		public override void startTransaction() {
			this.beginTransaction();
		}

		public override bool isTransactionActive() {
			this.normalizeConnection();

			var state = this.pdo.inTransaction();

			this.denormalizeConnection();

			return state;
		}

		public override void commit() {
			this.normalizeConnection();

			bool success=false; string msg=null;

			try {
				success = this.pdo.commit();
			}
			catch (Exception e) {
				msg = e.Message;
			}

			this.denormalizeConnection();

			if (success != true) {
				throw new CommitTransactionFailureException(msg);
			}
		}

		public override void rollBack() {
			this.normalizeConnection();

			bool success;
			string err=null;

			try {
				success = this.pdo.rollBack();
			}
			catch (Exception e) {
				success = false; 
				err = e.Message;
			}

			this.denormalizeConnection();

			if (!success) {
				throw new RollBackTransactionFailureException(err);
			}
		}

		override public Profiler getProfiler() {
			return this.profiler;
		}

		override public PdoDatabase setProfiler(Profiler profiler = null) {
			this.profiler = profiler;
			return this;
		}

		override public string getDriverName() {
			this.ensureConnected();

			switch (this.driverName) {
				case PdoDataSource.DRIVER_NAME_MYSQL: return "MySQL";
				case PdoDataSource.DRIVER_NAME_POSTGRESQL: return "PostgreSQL";
				case PdoDataSource.DRIVER_NAME_SQLITE: return "SQLite";
				case PdoDataSource.DRIVER_NAME_ORACLE: return "Oracle";
				default: return this.driverName;
			}
		}

		///**
		// * Returns some driver-specific information about the server that this instance is connected to
		// *
		// * @return string
		// */
		//public string getServerInfo() {
		//	this.ensureConnected();
		//	return this.pdo.getAttribute(ePDO.ATTR_SERVER_INFO);
		//}

		///**
		// * Returns the version of the database software used on the server that this instance is connected to
		// *
		// * @return string
		// */
		//public string getServerVersion() {
		//	this.ensureConnected();
		//	return this.pdo.getAttribute(ePDO.ATTR_SERVER_VERSION);
		//}

		///**
		// * Returns the version of the database client used by this instance
		// *
		// * @return string
		// */
		//public string getClientVersion() {
		//	this.ensureConnected();

		//	return this.pdo.getAttribute(ePDO.ATTR_CLIENT_VERSION);
		//}

		public override string quoteIdentifier(string identifier) {
			this.ensureConnected();

			string ch;

			if (this.driverName == PdoDataSource.DRIVER_NAME_MYSQL) {
				ch = "`";
			}
			else {
				ch = "\"";
			}

			return ch + str_replace(ch, ch + ch, identifier) + ch;
		}

		public override string quoteTableName(string[] _tableName)
		{
			return implode("+", array_map(this.quoteIdentifier, _tableName));
		}
		public override string quoteTableName(string _tableName)
		{ 
			return this.quoteIdentifier(_tableName);
		}

		/*public Dictionary<string,object> quoteLiteral(Dictionary<string,object> literal)
		{
			var ret = new Dictionary<string, object>();
			this.ensureConnected();
			foreach (var kvp in literal)
			{
				ret.Add(kvp.Key,this.pdo.quote(kvp.Value));
			}
			return ret;
		}

		public override string quoteLiteral(string literal)
		{
			this.ensureConnected();
			return this.pdo.quote(literal);
		}*/

		public override Database addOnConnectListener(DgtOnConnectListener onConnectListener)
		{
			// if the database connection has not been established yet
			if (this.pdo == null) {
				// schedule the callback for later execution
				this.onConnectListeners.Add( onConnectListener);
			}
			// if the database connection has already been established
			else {
				// execute the callback immediately
				onConnectListener(this);
			}

			return this;
		}

		/** Makes sure that the connection is active and otherwise establishes it automatically */
		private void ensureConnected() {
			if (this.pdo == null) {
				try {
					this.pdo = new Shim.PDO(this.dsn.getDsn(), this.dsn.getUsername(), this.dsn.getPassword());
				}
				catch (Exception e) {
					ErrorHandler.rethrow(e);
				}

				// iterate over all listeners waiting for the connection to be established
				foreach (var onConnectListener in this.onConnectListeners) {
					// execute the callback
					onConnectListener(this);
				}

				// discard the listeners now that they have all been executed
				this.onConnectListeners = new List<DgtOnConnectListener>();

				this.dsn = null;
			}

			if (this.driverName == null) {
				this.driverName = (string)this.pdo.getAttribute(ePDO.ATTR_DRIVER_NAME);
			}
		}

		/** Normalizes this connection by setting attributes that provide the strong guarantees about the connection"s behavior that we need */
		private void normalizeConnection() {
			this.ensureConnected();

			this.configureConnection(this.attributes, this.previousAttributes);
		}

		/** Restores this connection"s original behavior if desired */
		private void denormalizeConnection() {
			this.configureConnection(this.previousAttributes, this.attributes);
		}

		/**
		 * Configures this connection by setting appropriate attributes
		 *
		 * @param array|null newAttributes the new attributes to set
		 * @param array|null oldAttributes where old configurations may be saved to restore them later
		 */
		private void configureConnection(Dictionary<ePDO,object> newAttributes = null, Dictionary<ePDO,object> oldAttributes = null) {
			// if a connection is available
			if (isset(this.pdo)) {
				// if there are attributes that need to be applied
				if (isset(newAttributes)) {
					// get the keys and values of the attributes to apply
					foreach (var kvp in newAttributes) {

						var key = kvp.Key;
						var newValue = kvp.Value;
						object oldValue = null;

						// if the old state of the connection must be preserved
						if (isset(oldAttributes)) {
							// retrieve the old value for this attribute
							try {
								oldValue = this.pdo.getAttribute(key);
							}
							catch (Exception) {
								// the specified attribute is not supported by the driver
								oldValue = null;
							}

							// if an old value has been found
							if (isset(oldValue)) {
								// if the old value differs from the new value that we"re going to set
								if (oldValue != newValue) {
									// save the old value so that we"re able to restore it later
									oldAttributes[key] = oldValue;
								}
							}
						}

						// and then set the desired new value
						this.pdo.setAttribute(key, newValue);
					}

					// if the old state of the connection doesn"t need to be preserved
					if (!isset(oldAttributes)) {
						// we"re done updating attributes for this connection once and for all
						newAttributes = null;
					}
				}
			}
		}

		/**
		 * Selects from the database using the specified query and returns what the supplied callback extracts from the result set
		 *
		 * You should not include any dynamic input in the query
		 *
		 * Instead, pass `?` characters (without any quotes) as placeholders and pass the actual values in the third argument
		 *
		 * @param callable callback the callback that receives the executed statement and can then extract and return the desired results
		 * @param string query the query to select with
		 * @param array|null bindValues (optional) the values to bind as replacements for the `?` characters in the query
		 * @return mixed whatever the callback has extracted and returned from the result set
		 */
		private RetType selectInternal<RetType>(Func<Shim.PDOStatement, RetType> callback, string query, BindValues bindValues=null)
			where RetType : class
		{
			this.normalizeConnection();

			Shim.PDOStatement stmt=null;

			//try {
				// create a prepared statement from the supplied SQL string
				stmt = this.pdo.prepare(query);
			//}catch (Exception e) {	ErrorHandler.rethrow(e);}

			// if a performance profiler has been defined
			if (isset(this.profiler)) {
				this.profiler.beginMeasurement();
			}

			/** @var PDOStatement stmt */

			// bind the supplied values to the query and execute it
			//try {
				stmt.executeReader(bindValues);
			//}	catch (Exception e) {	ErrorHandler.rethrow(e);		}

			// if a performance profiler has been defined
			if (isset(this.profiler)) {
				this.profiler.endMeasurement(query, bindValues, 1);
			}

			// fetch the desired results from the result set via the supplied callback
			var results = callback(stmt);

			this.denormalizeConnection();

			bool _empty(RetType x)
			{
				if (!(x is RetType))
					return false;
				return true;
			}
			bool is_bool(RetType x)
				=> x is bool;
			bool is_array(RetType x)
				=> false; // TODO ???

			// if the result is empty
			if (_empty(results) 
				&& !stmt.hasRows() //&& stmt.rowCount() == 0 
				&& (this.driverName != PdoDataSource.DRIVER_NAME_SQLITE || is_bool(results) || is_array(results))) {
				// consistently return `null`
				return null;
			}
		// if some results have been found
			else {
				// return these as extracted by the callback
				return results;
			}
		}

	}
}