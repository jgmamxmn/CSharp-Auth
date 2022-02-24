using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{
	/** Implementation of an individual measurement of a profiler that monitors performance */
	sealed public class SimpleMeasurement : Measurement
	{

		/** @var float the duration in milliseconds */
		private double duration;
		/** @var string the SQL query or statement */
		private string sql;
		/** @var array|null the values that have been bound to the query or statement */
		private BindValues boundValues;
		/** @var array|null the trace that shows the path taken through the program until the operation was executed */
		private List<Shim.Shimmed.debug_backtrace_param> trace;

		/**
		 * Constructor
		 *
		 * @param float duration the duration in milliseconds
		 * @param string sql the SQL query or statement
		 * @param array|null boundValues (optional) the values that have been bound to the query or statement
		 * @param array|null trace (optional) the trace that shows the path taken through the program until the operation was executed
		 */
		public SimpleMeasurement(double _duration, string _sql, BindValues _boundValues = null, List<Shim.Shimmed.debug_backtrace_param> _trace = null) 
		{
			this.duration = _duration;
			this.sql = _sql;
			this.boundValues = _boundValues;
			this.trace = _trace;
		}

		override public double getDuration() {
			return this.duration;
		}

		override public string getSql() {
			return this.sql;
		}

		override public List<object> getBoundValues() {
			return this.boundValues.Values.ToList();
		}

		override public List<Shim.Shimmed.debug_backtrace_param> getTrace() {
			return this.trace.ToList();
		}

	}
}