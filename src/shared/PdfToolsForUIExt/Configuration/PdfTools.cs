using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Description of Pdf Tools related settings.
	/// </summary>
	[DataContract( Namespace = "" )]
	public class PdfTools
	{
		/// <summary>
		/// The license key for PDF Tools.
		/// </summary>
		[DataMember]
		public string LicenseKey { get; set; }

		/// <summary>
		/// Crypto provider. Defaults to Microsoft's Crypto API. See the PDF Tools documentation for more information.
		/// </summary>
		/// <remarks>
		/// The provider can either be Microsoft’s Crypt API or a library that implements PKCS#11 to support HSM, USB tokens and smart cards.
		/// 
		/// When using the Microsoft's Crypt API, the value of this property should conform to following syntax:
		/// [ProviderType:]Provider[;PIN]
		/// Examples: 
		/// Provider = “Microsoft Base Cryptographic Provider v1.0” (Default on Windows XP and Windows 2003 Server)
		/// Provider = “Microsoft Strong Cryptographic Provider”
		/// Provider = “PROV_RSA_AES:Microsoft Enhanced RSA and AES Cryptographic Provider” (Default on Vista and up)
		/// </remarks>
		[DataMember]
		public string Provider { get; set; }

		/// <summary>
		/// Parses the PDF Tools configuration from the given JSON string.
		/// </summary>
		/// <param name="jsonConfiguration">Serialized configuration</param>
		/// <returns>Deserialized PDF Tools configuration.</returns>
		internal static PdfTools Parse( string jsonConfiguration )
		{
			// Deserialize the configuration.
			var settings = new JsonSerializerSettings
			{
				Context = DeserializationContext.CreateContext()
			};
			var configuration = JsonConvert.DeserializeObject<MFiles.PdfTools.UIExt.Configuration.PdfTools>( jsonConfiguration, settings );
			return configuration;
		}
	}
}
