using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Exception for alias resolution failures.
	/// </summary>
	public class AliasResolutionFailedException : ApplicationException
	{
		/// <summary>
		/// Initializes new AliasResolutionFailedException.
		/// </summary>
		/// <param name="message">Failures message</param>
		internal AliasResolutionFailedException( string message ) : base( message )
		{
			
		}
	}
}
