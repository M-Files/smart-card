using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Condition for filtering the certificates based on if they are valid at this moment.
	/// </summary>
	public class TimeValidCondition : CertificateConditionBase
	{
		/// <summary>
		/// Initializes new TimeValid condition.
		/// </summary>
		public TimeValidCondition() : base( X509FindType.FindByTimeValid )
		{
			this.FindValue = DateTime.Now;
		}

		public override void NoMoreCertificates()
		{
			throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoTimeValidCertificates );
		}
	}
}
