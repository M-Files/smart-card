using System.Runtime.Serialization;

namespace MFiles.SmartCard.Configuration
{
	/// <summary>
	/// Module configuration.
	/// </summary>
	public class SignerConfiguration
	{
		/// <summary>
		/// Is the configuration enabled.
		/// </summary>
		public bool Enabled = false;
		
		/// <summary>
		/// The locale of the user.
		/// </summary>
		public string Locale = "";

		/// <summary>
		/// Is it needed to validate signature.
		/// </summary>
		public bool ValidateSignature = true;

		/// <summary>
		/// PDFTools component configuration.
		/// </summary>
		public MFiles.PdfTools.UIExt.Configuration.PdfTools PdfTools;

		/// <summary>
		/// Configuration for authentication.
		/// </summary>
		public MFiles.PdfTools.UIExt.Configuration.Authentication Authentication;

	}
}
