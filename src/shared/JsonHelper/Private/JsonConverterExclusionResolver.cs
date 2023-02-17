using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Resolver which prevents loops during serialization.
	/// </summary>
	internal class JsonConverterExclusionResolver< T > : DefaultContractResolver
	{
		/// <summary>
		/// Resolvers
		/// </summary>
		/// <param name="objectType">Type of the object for which the converter is requested.</param>
		/// <returns>Compatible JsonConverter</returns>
		protected override JsonConverter ResolveContractConverter( Type objectType )
		{
			// Use the base class to resolve the converter and ensure that we are not creating a recursion.
			var defaultConverter = base.ResolveContractConverter( objectType );
			if( defaultConverter != null && defaultConverter.GetType() == typeof( T ) )
			{
				// Already using us as the converter.
				// Prevent recursion.
				return null;
			}
			return defaultConverter;
		}
	}
}
