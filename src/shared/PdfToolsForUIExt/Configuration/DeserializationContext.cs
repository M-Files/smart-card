using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MFiles.Internal.Json;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Context for deserializing the configuration.
	/// </summary>
	internal class DeserializationContext : IConversionContext
	{
		/// <summary>
		/// Creates new deserialization context.
		/// </summary>
		/// <returns></returns>
		internal static StreamingContext CreateContext()
		{
			var context = new StreamingContext( StreamingContextStates.Other, new DeserializationContext() );
			return context;
		}

		/// <summary>
		/// Hides the constructor.
		/// </summary>
		private DeserializationContext()
		{
			
		}

		public bool ResolveAliases
		{
			get { return false; }
		}

		public IAliasResolver AliasResolver
		{
			get { return null; }
		}

		public Enumerations.ErrorReporting ErrorReporting { get { return Enumerations.ErrorReporting.Exception; } }		
	}
}
