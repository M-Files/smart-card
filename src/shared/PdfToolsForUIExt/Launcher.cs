using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFiles.PdfTools.Certificates;
using MFiles.PdfTools.UIExt.Configuration;
using Newtonsoft.Json;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Manages the signing of the files.
	/// </summary>
	public class Launcher
	{
		/// <summary>
		/// Locale
		/// </summary>
		private string locale;

		/// <summary>
		/// M-Files version.
		/// </summary>
		private string mfilesVersion;

		/// <summary>
		/// PDF Tools configuration.
		/// </summary>
		private MFiles.PdfTools.UIExt.Configuration.PdfTools pdfTools;

		/// <summary>
		/// Locale for the resources.
		/// </summary>
		public string Locale
		{
			get { return this.locale; }
			set
			{
				// Store the locale
				this.locale = value;

				// Determine the culture based on the configuration.
				this.UiCulture = CultureHelper.ParseUICulture( this.locale, this.mfilesVersion );
			}
		}

		/// <summary>
		/// The current M-Files version in use.
		/// </summary>
		/// <remarks>Required for selecting the ui culture in case 'mfiles' has been specified as the source.</remarks>
		public string MFilesVersion
		{
			get { return this.mfilesVersion; }
			set
			{
				// Store the locale
				this.mfilesVersion = value;

				// Determine the culture based on the configuration.
				this.UiCulture = CultureHelper.ParseUICulture( this.locale, this.mfilesVersion );
			}
		}

		/// <summary>
		/// Culture for the operation.
		/// </summary>
		public string UiCulture { get; set; }

		/// <summary>
		/// Configuration for PDF Tools.
		/// </summary>
		public string PdfTools
		{
			get { return JsonConvert.SerializeObject( this.pdfTools ); }
			set { this.pdfTools = MFiles.PdfTools.UIExt.Configuration.PdfTools.Parse( value ); }
		}

		private ErrorObject errorObject = null;

		/// <summary>
		/// Error message.
		/// </summary>
		public string ErrorMessage 
		{
			get { return errorObject != null ? errorObject.ErrorMessage : null; }
		}

		/// <summary>
		/// Error stack.
		/// </summary>
		public string ErrorStack
		{
			get { return errorObject != null ? errorObject.ErrorStack : null; }
		}

		/// <summary>
		/// Creates an evidence pdf for the document and signs it with smartcard. Shows a progress bar throughout the process.
		/// </summary>
		/// <param name="ownerHandle">UI handle to the owner window.</param>
		/// <param name="vault">The M-Files Vault object.</param>
		/// <param name="document">The object version of the document for which the evidence pdf is created.</param>
		/// <param name="authenticate">Configuration for authentication.</param>
		/// <param name="accountName">The account name of the user signing the document.</param>
		/// <param name="createEvidencePDF">True to create evidence PDF and sign that. False to sign this document</param>
		/// <returns>Zero if successful. If not successful, error information can be read from ErrorMessage and ErrorStack.</returns>
		public int SignPDF( object ownerHandle, dynamic vault, dynamic document, string authenticate, string accountName, bool createEvidencePDF )
		{
			// Debug aid.
			// System.Diagnostics.Debugger.Launch();
			// System.Diagnostics.Debugger.Break();
			
			// Set proper culture. This is required for properly localized error messages when running in separate task.
			using( TemporaryCulture temporaryCulture = createTemporaryCulture( this.UiCulture ) )
			{
				// Clear the previous error object.
				this.errorObject = null;

				try
				{
					// Create encoded configuration for signing in separate process.
					string encodedConfiguration = createEncodedConfigurationForSigning( this.Locale, this.pdfTools, authenticate );

					// Create evidence object and sign or sign this PDF.
					this.errorObject = SignPDFWithProgressBar( ( IntPtr )( int )ownerHandle, vault, document, encodedConfiguration, accountName, this.UiCulture, createEvidencePDF );
				}
				catch( Exception e )
				{
					this.errorObject = ErrorObject.FromException( e );
				}

				// Return the error code.
				if( this.errorObject != null )
				{
					// Write log file if log folder exists.
					string logFileDir = @"C:\MFilesSmartCardLogs\";
					string message = string.Format( "{0}\n{1}\n(M-Files {2})", this.ErrorMessage, this.ErrorStack, this.MFilesVersion );
					if( System.IO.Directory.Exists( logFileDir ) )
						System.IO.File.WriteAllText( string.Format( @"{0}errorstack_{1}.log", logFileDir, DateTime.Now.ToString( "yyyyMMddHHmmss" ) ), message );

					return this.errorObject.ErrorCode;
				}
				return ApplicationErrorCodes.Success;
			}
		}

		/// <summary>
		/// Creates encoded configuration for signing in separate process.
		/// </summary>
		private static string createEncodedConfigurationForSigning( string locale, Configuration.PdfTools pdfTools, string authenticate )
		{
			// Construct full configuration.
			var fullConfiguration = new MFiles.PdfTools.UIExt.Configuration.Configuration
			{
				Locale = locale,
				PdfTools = pdfTools,
				Authentication = Authentication.Parse( authenticate )
			};

			// To avoid problems with command line arguments compress them to save space and then use base64 encoding to avoid character conversion problems.
			return Helper.DeflateBase64( fullConfiguration.Serialize() );
		}

		/// <summary>
		/// Creates a temporary culture with the given ui culture.
		/// </summary>
		private static TemporaryCulture createTemporaryCulture( string uiCulture )
		{
			return new TemporaryCulture( null, string.IsNullOrEmpty( uiCulture ) ? null : uiCulture );
		}

		/// <summary>
		/// Creates an evidence pdf for the document and signs it with smartcard. Shows a progress bar throughout the process.
		/// </summary>
		/// <param name="ownerHandle">UI handle to the owner window.</param>
		/// <param name="vault">The M-Files Vault object.</param>
		/// <param name="document">The object version of the document for which the evidence pdf is created.</param>
		/// <param name="encodedConfiguration">Encoded configuration for signing in separate process.</param>
		/// <param name="accountName">The account name of the user signing the document.</param>
		/// <param name="uiCulture">The ui culture for the separate task.</param>
		/// <returns>Error object from the separate task or the separate process.</returns>
		private static ErrorObject SignPDFWithProgressBar( IntPtr ownerHandle, dynamic vault, dynamic document, string encodedConfiguration, string accountName, string uiCulture, bool createEvidencePDF )
		{
			// Show the progress bar during the signature process.
			ErrorObject error = null;
			using( Progress progressBarDialog = new Progress( GetDocumentName( document ) ) )
			{
				EvidencePdf evidencePdf = null;
				try
				{
					// Get latest checked in objver for the selected document.
					dynamic latestCheckedInObjVer = Helper.GetLatestCheckedInObjVer( document );

					// Start the background processing after the dialog has been shown.
					// The dialog must be running the message loop before we start because we need to handle M-Files API objects calls
					// within that message loop.
					Task<ErrorObject> pdfSigningTask = null;
					bool signingStarted = false;
					Task finalizationTask = null;
					progressBarDialog.Shown += delegate( Object o, EventArgs ev )
					{
						try
						{
							// Ignore if already started.
							if( signingStarted )
								return;
							signingStarted = true;

							// Create the evidence PDF.
							progressBarDialog.SetPhase( Progress.ProgressPhase.CreateEvidencePDF );
							evidencePdf = EvidencePdf.CreateFor( vault, latestCheckedInObjVer, createEvidencePDF );
							Application.DoEvents();

							if( evidencePdf.Path.ToLowerInvariant().EndsWith( "pdf" ) == false )
								throw new Exception( InternalResources.Error_NotInPDFFormat );

							// Sign the evidence pdf asynchronously in a separate task to show the progress bar.
							pdfSigningTask = Task.Factory.StartNew<ErrorObject>( () => SignEvidencePDF( evidencePdf.Path, encodedConfiguration, evidencePdf.DocumentObjectVersionAsString, accountName, uiCulture, progressBarDialog ) );

							// Finalize the evidence.
							finalizationTask = pdfSigningTask.ContinueWith( ( copmletedEvidencePdfTask ) => evidencePdf.Finalize( copmletedEvidencePdfTask, progressBarDialog, createEvidencePDF ) );

							// Close the progress dialog, when done.
							finalizationTask.ContinueWith( ( signedEvidencePdfObjectVersion ) => progressBarDialog.CloseFromAnyThread() );
						}
						catch( Exception ex )
						{
							// Capture error and close the dialog.
							error = ErrorObject.FromException( ex );
							progressBarDialog.CloseFromAnyThread();
						}
					};

					// Get the owner control from the owner handle.
					Control ownerControl = Control.FromHandle( ownerHandle );

					// Keep showing the dialog, if the user closes until the signing process is complete.
					while( ( finalizationTask == null || finalizationTask.IsCompleted == false ) && error == null )
						progressBarDialog.ShowDialog( ownerControl );

					// Return the error object, if there was a problem with the task.
					if( pdfSigningTask != null && pdfSigningTask.IsFaulted )
						error = ErrorObject.FromTask( pdfSigningTask );

					// Do not overwrite existing error object with the result from the signing.
					if( error == null && pdfSigningTask != null )
						error = pdfSigningTask.Result;
				}
				finally
				{
					
					
				}
			}
			return error;
		}

		/// <summary>
		/// Creates an evidence pdf for the document and signs it with smartcard in a separate process.
		/// </summary>
		/// <param name="evidencePath">Path to the evidence PDF.</param>
		/// <param name="encodedConfiguration">Encoded configuration for signing in separate process.</param>
		/// <param name="objver">The object version of the object that is signed</param>
		/// <param name="accountName">The account name of the user signing the document.</param>
		/// <param name="uiCulture">The ui culture for the separate task.</param>
		/// <returns>Error object from the separate process.</returns>
		private static ErrorObject SignEvidencePDF( 
			string evidencePath,
			string encodedConfiguration,
			string objver,
			string accountName, 
			string uiCulture, 
			Progress progressBarDialog 
		)
		{
			// Set proper culture. This is required for properly localized error messages when running in separate task.
			using( TemporaryCulture temporaryCulture = createTemporaryCulture( uiCulture ) )
			{
				// Sign the evidence pdf with with smart card in a separate process.
				progressBarDialog.SetPhase( Progress.ProgressPhase.SignEvidencePDF );
				ErrorObject errorObject = SignFileWithSmartCard( encodedConfiguration, objver, accountName, evidencePath );
				return errorObject;	
			}
		}

		/// <summary>
		/// Returns the name of the object. Will contain the extension, if the object is a single-file document.
		/// </summary>
		private static string GetDocumentName( dynamic document )
		{
			// Return also extension, if single-file and extension exists.
			string title = document.Title;
			if( document.SingleFile )
			{
				// Does the file have an extension?
				dynamic file = document.Files.Item( 1 );
				string extension = file.Extension;
				if( !string.IsNullOrEmpty( extension ) )
					return string.Format( "{0}.{1}", title, extension );
			}

			// Return only the title in other cases.
			return title;
		}

		/// <summary>
		/// Signs the pdf with a smartcard in a separate process.
		/// </summary>
		/// <param name="encodedConfiguration">Encoded configuration for signing in separate process.</param>
		/// <param name="objver">The object version of the object that is signed</param>
		/// <param name="accountName">The account name of the user signing the document.</param>
		/// <param name="pdfPath">The path to the pdf, that should be signed.</param>
		/// <returns>Error object from the separate process.</returns>
		private static ErrorObject SignFileWithSmartCard( string encodedConfiguration, string objver, string accountName, string pdfPath )
		{
			// Get temporary file path for signing the evidence pdf file.
			string readyFilePath = Path.GetTempFileName();

			try
			{
				// Prepare the process for execution.
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					CreateNoWindow = true,
					UseShellExecute = false,  // Using ShellExecutes would limit the length of allowed command line arguments.
					FileName = GetApplicationForThisPlatform(),
					Arguments = string.Format( "\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\"", pdfPath, encodedConfiguration, objver, accountName, readyFilePath )
				};

				// Prepare background process launcher for signing.
				BackgroundProcessLauncher pdfSignatureProcessLauncher = new BackgroundProcessLauncher( processStartInfo, readyFilePath );

				// Execute the signing in a separate process.
				return pdfSignatureProcessLauncher.Execute();
			}
			finally
			{
				// Delete the temporary file, if exists.
				if( File.Exists( readyFilePath ) )
					File.Delete( readyFilePath );
			}
		}
	
		/// <summary>
		/// Gets binaries that are required for the application to run.
		/// </summary>
		/// <returns>List of required binaries.</returns>
		public static IEnumerable< string > GetRequiredBinaries()
		{
			// Return static binaries.
			var staticBinaries = new string[] { "libpdfNET.dll", "PdfSecureNET.dll",
														"MFiles.PdfTools.dll", "MFiles.PdfTools.pdb", 
														"MFiles.PdfTools.UIExt.exe", "MFiles.PdfTools.UIExt.pdb",
														"Newtonsoft.Json.dll" };
			foreach( var staticBinary in staticBinaries )
			{
				yield return staticBinary;
			}

			// Return localization binaries.
			var localizedLanguages = new string[] {"fi-FI", "sv-SE"};
			var localizedAssemblies = new string[] {"MFiles.PdfTools.resources.dll", "MFiles.PdfTools.UIExt.resources.dll"};
			foreach( var localizedLanguage in localizedLanguages )
			{
				// Return the assemblies.
				foreach( var localizedAssembly in localizedAssemblies )
				{
					// Combine the language and the assembly.
					var relativePath = Path.Combine( localizedLanguage, localizedAssembly );
					yield return relativePath;
				}

			}  // end foreach
		}

		/// <summary>
		/// Gets the path to the application for this platform.
		/// </summary>
		/// <returns>Path to the platform.</returns>
		private static string GetApplicationForThisPlatform()
		{
			// Get path to the application for this platform.
			var currentAssembly = new FileInfo( Assembly.GetExecutingAssembly().Location );
			return currentAssembly.FullName;
		}
	}
}
