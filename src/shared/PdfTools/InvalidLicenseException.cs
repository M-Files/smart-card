using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools
{
	/// <summary>
	///  Expection which indicates an invalid license.
	/// </summary>
	public class InvalidLicenseException : ApplicationException
	{
		/// <summary>
		/// Initializes new InvalidLicenseException object.
		/// </summary>
		/// <param name="errorMessage">Error message</param>
		public InvalidLicenseException( string errorMessage ) : base( errorMessage )
		{
		}

		/// <summary>
		/// Initializes new InvalidLicenseException object with default error message.
		/// </summary>		
		public InvalidLicenseException() : base( InternalResources.Error_InvalidLicense )
		{
		}
	}
}
