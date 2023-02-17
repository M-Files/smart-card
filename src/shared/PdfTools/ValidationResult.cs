using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools
{
	/// <summary>
	/// Represents the signature validation result.
	/// </summary>
	public class ValidationResult
	{
		/// <summary>
		/// The number of signatures found.
		/// </summary>
		public int Signatures { get; internal set; }

		/// <summary>
		/// The number of valid signatures found.
		/// </summary>
		public int ValidSignatures { get; internal set; }

		/// <summary>
		/// Was the overall validation a success.
		/// </summary>
		public bool IsValid { get; internal set; }
	}
}
