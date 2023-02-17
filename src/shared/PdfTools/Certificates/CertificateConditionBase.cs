using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Base class for certificate conditions.
	/// </summary>
	public abstract class CertificateConditionBase : ICertificateCondition
	{
		/// <summary>
		/// Initializes certificate condition base
		/// </summary>
		/// <param name="findType">Find type.</param>
		protected CertificateConditionBase( X509FindType findType )
		{
			this.FindType = findType;
		}

		/// <summary>
		/// Find type of the condition.
		/// </summary>
		public X509FindType FindType { get; private set; }

		/// <summary>
		/// Value for the condition.
		/// </summary>
		public object FindValue { get; protected set; }

		/// <summary>
		/// Checks if this condition matches the given certificate
		/// </summary>
		/// <param name="certificate">The certificate to evaluate</param>
		/// <returns>True if this certificate matches the condition.</returns>
		public virtual bool Match( X509Certificate2 certificate )
		{
			// Most of the conditions can be evaluated directly.
			throw new NotImplementedException();
		}

		/// <summary>
		/// Require certificate that is valid on this machine.
		/// </summary>
		public bool Valid { get; protected set; }

		/// <summary>
		/// Called when no more certificates were found after processing this condition.
		/// </summary>
		public abstract void NoMoreCertificates();

		/// <summary>
		/// Does the evaluation of this condition require matching the certificates one-by-one?
		/// </summary>
		public virtual bool MatchingRequired { get { return false; } }
	}
}
