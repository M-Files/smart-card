using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Condition for filtering the certificates based on the thumbprint
	/// </summary>
	public class ThumbprintCondition : CertificateConditionBase
	{
		/// <summary>
		/// Initializes new ThumbprintCondition object.
		/// </summary>
		/// <param name="thumbprint">Thumbprint to look for.</param>
		public ThumbprintCondition( string thumbprint ) : base( X509FindType.FindByThumbprint )
		{
			// Strip extra characters from the thumbprint.
			// Especially when copying the thumbprint from the Certificates mmc snap-in there are
			// extra spaces and also there might be a hidden character at the beginning of the string.
			var normalizedThumbprint = Regex.Replace( thumbprint, @"[^\w\d]", "" );
			this.FindValue = normalizedThumbprint;
		}

		public override void NoMoreCertificates()
		{
			throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoCertificatesThumbprint );
		}
	}
}
