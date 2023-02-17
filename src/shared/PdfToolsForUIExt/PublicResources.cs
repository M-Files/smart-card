using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// This exposes resources for the user.
	/// </summary>
	public class PublicResources
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
		/// Culture for fetching the resources.
		/// </summary>
		private string UiCulture { get; set; }

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
		/// The name of the command for signing an object.
		/// </summary>
		public string CommandSignObject { get { return Translate( () => InternalResources.Commands_SignWithSmartCard ); } }

		/// <summary>
		/// The error is displayed when the object is checked out.
		/// </summary>
		public string ErrorObjectCheckedOut { get { return Translate( () => InternalResources.Error_ObjectCheckedOut ); } }

		/// <summary>
		/// The error is displayed when a new version of the object is available.
		/// </summary>
		public string ErrorNewVersionAvailable { get { return Translate( () => InternalResources.Error_NewVersionAvailable ); } }

		/// <summary>
		/// The error is displayed when the caller attempts to use the module while it is disabled.
		/// </summary>
		public string NotEnabled { get { return Translate( () => InternalResources.Error_NotEnabled ); } }
		
		/// <summary>
		/// Translates the given text.
		/// </summary>
		/// <param name="translation">Functor which returns the translated text.</param>
		/// <returns></returns>
		private string Translate( Func< string > translation )
		{
			using( var culture = new TemporaryCulture( null, this.UiCulture ) )
			{
				return translation();
			}
		}
	}
}
