using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Represents a validation error in the conversion.
	/// </summary>
	public class ValidationError
	{
		/// <summary>
		/// Path to the error.
		/// </summary>
		public string Path { get; internal set; }

		/// <summary>
		/// Error message.
		/// </summary>
		public string ErrorMessage { get; internal set; }
	}
}
