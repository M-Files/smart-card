using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Condition for searching certificates issued to specific user.
	/// </summary>
	public class SubjectCondition : CertificateConditionBase
	{
		/// <summary>
		/// Initializes new subject condition.
		/// </summary>
		/// <param name="subject">The user the certificate is issued to</param>
		public SubjectCondition( string subject )
			: base( X509FindType.FindBySubjectDistinguishedName )
		{
			this.FindValue = subject;
		}

		public override void NoMoreCertificates()
		{
			throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoCertificatesWithSubject );
		}		
	}
}
