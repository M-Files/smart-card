using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pdftools.PdfSecure;

namespace MFiles.PdfTools
{
	/// <summary>
	/// Expection that represents missing private key.
	/// 
	/// This exception is thrown the when user cancels the PIN prompt.
	/// </summary>
	public class PrivateKeyNotAvailableException : PdfToolsException
	{
		/// <summary>
		/// Initializes new PrivateKeyNotAvailableException object.
		/// </summary>
		/// <param name="pdf">Error source</param>
		internal PrivateKeyNotAvailableException( Secure pdf ) : base( pdf )
		{
		}

		/// <summary>
		/// Initializes new PrivateKeyNotAvailableException object.
		/// </summary>
		/// <param name="errorMessage">Error message</param>
		public PrivateKeyNotAvailableException( string errorMessage )
			: base( errorMessage )
		{			
		}
	}
}
