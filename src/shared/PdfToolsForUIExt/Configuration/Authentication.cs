using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using MFiles.Internal.Json;
using Newtonsoft.Json;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Settings for defining the authentication requirements.
	/// </summary>
	public class Authentication
	{
		/// <summary>
		/// List of all allowed authentication types.
		/// </summary>
		[DataMember]
		public List<int> AllowedAuthenticationTypes { get; set; }

		/// <summary>
		/// List of workflow states which require the SmartCard authentication.
		/// </summary>
		[DataMember]
		[JsonConverter( typeof( WorkflowStateAliasConverter ) )]
		public List<StructureItem> RequiredForStates { get; set; }

		/// <summary>
		/// List of workflow state transitions which require the SmartCard authentication.
		/// </summary>
		[DataMember]
		[JsonConverter( typeof( WorkflowStateTransitionAliasConverter ) )]
		public List<StructureItem> RequiredForStateTransitions { get; set; }

		/// <summary>
		/// List of workflow state transitions which signs PDF with the SmartCard authentication.
		/// </summary>
		[DataMember]
		[JsonConverter( typeof( WorkflowStateTransitionAliasConverter ) )]
		public List<StructureItem> SignPDFForStateTransitions { get; set; }

		/// <summary>
		/// The certificate used in the authentication.
		/// </summary>
		[DataMember]
		public List<Certificate> Certificates { get; set; }

		/// <summary>
		/// Parses the configuration from the given JSON string.
		/// </summary>
		/// <param name="jsonConfiguration"></param>
		/// <returns>Confgiuration.</returns>
		internal static Authentication Parse( string jsonConfiguration )
		{
			// Deserialize the configuration.
			var settings = new JsonSerializerSettings
				{
					Context = DeserializationContext.CreateContext()
				};
			var configuration = JsonConvert.DeserializeObject<MFiles.PdfTools.UIExt.Configuration.Authentication>( jsonConfiguration, settings );
			return configuration;
		}
	}
}
