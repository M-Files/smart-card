using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Abstraction for launching processes in the background.
	/// </summary>
	internal class BackgroundProcessLauncher
	{
		/// <summary>
		/// Parameters for the process.
		/// </summary>
		private ProcessStartInfo Parameters { get; set; }

		/// <summary>
		/// Ready file that indicates that the process has finished.
		/// </summary>
		private string ReadyFilePath { get; set; }

		/// <summary>
		/// Initializes new 
		/// </summary>
		/// <param name="parameters">Process startup parameters</param>
		/// <param name="readyFilePath">Path to the ready file that the called process uses to indicate the results.</param>
		internal BackgroundProcessLauncher( ProcessStartInfo parameters, string readyFilePath )
		{
			this.Parameters = parameters;
			this.ReadyFilePath = readyFilePath;
		}

		/// <summary>
		/// Executes the process in the background.
		/// </summary>
		/// <param name="completed">Called after the process has exited.</param>
		internal ErrorObject Execute()
		{
			// Execute the process.
			using( var proc = new Process { StartInfo = this.Parameters, EnableRaisingEvents = false } )
			{
				// Execute.
				proc.Start();

				// Wait for the process to exist.
				ReadyFile ready = null;
				var sw = new Stopwatch();
				while( !proc.WaitForExit( 50 ) )
				{
					// On some environments the application performing the signign gets stuck after exiting the main method.
					// The signing itself has been completed correctly at this point.
					//
					// As a workaround for this issue we allow 500 ms for the application to close properly before we forcibly terminate it here.
					// All relevant error information is stored in the ready file. The ReadyFile is the last thing the appliction does.
					// => We count 500 ms after ready file has been written and terminate the process after that.
					//
					// This behavior was consistently reproducable on Timo Hyrsylä's computer. OS was Windows 8.
					if( sw.ElapsedMilliseconds > 500 )
						proc.Kill();

					// Check for ready file.
					ready = ReadyFile.TryRead( this.ReadyFilePath );
					if( ready != null && !sw.IsRunning )
						sw.Start();
				}
				sw.Stop();
				if( ready == null )
					ready = ReadyFile.TryRead( this.ReadyFilePath );

				// Did we get a valid ready file?.
				if( ready != null && proc.ExitCode == 0 )
				{
					return null;
				}
				else if( ready != null )
				{
					// Convert error to error object.
					return new ErrorObject( ready.ErrorCode, this.GetErrorMessage( ready.ErrorCode, ready.ErrorMessage ), ready.ErrorStack );
				}
				else if( proc.ExitCode == 0 )
				{
					// The application did not procude ready file.
					return new ErrorObject( ApplicationErrorCodes.GeneralError, "The signer application was unable to report the results.", "" );
				}
				else
				{
					// The application failed and was unable to produce the ready file.
					return new ErrorObject( proc.ExitCode, string.Format( "Unknown error {0}", proc.ExitCode ), "" );
				}

			}  // end using
		}

		/// <summary>
		/// Gets error message for the specified error code.
		/// </summary>
		/// <param name="errorCode">Error code</param>
		/// <param name="defaultMessage">Default error message if no error code specific translation was found.</param>
		/// <returns>Translated error message</returns>
		private string GetErrorMessage( int errorCode, string defaultMessage )
		{
			// Translate the error code.
			string message;
			switch( errorCode )
			{
			// Special error message when the certificate is not found.
			case ApplicationErrorCodes.CertificateNotFound:
				message = InternalResources.Error_CertificateNotFound;
				break;

			// Special error when the private key could not be used.
			case ApplicationErrorCodes.PrivateKeyNotFound:
				message = InternalResources.Error_PrivateKeyNotFound;
				break;

			// Use the given error message as-is.
			default:
				message = defaultMessage;
				break;
			}
			return message;
		}
	}
}
