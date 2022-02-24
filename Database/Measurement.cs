using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{

	/** Individual measurement of a profiler that monitors performance */
	public abstract class Measurement : Delight.Shim.Shimmed
	{

		/**
		 * Returns the duration of the operation that was monitored
		 *
		 * @return float the duration in milliseconds
		 */
		public abstract double getDuration();

		/**
		 * Returns the (parameterized) SQL query or statement that was used during the operation
		 *
		 * @return string
		 */
		public abstract string getSql();

		/**
		 * Returns any values that might have been bound to the statement
		 *
		 * @return array|null
		 */
		public abstract List<object> getBoundValues();

		/**
		 * Returns the trace that shows the path taken through the application until the operation was executed
		 *
		 * @return array
		 */
		public abstract List<Shim.Shimmed.debug_backtrace_param> getTrace();

	}
}