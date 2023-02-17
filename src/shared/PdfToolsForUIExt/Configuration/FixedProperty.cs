using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MFiles.Internal.Json;
using Newtonsoft.Json;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Represents a fixed property value.
	/// </summary>
	[DataContract( Namespace = "" )]
	public class FixedProperty
	{
		/// <summary>
		/// Property definition 
		/// </summary>
		[DataMember]
		[JsonConverter( typeof( PropertyDefAliasConverter ) )]
		public StructureItem PropertyDef { get; set; }

		/// <summary>
		/// Value for the property.
		/// </summary>
		[DataMember]
		public object Value { get; set; }
	}
}
