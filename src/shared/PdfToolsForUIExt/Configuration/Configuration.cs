using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Configuration for the UI Extension.
	/// </summary>
	[DataContract( Namespace = "" )]
	internal class Configuration
	{
		/// <summary>
		/// The locale of the user.
		/// </summary>
		[DataMember]
		public string Locale { get; set; }

		/// <summary>
		/// Settings for the PDF Tools.
		/// </summary>
		[DataMember]
		public PdfTools PdfTools { get; set; }

		/// <summary>
		/// Defines the actual authentication requirements.
		/// </summary>
		[DataMember]
		public Authentication Authentication { get; set; }

		/// <summary>
		/// Gets the UI culture based on the defined locale.
		/// </summary>
		/// <param name="mfilesVersion">M-Files version in use.</param>
		/// <returns>Requested UI culture.</returns>
		public string GetUICulture( string mfilesVersion )
		{
			// Delegate to the helper.
			return MFiles.PdfTools.UIExt.CultureHelper.ParseUICulture( this.Locale, mfilesVersion );
		}

		/// <summary>
		/// Serializes this configuration
		/// </summary>
		/// <returns>Serialized configuration.</returns>
		internal string Serialize()
		{
			var settings = new JsonSerializerSettings
			{
				Context = DeserializationContext.CreateContext()
			};
			var serializedConfiguration = JsonConvert.SerializeObject( this, settings );
			return serializedConfiguration;
		}

		/// <summary>
		/// Parses the configuration from the given JSON string.
		/// </summary>
		/// <param name="jsonConfiguration"></param>
		/// <returns>Confgiuration.</returns>
		internal static Configuration Parse( string jsonConfiguration )
		{
			// Deserialize the configuration.
			var settings = new JsonSerializerSettings
				{
					Context = DeserializationContext.CreateContext()
				};
			var configuration = JsonConvert.DeserializeObject<MFiles.PdfTools.UIExt.Configuration.Configuration>( jsonConfiguration, settings );
			return configuration;
		}
	}
}
