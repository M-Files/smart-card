using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Condition for searching certificates based on Rfc822 name or e-mail address.
	/// </summary>
	public class Rfc822NameCondition : CertificateConditionBase
	{
		/// <summary>
		/// Certificate extenstion that represents the email.
		/// </summary>
		public const string SubjectAlternativeName = "2.5.29.17";		

		/// <summary>
		/// Regular expressions for detecting correct email.
		/// </summary>
		public Regex EmailAddressMatcher { get; private set; }

		/// <summary>
		/// Temp storage for the regular exp.
		/// </summary>
		private string RegularExpression { get; set; }

		/// <summary>
		/// Initializes search condition based on the email address of the user.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		public Rfc822NameCondition( string emailAddress ) : base( X509FindType.FindByExtension )
		{
			// We can only search for certificates that contain the specific extension.
			this.FindValue = Rfc822NameCondition.SubjectAlternativeName;
			this.RegularExpression = string.Format( "^{0}$", Regex.Escape( emailAddress ) );
			this.EmailAddressMatcher = new Regex( this.RegularExpression );
		}

		/// <summary>
		/// Initializes search condition based on the email address of the user.
		/// </summary>
		/// <param name="emailAddress">The email address.</param>
		public Rfc822NameCondition( Regex emailAddress, string expressionAsString )
			: base( X509FindType.FindByExtension )
		{
			// We can only search for certificates that contain the specific extension.
			this.FindValue = Rfc822NameCondition.SubjectAlternativeName;
			this.RegularExpression = expressionAsString;
			this.EmailAddressMatcher = emailAddress;
		}

		/// <summary>
		/// Checks if this condition matches the given certificate
		/// </summary>
		/// <param name="certificate">The certificate to evaluate</param>
		/// <returns>True if this certificate matches the condition.</returns>
		public override bool Match( X509Certificate2 certificate )
		{
			// Did we find the correct email?
			var email = certificate.Rfc822Name();
			if( string.IsNullOrEmpty( email ) )
			{
				string logFileDir = @"C:\MFilesSmartCardLogs\";
				string message = "Could not find email address from certificate.";
				if( System.IO.Directory.Exists( logFileDir ) )
					System.IO.File.WriteAllText( string.Format( @"{0}erroremail_{1}.log", logFileDir, DateTime.Now.ToString( "yyyyMMddHHmmss" ) ), message );

				return false;
			}
			
			bool match = this.EmailAddressMatcher.IsMatch( email );

			// Write log file if failed to match email.
			if( match == false )
			{
				string logFileDir = @"C:\MFilesSmartCardLogs\";
				string message = string.Format( "Failed to match email \"{0}\" to regular expression \"{1}\".", email, this.RegularExpression );
				if( System.IO.Directory.Exists( logFileDir ) )
					System.IO.File.WriteAllText( string.Format( @"{0}erroremail_{1}.log", logFileDir, DateTime.Now.ToString( "yyyyMMddHHmmss" ) ), message );
			}

			return match;
		}

		/// <summary>
		/// Does the evaluation of this condition require matching the certificates one-by-one?
		/// </summary>
		public override bool MatchingRequired { get { return true; } }

		public override void NoMoreCertificates()
		{
			throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoCertificatesWithEmailAddress );
		}
	}
}
