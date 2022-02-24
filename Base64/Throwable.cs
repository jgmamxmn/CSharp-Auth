using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Delight
{
	public sealed partial class Base64
	{
		public abstract class Error : Exception { }
		/// <summary>
		/// Error that is thrown when an attempt is being made to encode illegal input
		/// </summary>
		public class EncodingError : Error { }
		/// <summary>
		/// Error that is thrown when an attempt is being made to decode illegal input
		/// </summary>
		public class DecodingError : Error { }
	}
}
