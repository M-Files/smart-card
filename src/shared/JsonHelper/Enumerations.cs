using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	public static class Enumerations
	{
		/// <summary>
		/// The error reporting mode.
		/// </summary>
		public enum ErrorReporting
		{
			/// <summary>
			/// Errors in conversion are reported by throwing an exception.
			/// </summary>
			Exception,

			/// <summary>
			/// Errors are collected in the deserialized objects where possible.
			/// </summary>
			/// <remarks>Exception is thrown if collecting is not possible.</remarks>
			Collect
		}
	}
}
