using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFilesSmartCard
{
	/// <summary>
	/// Exception that represents failure in the signing process.
	/// </summary>
	public class SignatureVerificationFailedException : ApplicationException
	{
		/// <summary>
		/// Initializes new SignatureVerificationFailedException object.
		/// </summary>
		/// <param name="message">Message</param>
		public SignatureVerificationFailedException( string message ) : base( message )
		{
			
		}
	}
}
