using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MFiles.PdfTools.UIExt;
using MFilesAPI;

namespace MFilesSmartCard
{
	/// <summary>
	/// Sets temporary culture for the operation.
	/// </summary>
	internal class TemporaryCulture : IDisposable
	{
		/// <summary>
		/// Previous culture.
		/// </summary>
		private CultureInfo PreviousCulture { get; set; }

		/// <summary>
		/// Previous UI culture.
		/// </summary>
		private CultureInfo PreviousUICulture { get; set; }

		/// <summary>
		/// Initializes new temporary culture.
		/// </summary>
		internal TemporaryCulture()
		{
			this.PreviousCulture = CultureInfo.CurrentCulture;
			this.PreviousUICulture = CultureInfo.CurrentUICulture;
		}

		/// <summary>
		/// Initializes new temporary culture.
		/// </summary>
		/// <param name="vault">New culture.</param>
		/// <param name="selectedLocale">Locale for the user interface culture.</param>
		internal TemporaryCulture( Vault vault, string selectedLocale )
		{
			// Store previous culture.
			this.PreviousCulture = CultureInfo.CurrentCulture;
			this.PreviousUICulture = CultureInfo.CurrentUICulture;

			// Determine new UI culture.
			var serverApp = new MFilesServerApplication();
			var mfilesVersion = serverApp.GetAPIVersion();
			string uiCulture = CultureHelper.ParseUICulture( selectedLocale, mfilesVersion.Display );			
			Marshal.FinalReleaseComObject( serverApp );
			Marshal.FinalReleaseComObject( mfilesVersion );

			// Set new UI culture.
			this.SetCulture( null, uiCulture );
		}

		/// <summary>
		/// Sets new culture.
		/// </summary>
		/// <param name="culture">New culture.</param>
		/// <param name="uiCulture">New user interface culture.</param>
		internal void SetCulture( string culture, string uiCulture )
		{
			if( !string.IsNullOrEmpty( culture ) )
				Thread.CurrentThread.CurrentCulture = new CultureInfo( culture );
			if( !string.IsNullOrEmpty( uiCulture ) )
				Thread.CurrentThread.CurrentUICulture = new CultureInfo( uiCulture );
		}

		public void Dispose()
		{
			// Restore previous cultures.
			if( this.PreviousCulture != null )
				Thread.CurrentThread.CurrentCulture = this.PreviousCulture;
			if( this.PreviousUICulture != null )
				Thread.CurrentThread.CurrentUICulture = this.PreviousUICulture;
		}
	}
}
