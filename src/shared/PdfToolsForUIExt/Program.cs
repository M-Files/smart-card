using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MFiles.PdfTools.Certificates;
using MFiles.PdfTools.UIExt.Exceptions;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Application class.
	/// </summary>
	internal class Program
	{
		/// <summary>
		/// The location of the input file in the parameters.
		/// </summary>
		private const int InputFileArg = 0;

		/// <summary>
		/// The location of the encoded configuration in the parameters.
		/// </summary>
		private const int EncodedConfigurationArg = 1;

		/// <summary>
		/// The location of the reason for the signature in the parameters.
		/// </summary>
		private const int Reason = 2;

		/// <summary>
		/// The location of the account name in the parameters.
		/// </summary>
		private const int AccountNameArg = 3;

		/// <summary>
		/// The Ready file which is written after the signing has completed.
		/// </summary>
		private const int ReadyFileArg = 4;

		/// <summary>
		/// The number of mandatory paramaters.
		/// </summary>
		private const int MandatoryParameterCount = 5;

		/// <summary>
		/// The optional license key in the parameters.
		/// </summary>
		private const int OptionalLicenseKeyArg = MandatoryParameterCount;

		[STAThread]
		static int Main( string[] args )
		{
			// Debugging aid.
			// System.Diagnostics.Debugger.Launch();
			// System.Diagnostics.Debugger.Break();

			// Report version.
			int errorCode;
			Exception error = null;
			try
			{
				// Initialize
				ConfigurationManager.Initialize();

				// Check that we have minimum arguments were given.
				if( args.Length < MandatoryParameterCount )
					throw new ArgumentException( "Missing arguments: <input PDF> <configuration> <reason> <account name> <ready file>" );

				// Verify that input file exists.
				string inputFile = args[ InputFileArg ];
				if( !File.Exists( inputFile ) )
					throw new FileNotFoundException( "The given input file does not exist.", inputFile );

				// Parse configuration.
				// To avoid problems with command line arguments are transmitted as compressed base64 encoded string.
				string encodedConfiguration = args[ EncodedConfigurationArg ];
				string jsonConfiguration = Helper.InflateBase64( encodedConfiguration );
				var config = MFiles.PdfTools.UIExt.Configuration.Configuration.Parse( jsonConfiguration );

				// Get license key from parameter or configuration.
				if( args.Length > MandatoryParameterCount )
					Licensing.LicenseKey = args[ OptionalLicenseKeyArg ];
				else if( !string.IsNullOrEmpty( config.PdfTools.LicenseKey ) )
					Licensing.LicenseKey = Licensing.LicenseKey = config.PdfTools.LicenseKey;

				// Set default license if the license is still not set.
				if( string.IsNullOrEmpty( Licensing.LicenseKey ) )
					Licensing.LicenseKey = Licensing.OemLicenseKey;

				// Ensure PDF tools has a valid license.
				if( !Licensing.IsLicenseValid )
					throw new InvalidLicenseException();

				// Get the account name.
				string accountName = args[ AccountNameArg ];

				// Determine the object version that is signed.
				// This is stored in the Reason field of the certificate and will be used later in verifying the signature.
				var reason = args[ Reason ];

				// Get certificate configuration and crypto provider.
				var certificateConfigs = config.Authentication.Certificates;
				var cryptoProvider = config.PdfTools.Provider;

				// Create placeholders for searching for the certificate.
				var placeholders = new Dictionary<string, string>();
				placeholders.Add( "accountName", accountName );
				placeholders.Add( "username", GetUsername( accountName ) );
				string domain = GetDomain( accountName );
				if( !string.IsNullOrEmpty( domain ) )
					placeholders.Add( "domain", domain );

				// Ensure that IssuedBy was correctly specified for all the configs.
				foreach( var certificateConfig in certificateConfigs )
					if( string.IsNullOrEmpty( certificateConfig.IssuedBy ) )
						throw new IssuedByNotSpecifiedException();

				// Open certificate store.
				using( var store = new CertificateStore() )
				{
					// Use the first matching certificate config to get a certificate from the store.
					X509Certificate2 certificate = null;
					Configuration.Certificate certificateConfig = null;
					foreach( var certificateConfigCandidate in certificateConfigs )
					{
						// Try getting a certificate from the store.
						try
						{
							// Get certificate using this config.
							certificate = FindCertificate( certificateConfigCandidate, placeholders, store );
							certificateConfig = certificateConfigCandidate;

							// Got certificate successfully. Stop searching.
							break;
						}
						catch( CertificateNotFoundException ex )
						{
							// Report the last exception.
							error = ex;

						}  // end try.
						
					}  // end foreach.

					// Sign if we found a certificate.
					if( certificate != null )
					{
						// Try to sign the input file.
						Sign( inputFile, reason, cryptoProvider, certificateConfig, certificate );

						// Remove the certificate from the key store?
						if( certificateConfig.RemoveCertificateFromKeyStoreAfterUsage )
							store.RemoveCertificate( certificate );

						// Successfully signed.
						error = null;
						errorCode = ApplicationErrorCodes.Success;

					}
					else
					{
						// Certificate was not found. The last exception is reported.
						errorCode = ApplicationErrorCodes.CertificateNotFound;

					}  // end if.

				}  // end using.

			}
			catch( ArgumentException ex )
			{
				error = ex;
				Program.ReportExceptionToConsole( ex );
				errorCode = ApplicationErrorCodes.InvalidArguments;
			}
			catch( FileNotFoundException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.FileToBeSignedNotFound;
			}
			catch( DuplicateSignatureException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.DuplicateSignature;
			}
			catch( PrivateKeyNotAvailableException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.PrivateKeyNotFound;
			}
			catch( InvalidLicenseException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.LicenseIsInvalid;
			}
			catch( IssuedByNotSpecifiedException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.IssuedByNotSpecified;
			}
			catch( NewObjectVersionException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.NewVersionExists;
			}
			catch( PdfToolsException ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.PdfToolsError;
			}
			catch( Exception ex )
			{
				error = ex;
				errorCode = ApplicationErrorCodes.GeneralError;

			}  // end try

			// Process error.
			if( error != null )
				Program.ReportExceptionToConsole( error );

			// Write the ready file.
			using( var sw = new StreamWriter( File.Open( args[ ReadyFileArg ], FileMode.Create, FileAccess.ReadWrite, FileShare.None ) ) )
			{
				// Error code.
				sw.WriteLine( errorCode );
				if( error != null )
				{
					// Actual error message.
					sw.WriteLine( error.Message );
					sw.WriteLine( error.ToString() );
				}

			}  // end using.

			// Uninitialize
			ConfigurationManager.Uninitialize();

			// Return.
			return errorCode;
		}

		/// <summary>
		/// Signs the file with the given certificate.
		/// </summary>
		/// <param name="inputFile">The file path for the input file.</param>
		/// <param name="reason">Reason is a free description added to the signature.</param>
		/// <param name="cryptoProvider">Security provider for managing the signatures.</param>
		/// <param name="certificateConfig">Configuration for the certificate.</param>
		/// <param name="certificate">Certificate used for signing.</param>
		private static void Sign( string inputFile, string reason, string cryptoProvider, Configuration.Certificate certificateConfig, X509Certificate2 certificate )
		{
			// Get temp file path for signed file.
			string outputFile = Path.GetTempFileName();
			try
			{
				// Sign.
				using( var signer = new PdfSecurity( inputFile, cryptoProvider ) )
				{
					signer.Sign( certificate, reason, true, certificateConfig.AllowDuplicates );
					signer.SaveAs( outputFile );
				}

				// Replace original with the signed file.
				File.Copy( outputFile, inputFile, true );
			}
			finally
			{
				// Try to cleanup the output file.
				try
				{
					if( File.Exists( outputFile ) )
						File.Delete( outputFile );
				}
				catch {}

			}  // end try.
		}

		/// <summary>
		/// Finds the single certificate matching the configuration.
		/// </summary>
		/// <param name="certificateConfig">Configuration for the certificate.</param>
		/// <param name="placeholders">Placeholders for filtering by subject.</param>
		/// <param name="store">Certificate store.</param>
		/// <returns>Matching certificate from the Certificate store.</returns>
		private static X509Certificate2 FindCertificate( Configuration.Certificate certificateConfig, Dictionary<string, string> placeholders, CertificateStore store )
		{
			// Find appropriate certificate. No not filter by SubjectName if it's empty in configuration.
			X509Certificate2 certificate;
			if( string.IsNullOrEmpty( certificateConfig.SubjectRfc822Name.Value ) )
			{
				certificate = store.FindCertificates()
									.IssuedBy( certificateConfig.IssuedBy )
									.KeyUsage( certificateConfig.KeyUsage )
									.TimeValid().Single();
			}
			else
			{
				certificate = store.FindCertificates()
									.IssuedBy( certificateConfig.IssuedBy )
									.KeyUsage( certificateConfig.KeyUsage )
									.Condition( certificateConfig.SubjectRfc822Name.AsCondition( placeholders ) )
									.TimeValid().Single();
			}

			return certificate;
		}

		/// <summary>
		/// Gets the username from the account name.
		/// </summary>
		/// <param name="accountName">The account name</param>
		/// <returns>Username</returns>
		private static string GetUsername( string accountName )
		{
			// Extract the username from the account name.
			var domainSeparator = accountName.IndexOf( '\\' );
			string username;
			if( domainSeparator != -1 )
				username = accountName.Substring( domainSeparator + 1 );
			else
				username = accountName;

			// Return the username.
			return username;
		}

		/// <summary>
		/// Gets the domain from the account name.
		/// </summary>
		/// <param name="accountName">The account name</param>
		/// <returns>Username</returns>
		private static string GetDomain( string accountName )
		{
			// Extract the username from the account name.
			var domainSeparator = accountName.IndexOf( '\\' );
			string domain;
			if( domainSeparator != -1 )
				domain = accountName.Substring( 0, domainSeparator );
			else
				domain = null;

			// Return the username.
			return domain;
		}

		/// <summary>
		/// Reports exception to console.
		/// </summary>
		/// <param name="ex">Exception</param>
		private static void ReportExceptionToConsole( Exception ex )
		{
			// Write clean error message to standard output and the stack to error output.
			Console.WriteLine( ex.Message );
			Console.Error.WriteLine( ex.ToString() );
		}
	}
}
