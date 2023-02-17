using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Text.RegularExpressions;
using MFiles.PdfTools.Certificates;
using Pdftools.Pdf;
using Pdftools.PdfSecure;

namespace MFiles.PdfTools
{
	/// <summary>
	/// Class for managing PDF security.
	/// </summary>
	public class PdfSecurity : IDisposable
	{
		/// <summary>
		/// The PDF that we are manipulating.
		/// </summary>
		private Secure Pdf { get; set; }

		/// <summary>
		/// List of signatures that needed to disposed.
		/// </summary>
		private readonly List< Signature > signaturesForCleanup = new List< Signature >();

		/// <summary>
		/// Crypto provider. Defaults to Microsoft's Crypto API. See the PDF Tools documentation for more information.
		/// </summary>
		/// <remarks>
		/// The provider can either be Microsoft’s Crypt API or a library that implements PKCS#11 to support HSM, USB tokens and smart cards.
		/// 
		/// When using the Microsoft's Crypt API, the value of this property should conform to following syntax:
		/// [ProviderType:]Provider[;PIN]
		/// Examples: 
		/// Provider = “Microsoft Base Cryptographic Provider v1.0” (Default on Windows XP and Windows 2003 Server)
		/// Provider = “Microsoft Strong Cryptographic Provider”
		/// Provider = “PROV_RSA_AES:Microsoft Enhanced RSA and AES Cryptographic Provider” (Default on Vista and up)
		/// </remarks>
		public string Provider { get; set; }

		/// <summary>
		/// Initializes tester.
		/// </summary>
		/// <param name="pdf"></param>
		public PdfSecurity( string pdf ) : this( pdf, null )
		{
			// Delegates.
		}

		/// <summary>
		/// Initializes tester.
		/// </summary>
		/// <param name="pdf">Path to the PDF document</param>
		/// <param name="provider">Security provider for managing the signatures.</param>
		public PdfSecurity( string pdf, string provider )
		{
			ConfigurationManager.Initialize();
			this.Pdf = new Secure();
			this.Pdf.NoCache = true;
			// "SHA-256" is default value for MessageDigestAlgorithm, according to 3-Heights PDF Security API documentation.
			// e.g. "SHA-384" could also be used.
			// TODO: Test this MessageDigestAlgoritm change and have this as configuration.
			// this.Pdf.SetSessionPropertyString( "MessageDigestAlgorithm", "SHA-256" );
			this.HandleError( this.Pdf.Open( pdf, null ) );
			this.Provider = provider;
		}

		/// <summary>
		/// Signs the specified PDF with the given certificate if the PDF has not been already signed.
		/// </summary>
		/// <remarks>This method allows duplicate signatures with the same certificate.</remarks>
		/// <param name="certificate">Specifies the certificate in the certificate store used to sign the document.</param>
		/// <param name="reason">Reason for the signature. Why was the object signed.</param>
		/// <param name="accessCode">Possible access code for the certifcate. E.g. a PIN code of a smart card.</param>
		public bool Sign( X509Certificate2 certificate, string reason, string accessCode = null )
		{
			// Delegate.
			this.Sign( certificate, reason, true, true, accessCode );
			return true;
		}

		/// <summary>
		/// Signs the specified PDF with the given certificate
		/// </summary>
		/// <param name="certificate">Specifies the certificate in the certificate store used to sign the document.</param>
		/// <param name="reason">Reason for the signature. Why was the object signed.</param>
		/// <param name="storeContactInfo">True to store the contact information.</param>
		/// <param name="allowDuplicates">True to allow duplicate signatures.</param>
		/// <param name="accessCode">Possible access code for the certifcate. E.g. a PIN code of a smart card.</param>
		public void Sign( X509Certificate2 certificate, string reason, bool storeContactInfo, bool allowDuplicates, string accessCode = null )
		{
			// Delegate.
			this.Sign( certificate, reason, storeContactInfo ? certificate.Rfc822Name() : null, allowDuplicates, accessCode );
		}

