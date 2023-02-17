using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.UIExt.Exceptions
{
	/// <summary>
	/// Exception which is thrown when a new object version of the signed object exists on the server.
	/// </summary>
	public class NewObjectVersionException : ApplicationException
	{
		/// <summary>
		/// Initializes new NewObjectVersionException object.
		/// </summary>
		public NewObjectVersionException()
			: base( InternalResources.Error_NewObjectVersionExists )
		{
			
		}
	}
}
