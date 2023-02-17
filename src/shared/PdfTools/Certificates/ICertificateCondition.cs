using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Interface that implements certificate condition.
	/// </summary>
	public interface ICertificateCondition
	{
		/// <summary>
		/// Find type of the condition.
		/// </summary>
		X509FindType FindType { get; }

		/// <summary>
		/// Value for the condition.
		/// </summary>
		object FindValue { get; }

		/// <summary>
		/// Checks if this condition matches the given certificate
		/// </summary>
		/// <param name="certificate">The certificate to evaluate</param>
		/// <returns>True if this certificate matches the condition.</returns>
		bool Match( X509Certificate2 certificate );

		/// <summary>
		/// Require certificate that is valid on this machine.
		/// </summary>
		bool Valid { get; }

		/// <summary>
		/// Called when no more certificates were found after processing this condition.
		/// </summary>
		/// <remarks>Condition implementers should throw an exception when called with descriptive error message.</remarks>
		void NoMoreCertificates();

		/// <summary>
		/// Does the evaluation of this condition require matching the certificates one-by-one?
		/// </summary>
		bool MatchingRequired { get; }
	}
}
