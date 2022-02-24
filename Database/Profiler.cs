using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{
	/** Profiler that monitors performance of individual database queries and statements */
	public abstract class Profiler : Delight.Shim.Shimmed
	{

		/** Starts a new measurement */
		public abstract void beginMeasurement();

		/**
		 * Ends a previously started measurement
		 *
		 * @param string sql the SQL query or statement that was monitored
		 * @param array|null boundValues (optional) the values that have been bound to the query or statement
		 * @param int|null discardMostRecentTraceEntries (optional) the number of trace entries that should be discarded (starting with the most recent ones)
		 */
		public abstract void endMeasurement(string sql, BindValues boundValues = null, int? discardMostRecentTraceEntries = null);

		/**
		 * Returns the number of measurements that this profiler has recorded
		 *
		 * @return int
		 */
		public abstract int getCount();

		/**
		 * Returns the measurement at the specified index
		 *
		 * @param int index the index of the measurement to return
		 * @return Measurement
		 */
		public abstract Measurement getMeasurement(int index);

		/**
		 * Returns all measurements that this profiler has recorded
		 *
		 * @return Measurement[]
		 */
		public abstract Measurement[] getMeasurements();

		/** Sorts the measurements that this profiler has recorded so that the longest-running operations are listed first */
		public abstract void sort();

	}
}