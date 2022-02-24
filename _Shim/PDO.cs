using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace Delight.Shim
{
	public class PDO : IDisposable
	{
		public Npgsql.NpgsqlConnection Connection;
		public Npgsql.NpgsqlDataReader Reader = null;

		public PDO(string dsn, string username, string password)
		{
			dsn = dsn.TrimEnd();
			var dsn2 = new StringBuilder()
				.Append(dsn);
			if (dsn.Last() != ';')
				dsn2.Append(';');

			dsn2.Append("Username=").Append(username).Append(";")
				.Append("Password=").Append(password).Append(";");

			Connection = new NpgsqlConnection(dsn2.ToString());
			Connection.Open();
		}

		public void Disconnect() => Connection.Close();

		public enum ATTR_DRIVER_NAME
		{
			PDO_POSTGRESQL
		}
		public PDOStatement prepare(string queryString) => new PDOStatement(this, queryString);
		public bool beginTransaction()
		{
			_transaction=Connection.BeginTransaction();
			return true; // false means failure, let's just throw an exception
		}
		public bool commit()
		{
			if(_transaction is object)
			{
				_transaction.Commit();
				_transaction = null;
				return true;
			}
			else
			{
				return false;
			}
		}
		public bool rollBack()
		{
			if (_transaction is object)
			{
				_transaction.Rollback();
				_transaction = null;
				return true;
			}
			else
			{
				return false;
			}
		}

		private NpgsqlTransaction _transaction = null;
		public bool inTransaction() => (_transaction is object);

		private Dictionary<Db.PdoDatabase.ePDO, object> attribs = new System.Collections.Generic.Dictionary<Db.PdoDatabase.ePDO, object>();
		private bool disposedValue;

		public void setAttribute(Db.PdoDatabase.ePDO attr, object val)
		{
			if (attribs.ContainsKey(attr))
				attribs.Remove(attr);
			attribs.Add(attr, val);
		}
		public object getAttribute(Db.PdoDatabase.ePDO attr)
		{
			switch(attr)
			{
				case Db.PdoDatabase.ePDO.ATTR_DRIVER_NAME:
					return Db.PdoDataSource.DRIVER_NAME_POSTGRESQL;
				default:
					if (attribs.TryGetValue(attr, out object val))
						return val;
					else
						return null; // ???
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if(Reader is object)
					{
						Reader.Close();
						Reader = null;
					}
					if (Connection is object)
					{
						Connection.CloseAsync();
						Connection = null;
					}
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	public class PDOStatement
	{
		PDO PDO;
		NpgsqlCommand Command;
		NpgsqlDataReader Reader=null;

		public PDOStatement(PDO pdo, string queryString)
		{
			PDO = pdo;
			Command = new NpgsqlCommand(queryString, PDO.Connection);
			//Command.Prepare();
		}
		private void _prepParams(Dictionary<string, object> queryParams)
		{
			Command.Parameters.Clear();
			Command.Parameters.AddRange(queryParams.Select(X => new NpgsqlParameter(X.Key, X.Value)).ToArray());
		}
		private void _ensureFree()
		{
			if (PDO.Reader is object)
			{
				PDO.Reader.Close();
				PDO.Reader = null;
			}
		}
		public void executeReader(Dictionary<string,object> queryParams)
		{
			_ensureFree();
			_prepParams(queryParams);
			Reader = Command.ExecuteReader();
			PDO.Reader = Reader;
		}
		public object executeScalar(Dictionary<string, object> queryParams)
		{
			_ensureFree();
			_prepParams(queryParams);
			return Command.ExecuteScalar();
		}
		public int executeNonQuery(Dictionary<string,object> queryParams)
		{
			_ensureFree();
			_prepParams(queryParams);
			return Command.ExecuteNonQuery();
		}

		public Delight.Auth.DatabaseResultRow fetch()
		{
			if(Reader.Read())
			{
				var ColumnSchema = Reader.GetColumnSchema();

				var row = new Delight.Auth.DatabaseResultRow();

				int colNum = 0;
				foreach (var colObj in ColumnSchema)
				{
					var val = Reader.GetValue(colNum);
					row.Add(colObj.ColumnName, val);

					++colNum;
				}

				return row;
			}
			else
			{
				return null;
			}
		}

		public List<Delight.Auth.DatabaseResultRow> fetchAll()
		{
			var ret = new List<Delight.Auth.DatabaseResultRow>();

			Delight.Auth.DatabaseResultRow row = null;
			do
			{
				row = fetch();
				if (row is object)
					ret.Add(row);
			} while (row is object);

			return ret;
		}

		public List<object> fetchAll(Db.PdoDatabase.ePDO specifyFetchColumn, int colNum)
		{
			var ret = new List<object>();

			while (Reader.Read())
			{
				var val = Reader.GetValue(colNum);
				ret.Add(val);
			}

			return ret;
		}

		public object fetchColumn(int column)
		{
			Reader.Read();
			return Reader.GetValue(column);
		}

		public int affectedRowCount() => (int)Reader.Rows;
		public bool hasRows() => Reader.HasRows;
	}
}
