using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pdftools.Pdf;
using Pdftools.PdfSecure;

namespace MFiles.PdfTools
{
	/// <summary>
	/// Exception that represents an erro caused by PDF Tools
	/// </summary>
	public class PdfToolsException : ApplicationException
	{
		/// <summary>
		/// Error code
		/// </summary>
		internal Pdftools.Pdf.PDFErrorCode ErrorCode { get; private set; }

		/// <summary>
		/// Initializes new Pdf Tools exception.
		/// </summary>
		/// <param name="pdf">Session after error condition.</param>
		internal PdfToolsException( Secure pdf )
			: base( GetLocalizedErrorMessage( pdf.ErrorCode, pdf.ErrorMessage ) )
		{
			this.ErrorCode = pdf.ErrorCode;
		}

		/// <summary>
		/// Initializes new Pdf Tools exception.
		/// </summary>
		/// <param name="errorMessage">Error message</param>
		public PdfToolsException( string errorMessage ) : base( errorMessage )
		{			
		}

		/// <summary>
		/// Gets localized error message for specific errors.
		/// </summary>
		/// <param name="errorCode">Error code</param>
		/// <param name="message">Default error message.</param>
		/// <returns>Localized error message.</returns>
		private static string GetLocalizedErrorMessage( Pdftools.Pdf.PDFErrorCode errorCode, string message )
		{
			// Get localized message.
			string localizedMessage;
			switch( errorCode )
			{
			// Private key not available error is commonly shown to the user if the 
			case PDFErrorCode.SIG_CREA_E_PRIVKEY:
				localizedMessage = InternalResources.Error_PdfTools_PrivateKeyNotAvailable;
				break;

			// By default user the original error message from the library.
			default:
				localizedMessage = message;
				break;
			}
			return localizedMessage;
		}
	}
}
