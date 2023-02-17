using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Interface for JSON value that can be validated during conversion. 
	/// 
	/// Possible validation errors are accessible via the interface.
	/// </summary>
	interface IValidatedValue
	{

		/// <summary>
		/// Is this value valid?
		/// </summary>
		bool IsValid { get;}

		/// <summary>
		/// Possible error message in case this value is not valid.
		///  </summary>
		string ErrorMessage { get; }
	}
}