		/// <summary>
		/// Signs the specified PDF with the given certificate
		/// </summary>
		/// <param name="certificate">Specifies the certificate in the certificate store used to sign the document.</param>
		/// <param name="reason">Reason for the signature. Why was the object signed.</param>
		/// <param name="contactInfo">Contact information for the signature..</param>
		/// <param name="allowDuplicates">True to allow duplicate signatures.</param>
		/// <param name="accessCode">Possible access code for the certifcate. E.g. a PIN code of a smart card.</param>
		public void Sign( X509Certificate2 certificate, string reason, string contactInfo, bool allowDuplicates, string accessCode = null )
		{
			// Check if the PDF has been already signed.
			if( ! allowDuplicates && GetExistingSignatures( certificate ).Any() )
				throw new DuplicateSignatureException();

			// Initialize the signature.
			var signature = new Signature();
			signature.Provider = this.ResolveProvider();
			this.signaturesForCleanup.Add( signature );

			// Set parameters for the signature.
			signature.SignerFingerprintStr = certificate.Thumbprint;
			signature.Rect = PDFRect.None;
			if( !string.IsNullOrEmpty( reason ) )
				signature.Reason = reason;
			if( !string.IsNullOrEmpty( contactInfo ) )
				signature.ContactInfo = contactInfo;
			if( !string.IsNullOrEmpty( accessCode ) )
				signature.Provider += string.Format( ",{0}", accessCode );

			// Assign the signature.
			this.HandleError( this.Pdf.AddSignature( signature ) );
		}

		/// <summary>
		/// Saves the PDF.
		/// </summary>
		/// <param name="path">Target path.</param>
		public void SaveAs( string path )
		{
			// Saves the PDF without additional encryption enabled.
			var targetFile = new FileInfo( path );
			this.HandleError( this.Pdf.SaveAs( targetFile.FullName, null, null, PDFPermission.ePermNoEncryption, 0, null, null ) );				
		}

		/// <summary>
		/// Checks if the signature has a 
		/// </summary>
		/// <param name="commonNameOfIssuer">The common name of the isser</param>
		/// <param name="expectedReason">Expected reason for the signing.</param>
		/// <param name="rfc822Name">RFC-822 name of the user.</param>
		/// <param name="regularExpression">True if the compatible RFC-822 is a regular expression.</param>
		/// <returns>Validation result</returns>
		public ValidationResult ValidateSignature( string commonNameOfIssuer, string expectedReason, string rfc822Name, bool regularExpression )
		{
			// Iterate over the signatures.
			int signatures = this.Pdf.SignatureCount;
			var result = new ValidationResult();
			result.Signatures = signatures;
			int validSignatures = 0;
			for( int i = 0; i < signatures; i++ )
			{
				// Read the signature from PDF.
				var existingSignature = this.Pdf.GetSignature( i );
				existingSignature.Provider = this.ResolveProvider();
				
				// Validate the integrity of the PDF.
				// The trust is also validate here if the PDF Tools supports it with selected security provider supports it.
				//
				// Note: The trust is not validated with Windows CryptoAPI which is the default provider.
				// PDF Tools supports trust validation only with providers that implement the PKCS#11 standard.
				bool valid = this.Pdf.ValidateSignature( existingSignature );
				if( !valid )
					continue;

				// Validate issuer.
				if( !existingSignature.Issuer.Equals( commonNameOfIssuer, StringComparison.InvariantCulture ) )
					continue;
				validSignatures++;
				
				// Skip if the reason does not exist.
				if( string.IsNullOrEmpty( existingSignature.Reason ) )
					continue;

				// Skip if the RFC-822 does not exist.
				if( string.IsNullOrEmpty( existingSignature.ContactInfo ) && ! string.IsNullOrEmpty( rfc822Name ) )
					continue;

				// Validate reason?
				bool validReason = existingSignature.Reason.Equals( expectedReason );

				// Compare Text1 to the specified RFC-822 name. Only include valid certificates.
				string contactInfo = "";
				if( ! string.IsNullOrEmpty( existingSignature.ContactInfo ) )
					contactInfo = existingSignature.ContactInfo.ToLowerInvariant();

				bool validContactInfo = false;
				if( string.IsNullOrEmpty( rfc822Name ) )
				{
					// RFC822 configured to empty, do not validate certificate info. Accept as is.
					validContactInfo = true;
				}
				else if( regularExpression )
				{
					// Does the contact information match the specified name as a regular expression?
					var regex = new Regex( rfc822Name );
					if( regex.IsMatch( contactInfo ) )
						validContactInfo = true;
				}
				else if( contactInfo.Equals( rfc822Name ) )
				{
					// Found a signature with a valid contact infor.
					validContactInfo = true;
				}
				else
				{
					// The contact information does not match.
				}

				// Signature valid?
				if( validReason && validContactInfo )
					result.IsValid = true;

			}  // end for.
			result.ValidSignatures = validSignatures;

			// The signature was not found.
			return result;
		}

