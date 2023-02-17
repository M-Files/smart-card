using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools
{
	/// <summary>
	/// Exception which is thrown when duplicate signature is detected when signing.
	/// </summary>
	public class DuplicateSignatureException : ApplicationException
	{
		/// <summary>
		/// Initializes new exception.
		/// </summary>
		public DuplicateSignatureException() : base(  InternalResources.Error_PdfTools_DuplicateSignature )
		{
			
		}
	}
}
