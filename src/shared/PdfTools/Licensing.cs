using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pdftools.PdfSecure;

namespace MFiles.PdfTools
{
	/// <summary>
	/// General environment of the PDF tools.
	/// </summary>
	public static class Licensing
	{
		/// <summary>
		/// Mutex protecting the license key.
		/// </summary>
		private static readonly object KeyLock = new object();

		/// <summary>
		/// The license key.
		/// </summary>
		private static string licenseKey;

		/// <summary>
		/// OEM license key.
		/// </summary>
		public const string OemLicenseKey = "<insert license key here>";

		/// <summary>
		/// License key for the PDF Tools.
		/// </summary>
		public static string LicenseKey 
		{ 
			get
			{
				// Must be initialized.
				ConfigurationManager.Initialize();

				// Syncronize access to the key.
				lock( Licensing.KeyLock )
				{
					return Licensing.licenseKey;
				}
			}
			set
			{
				// Must be initialized.
				ConfigurationManager.Initialize();

				// Syncronize access to the key.
				lock( Licensing.KeyLock )
				{
					Licensing.licenseKey = value;
					Secure.SetLicenseKey( licenseKey );
				}
			} 
		}

		/// <summary>
		/// Is the license currently valid?
		/// </summary>
		public static bool IsLicenseValid 
		{ 
			get
			{
				// Must be initialized.
				ConfigurationManager.Initialize();

				return Secure.GetLicenseIsValid();
			} 
		}
	}
}