		/// <summary>
		/// Closes the PDF.
		/// </summary>
		public void Close()
		{
			// Delegate.
			this.HandleError( this.Pdf.Close() );
		}

		public void Dispose()
		{
			// Clear signatures.
			foreach( var signature in this.signaturesForCleanup )
			{
				signature.Dispose();
			}
			this.signaturesForCleanup.Clear();

			if( this.Pdf != null )
			{
				this.Pdf.Close();
				this.Pdf.Dispose();
			}
		}

		/// <summary>
		/// Gets existing signatures matching the given certificate.
		/// </summary>
		/// <param name="certificate">Certificate</param>
		/// <returns>Matching signatures.</returns>
		private IEnumerable< Signature > GetExistingSignatures( X509Certificate2 certificate )
		{
			// Iterate over the signatures.
			int signatures = this.Pdf.SignatureCount;
			for( int i = 0; i < signatures; i++ )
			{
				// Read the signature from PDF.
				var existingSignature = this.Pdf.GetSignature( i );
				existingSignature.Provider = this.ResolveProvider();
				bool valid = this.Pdf.ValidateSignature( existingSignature );

				// Compare fingerprint. Only include valid certificates.
				string normalizedFingerprint = existingSignature.SignerFingerprintStr.Replace( " ", "" ).ToUpperInvariant();
				if( valid && certificate.Thumbprint.Equals( normalizedFingerprint ) )
				{
					yield return existingSignature;
				}

			}  // end for.
		}

		/// <summary>
		/// Handlers errors after calling Secure object.
		/// </summary>
		/// <param name="result">Result of the operation.</param>
		private void HandleError( bool result )
		{
			// No need to handle errors on success.
			if( result )
				return;

			// Throw appropriate exception.
			if( this.Pdf.ErrorCode == PDFErrorCode.SIG_CREA_E_PRIVKEY )
				throw new PrivateKeyNotAvailableException( this.Pdf );
			else
				throw new PdfToolsException( this.Pdf );
		}

		/// <summary>
		/// Resolves the provider for use.
		/// </summary>
		/// <returns>Provider to be used.</returns>
		private string ResolveProvider()
		{
			// Use explicitly specified provider?
			if( !string.IsNullOrEmpty( this.Provider ) )
				return this.Provider;

			// Determine the provider based on the platform.
			if( System.Environment.OSVersion.Version.Major >= 6 )
			{
				// Stronger CryptoAPI provider available. Use it.
				return "PROV_RSA_AES:Microsoft Enhanced RSA and AES Cryptographic Provider";
			}
			else
			{
				// PDF Tools defaults to Microsoft Base Cryptographic Provider v1.0.
				return null;
			}
		}
	}
}
