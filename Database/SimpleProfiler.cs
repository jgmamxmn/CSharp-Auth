using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Delight.Auth;

namespace Delight.Db
{
	/** Implementation of a profiler that monitors performance of individual database queries and statements */
	sealed public class SimpleProfiler : Profiler
	{

		/** The maximum number of entries in traces to use as the default */
		public const int TRACE_MAX_LENGTH_DEFAULT = 10;

		/** @var Measurement[] the measurements recorded by this instance */
		private List<Measurement> measurements;
		/** @var int the maximum number of entries in traces */
		private int maxTraceLength;
		/** @var float|null the start time of the current measurement in milliseconds */
		private double? currentMeasurementStartTime;

		public SimpleProfiler(int? _maxTraceLength = null) {
				this.measurements = new List<Measurement>();

			if (_maxTraceLength is int _iMaxTraceLength)
				this.maxTraceLength = _iMaxTraceLength;
			else
				this.maxTraceLength = TRACE_MAX_LENGTH_DEFAULT;

			this.currentMeasurementStartTime = null;
		}

		override public void beginMeasurement() 
		{
			this.currentMeasurementStartTime = microtime(true) * 1000;
		}

		override public void endMeasurement(string sql, BindValues boundValues = null, int? discardMostRecentTraceEntries = null) {

			discardMostRecentTraceEntries = discardMostRecentTraceEntries ?? 0;

			// get the trace at this point of the program execution
			var trace = debug_backtrace( debug_backtrace_opts.DEBUG_BACKTRACE_IGNORE_ARGS, this.maxTraceLength);

			// discard as many of the most recent entries as desired (but always discard at least the current method)
			for (var i = 0; i < discardMostRecentTraceEntries + 1; i++) {
				array_shift(trace);
			}

			// calculate the duration in milliseconds
			var duration = (microtime(true) * 1000) - this.currentMeasurementStartTime ?? 0;

			// and finally record the measurement
			this.measurements.Add(new SimpleMeasurement(
				duration,
				sql,
				boundValues,
				trace
			));
		}

		override public int getCount() {
			return count(this.measurements);
		}

		override public Measurement getMeasurement(int index) {
			return this.measurements[index];
		}

		public override Measurement[] getMeasurements()
		{
			return this.measurements.ToArray();
		}

		override public void sort() {
			measurements.Sort(new Comparison<Measurement>((a, b) =>
				/** @var Measurement a */
				/** @var Measurement b */
				(int)(b.getDuration() - a.getDuration())
			));
		}

	}
}