using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MFiles.PdfTools.Certificates;
using MFiles.PdfTools.UIExt.Exceptions;

namespace MFiles.PdfTools.UIExt
{
	public class ErrorObject
	{
		/// <summary>
		/// The error code of the process once it as finished.
		/// </summary>
		public int ErrorCode { get; set; }

		/// <summary>
		/// The error message of the process once it has finished.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// The error stack of the process once it has finished.
		/// </summary>
		public string ErrorStack { get; set; }

		public ErrorObject( int ErrorCode, string ErrorMessage, string ErrorStack )
		{
			this.ErrorCode = ErrorCode;
			this.ErrorMessage = ErrorMessage;
			this.ErrorStack = ErrorStack;
		}

		public static ErrorObject FromTask( System.Threading.Tasks.Task faultedTask )
		{
			return new ErrorObject(
					ApplicationErrorCodes.GeneralError,
					faultedTask.Exception.InnerExceptions
							.Select( ex => ex.Message )
							.Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ? next : string.Concat( current, "; ", next ) ),
					faultedTask.Exception.InnerExceptions
							.Select( ex => ex.ToString() )
							.Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ? next : string.Concat( current, "; ", next ) )
			);
		}

		public static ErrorObject FromException( Exception e )
		{
			return new ErrorObject(
					ApplicationErrorCodes.GeneralError,
					getExceptions( e )
							.Select( ex => ex.Message )
							.Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ? next : string.Concat( current, "; ", next ) ),
					getExceptions( e )
							.Select( ex => ex.ToString() )
							.Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ? next : string.Concat( current, "; ", next ) )
			);
		}

		private static IEnumerable<Exception> getExceptions( Exception e )
		{
			while( e != null )
			{
				yield return e;
				e = e.InnerException;
			}
		}

		/// <summary>
		/// Throws the error as an exception.
		/// </summary>
		/// <param name="errorCode">Error code</param>
		/// <param name="errorMessage">Error message</param>
		public void ThrowError()
		{
			// Process possible error.
			switch( ErrorCode )
			{
			// No error?
			case ApplicationErrorCodes.Success:
				break;

			// Unknown error.
			case ApplicationErrorCodes.GeneralError:
				throw new ApplicationException( ErrorMessage );

			// Invalid arguments.
			case ApplicationErrorCodes.InvalidArguments:
				throw new ArgumentException( ErrorMessage );

			// File not found.
			case ApplicationErrorCodes.FileToBeSignedNotFound:
				throw new FileNotFoundException( ErrorMessage );

			// License error.
			case ApplicationErrorCodes.LicenseIsInvalid:
				throw new InvalidLicenseException( ErrorMessage );

			// Certificate was not found.
			case ApplicationErrorCodes.CertificateNotFound:
				throw new CertificateNotFoundException( ErrorMessage );

			// Duplicate signature.
			case ApplicationErrorCodes.DuplicateSignature:
				throw new DuplicateSignatureException();

			// IssuedBy was not specified.
			case ApplicationErrorCodes.IssuedByNotSpecified:
				throw new IssuedByNotSpecifiedException();

			// New version exists.
			case ApplicationErrorCodes.NewVersionExists:
				throw new NewObjectVersionException();

			// Private key of the certificate could not be located.
			case ApplicationErrorCodes.PrivateKeyNotFound:
				throw new PrivateKeyNotAvailableException( ErrorMessage );

			// A generic PDF tools error.
			case ApplicationErrorCodes.PdfToolsError:
				throw new PdfToolsException( ErrorMessage );

			// Unexpected error.
			default:
				throw new ApplicationException( string.Format( InternalResources.Error_SigningFailedWithUnexpectedError_X_Code_Y, ErrorMessage, ErrorCode ) );

			}
		}
	}
}
