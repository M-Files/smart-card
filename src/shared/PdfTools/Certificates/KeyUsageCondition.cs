using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Condition that specifies key usage.
	/// </summary>
	public class KeyUsageCondition : CertificateConditionBase
	{
		/// <summary>
		/// Initializes new key usage condition.
		/// </summary>
		/// <param name="keyUsage">The intended key usage of the certificate</param>
		public KeyUsageCondition( X509KeyUsageFlags keyUsage ) : base( X509FindType.FindByKeyUsage )
		{
			this.FindValue = keyUsage;
		}				

		public override void NoMoreCertificates()
		{
			throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoCertificatesWithKeyUsage );
		}
	}
}
