using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MFiles.Internal.Win32PInvoke;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Helper functions for selecting proper culture.
	/// </summary>
	public static class CultureHelper
	{
		/// <summary>
		/// Determines the correct culture based on the configuration.
		/// </summary>
		/// <remarks>
		/// The allowed values for locale are:
		/// "" or "mfiles" The locale is selected based on the installation language of M-Files. M-Files version must be specified.
		/// "windows" The locale is selected based on the current local of Windows.
		/// "en-US" Fixed locale. See http://msdn.microsoft.com/en-us/library/ee825488%28v=cs.20%29.aspx for allowed Language Culture Names.		
		/// </remarks>
		/// <param name="locale">The configured locale</param>
		/// <param name="mfilesVersion">M-Files version. Required if the language of M-Files is to be used.</param>
		/// <returns>Culture</returns>
		public static string ParseUICulture( string locale, string mfilesVersion )
		{
			// Use fixed culture from the configuration?
			if( !string.IsNullOrEmpty( mfilesVersion )
				&& ( string.IsNullOrEmpty( locale ) || locale.Equals( "mfiles", StringComparison.InvariantCultureIgnoreCase ) ) )
			{
				// Default to M-Files local. Only possible if M-Files version was provided.
				return DetermineCultureBasedOnMFilesInstallationLanguage( mfilesVersion );
			}
			else if( locale.Equals( "windows", StringComparison.InvariantCultureIgnoreCase ) )
				return CultureInfo.CurrentUICulture.Name;
			else if( string.IsNullOrEmpty( locale ) )
				throw new ArgumentException( "M-Files version must be specified when locale is left empty.", "mfilesVersion" );
			else
				return locale;
		}

		/// <summary>
		/// Determines the culture based on the M-Files installation language.
		/// </summary>
		/// <param name="mfilesVersion">M-Files version.</param>
		/// <returns>Culture</returns>
		private static string DetermineCultureBasedOnMFilesInstallationLanguage( string mfilesVersion )
		{
			// Read the installation langugage.	
			string key = string.Format( @"SOFTWARE\Motive\M-Files\{0}\", mfilesVersion );
			var installationLanguage = RegistryHelper.ReadRegValue( RegistryHelper.HKEY_LOCAL_MACHINE,
				RegistryHelper.enumRegistryAccessFlags.eregaccessSystemNative, key, "Language" );
			if( installationLanguage == null )
				throw new ArgumentException( "Installation language could not be found." );

			// Convert the language to appropriate culture.
			string culture;
			switch( installationLanguage.GetValue().ToString().ToLowerInvariant() )
			{

			case "fin":
				culture = "fi-FI";
				break;

			case "fra":
				culture = "fr-FR";
				break;

			case "cht":
				culture = "zh-TW";
				break;

			case "deu":
				culture = "de-DE";
				break;

			case "plk":
				culture = "pl-PL";
				break;

			case "slv":
				culture = "sl-SI";
				break;

			case "vit":
				culture = "vi-VN";
				break;

			case "ptb":
				culture = "pt-BR";
				break;

			case "hun":
				culture = "hi-HU";
				break;

			case "sve":
				culture = "sv-SE";
				break;

			case "nld":
				culture = "nl-NL";
				break;

			case "trk":
				culture = "tr-TR";
				break;

			case "ell":
				culture = "el-GR";
				break;

			case "jpn":
				culture = "ja-JP";
				break;

			case "ara":
				culture = "ar-JO";
				break;

			case "ita":
				culture = "it-IT";
				break;

			case "esn":
				culture = "es-ES";
				break;

			case "rus":
				culture = "ru-RU";
				break;

			case "hrv":
				culture = "hr-HR";
				break;

			case "chs":
				culture = "zh-CN";
				break;

			case "csy":
				culture = "cs-CZ";
				break;

			case "bgr":
				culture = "bg-BG";
				break;

			case "heb":
				culture = "he-IL";
				break;

			case "eti":
				culture = "et-EE ";
				break;

			// Default to en-US
			default:
				culture = "en-US";
				break;
			}
			return culture;
		}
	}
}
