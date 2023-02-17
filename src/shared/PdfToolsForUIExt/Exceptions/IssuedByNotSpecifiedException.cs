using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.UIExt.Exceptions
{
	/// <summary>
	/// Expcetion thrown when issued by is not specified.
	/// </summary>
	public class IssuedByNotSpecifiedException : ApplicationException
	{
		/// <summary>
		/// Initializes new IssuedByNotSpecifiedException exception.
		/// </summary>
		public IssuedByNotSpecifiedException() : base( InternalResources.Error_Configuration_IssuedByMissing )
		{
			
		}
	}
}
