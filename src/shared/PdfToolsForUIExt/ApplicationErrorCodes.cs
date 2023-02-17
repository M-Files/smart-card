using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Error codes returned by the application.
	/// 
	/// These error code may be used in UI Ext applications in JS.
	/// Do not alter them.
	/// </summary>
	public static class ApplicationErrorCodes
	{
		/// <summary>
		/// Success.
		/// </summary>
		public const int Success = 0;

		/// <summary>
		/// A general error occurred.
		/// </summary>
		public const int GeneralError = -1;

		/// <summary>
		/// Invalid arguments were prodived.
		/// </summary>
		public const int InvalidArguments = -2;

		/// <summary>
		/// The file that were supposed to sign was not found.
		/// </summary>
		public const int FileToBeSignedNotFound = -3;

		/// <summary>
		/// The specified PDF Tools license is invalid.
		/// </summary>
		public const int LicenseIsInvalid = -4;

		/// <summary>
		/// Certificate not found error code.
		/// </summary>
		public const int CertificateNotFound = -5;

		/// <summary>
		/// The document has been already signed with the defined certificate.
		/// </summary>
		public const int DuplicateSignature = -6;

		/// <summary>
		/// Issued By configuration value was not set.
		/// </summary>
		public const int IssuedByNotSpecified = -7;

		/// <summary>
		/// A new version of the object we are signing exists on the server.
		/// </summary>
		public const int NewVersionExists = -8;

		/// <summary>
		/// A general error in PDF tools.
		/// </summary>
		public const int PdfToolsError = -100;

		/// <summary>
		/// The private key of the signing certificate was not found.
		/// </summary>
		public const int PrivateKeyNotFound = -101;	

	}
}
