using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{

	/** Description of a data source */
	public abstract class DataSource : Delight.Shim.Shimmed_PHPOnly
	{

		/**
		 * Converts this instance to a DSN
		 *
		 * @return Dsn
		 */
		//public abstract Dsn toDsn();

	}

}