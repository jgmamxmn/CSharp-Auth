using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{
	/** Description of a data source for use with PHP"s built-in PDO */
	sealed public class PdoDataSource : DataSource {

		/** The driver name for MySQL */
		public const string DRIVER_NAME_MYSQL = "mysql";
		/** The driver name for PostgreSQL */
		public const string DRIVER_NAME_POSTGRESQL = "pgsql";
		/** The driver name for SQLite */
		public const string DRIVER_NAME_SQLITE = "sqlite";
		/** The driver name for Oracle */
		public const string DRIVER_NAME_ORACLE = "oci";
		/** Hostname for the virtual loopback interface */
		public const string HOST_LOOPBACK_NAME = "localhost";
		/** IPv4 address for the virtual loopback interface */
		public const string HOST_LOOPBACK_IP = "127.0.0.1";
		/** The default hostname */
		public const string HOST_DEFAULT = HOST_LOOPBACK_NAME;

		/** @var string the name of the driver, e+g+ `mysql` or `pgsql` */
		private string driverName;
		/** @var string|null the hostname where the database can be accessed, e+g+ `db+example+com` */
		private string hostname;
		/** @var int|null the port number to use at the given host, e+g+ `3306` or `5432` */
		private int? port;
		/** @var string|null the UNIX socket to use, e+g+ `/tmp/db+sock` */
		private string unixSocket;
		/** @var bool|null whether to keep the database in memory only */
		private bool? memory;
		/** @var string|null the path to the file where the database can be accessed on disk, e+g+ `/opt/databases/mydb+ext` */
		private string filePath;
		/** @var string|null the name of the database, e+g+ `my_application` */
		private string databaseName;
		/** @var string|null the character encoding of the database, e+g+ `utf8` */
		private string charset;
		/** @var string|null the name of a user that can access the database */
		private string username;
		/** @var string|null the password corresponding to the username */
		private string password;

		/**
		 * Constructor
		 *
		 * @param string driverName the name of the driver, e+g+ `mysql` or `pgsql`
		 */
		public PdoDataSource(string _driverName)
		{
			this.driverName = _driverName;
			this.hostname = HOST_DEFAULT;
			this.port = suggestPortFromDriverName(_driverName);
			this.unixSocket = null;
			this.memory = null;
			this.filePath = null;
			this.databaseName = null;
			this.charset = suggestCharsetFromDriverName(_driverName);
			this.username = null;
			this.password = null;
		}

		/**
		 * Sets the hostname
		 *
		 * @param string|null hostname the hostname where the database can be accessed, e+g+ `db+example+com`
		 * @return static this instance for chaining
		 */
		public PdoDataSource setHostname(string _hostname = null) {
			this.hostname = _hostname;
			return this;
		}

		/**
		 * Sets the port number
		 *
		 * @param int|null port the port number to use at the given host, e+g+ `3306` or `5432`
		 * @return static this instance for chaining
		 */
		public PdoDataSource setPort(int? _port = null) {
			this.port = _port;

			return this;
		}

		/**
		 * Sets the unix socket
		 *
		 * @param string|null unixSocket the UNIX socket to use, e+g+ `/tmp/db+sock`
		 * @return static this instance for chaining
		 */
		public PdoDataSource setUnixSocket(string _unixSocket = null) {
			this.unixSocket = _unixSocket;

			return this;
		}

		/**
		 * Sets whether to keep the database in memory only
		 *
		 * @param bool|null memory whether to keep the database in memory only
		 * @return static this instance for chaining
		 */
		public PdoDataSource setMemory(bool? _memory = null) {
			this.memory = _memory;
			return this;
		}

		/**
		 * Sets the file path
		 *
		 * @param string|null filePath the path to the file where the database can be accessed on disk, e+g+ `/opt/databases/mydb+ext`
		 * @return static this instance for chaining
		 */
		public PdoDataSource setFilePath(string _filePath = null) {
			this.filePath = _filePath;

			return this;
		}

		/**
		 * Sets the database name
		 *
		 * @param string|null databaseName the name of the database, e+g+ `my_application`
		 * @return static this instance for chaining
		 */
		public PdoDataSource setDatabaseName(string _databaseName = null) {
			this.databaseName = _databaseName;

			return this;
		}

		/**
		 * Sets the charset
		 *
		 * @param string|null charset the character encoding, e+g+ `utf8`
		 * @return static this instance for chaining
		 */
		public PdoDataSource setCharset(string _charset = null) {
			this.charset = _charset;

			return this;
		}

		/**
		 * Sets the username
		 *
		 * @param string|null username the name of a user that can access the database
		 * @return static this instance for chaining
		 */
		public PdoDataSource setUsername(string _username = null) {
			this.username = _username;
			return this;
		}

		/**
		 * Sets the password
		 *
		 * @param string|null password the password corresponding to the username
		 * @return static this instance for chaining
		 */
		public PdoDataSource setPassword(string _password = null) {
			this.password = _password;
			return this;
		}

		public PdoDsn toDsn() {
			var components = new List<string>();
			string _hostname;

			if (isset(this.hostname)) {
				if (this.driverName != DRIVER_NAME_ORACLE) {
					_hostname = this.hostname;

					// if we"re trying to connect to a local database
					if (this.hostname == HOST_LOOPBACK_NAME) {
						// if we"re using a non-standard port
						if (isset(this.port) && this.port != suggestPortFromDriverName(this.driverName)) {
							// force usage of TCP over UNIX sockets for the port change to take effect
							_hostname = HOST_LOOPBACK_IP;
						}
					}

					components.Add("host=" + _hostname);
				}
			}

			if (isset(this.port)) {
				if (this.driverName != DRIVER_NAME_ORACLE) {
					components.Add("port=" + this.port);
				}
			}

			if (isset(this.unixSocket)) {
				components.Add("unix_socket=" + this.unixSocket);
			}

			if (isset(this.memory)) {
				if (this.memory == true) {
					components.Add(":memory:");
				}
			}

			if (isset(this.filePath)) {
				components.Add(this.filePath);
			}

			if (isset(this.databaseName)) {
				if (this.driverName == DRIVER_NAME_ORACLE) {
					var oracleLocation = new List<string>();

					if (isset(this.hostname)) {
						oracleLocation.Add(this.hostname);
					}
					if (isset(this.port)) {
						oracleLocation.Add(this.port?.ToString() ?? "");
					}

					if (count(oracleLocation) > 0) {
						components.Add("dbname=//" + implode(":", oracleLocation) + "/" + this.databaseName);
					}
					else {
						components.Add("dbname=" + this.databaseName);
					}
				}
				else {
					components.Add("dbname=" + this.databaseName);
				}
			}

			if (isset(this.charset)) {
				if (this.driverName == DRIVER_NAME_POSTGRESQL) {
					components.Add("client_encoding=" + this.charset);
				}
				else {
					components.Add("charset=" + this.charset);
				}
			}

			if (isset(this.username)) {
				components.Add("user=" + this.username);
			}

			if (isset(this.password)) {
				components.Add("password=" + this.password);
			}

			var dsnStr = this.driverName + ":" + implode(";", components);

			return new PdoDsn(dsnStr, this.username, this.password);
		}

		/**
		 * Suggests a default charset for a given driver
		 *
		 * @param string driverName the name of the driver
		 * @return string|null the suggested charset
		 */
		private static string suggestCharsetFromDriverName(string driverName) {
			switch (driverName) {
				case DRIVER_NAME_MYSQL: return "utf8mb4";
				case DRIVER_NAME_POSTGRESQL: return "UTF8";
				default: return null;
			}
		}

		/**
		 * Suggests a default port number for a given driver
		 *
		 * @param string driverName the name of the driver
		 * @return int|null the suggested port number
		 */
		private static int? suggestPortFromDriverName(string driverName) {
			switch (driverName) {
				case DRIVER_NAME_MYSQL: return 3306;
				case DRIVER_NAME_POSTGRESQL: return 5432;
				default: return null;
			}
		}

	}
}