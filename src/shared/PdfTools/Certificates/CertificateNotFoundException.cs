using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Expection that indicates that a certificate was not found.
	/// </summary>
	public class CertificateNotFoundException : ApplicationException
	{
		/// <summary>
		/// Initializes new CertificateNotFoundException object.
		/// </summary>
		/// <param name="message">Reason why the certificate was not found.</param>
		public CertificateNotFoundException( string message ) : base( message )
		{
			
		}

		/// <summary>
		/// Initializes new CertificateNotFoundException object.
		/// </summary>
		/// <param name="message">Reason why the certificate was not found.</param>
		/// <param name="ex">Inner exception</param>
		public CertificateNotFoundException( string message, Exception ex )
			: base( message, ex )
		{

		}
	}
}
