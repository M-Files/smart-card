using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Base class for alias converters.
	/// </summary>
	public abstract class AliasConverterBase< T > : JsonConverter, ITypedAliasResolver
	{
		/// <summary>
		/// Prevents recursive use of this converter.
		/// </summary>
		private readonly IContractResolver exclusionResolver = new JsonConverterExclusionResolver< T >();

		public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
		{
			// Must have alias resolver available.
			if( serializer.Context.Context == null )
				throw new ApplicationException();
			var conversionContext = serializer.Context.Context as IConversionContext;
			if( conversionContext == null )
				throw new ApplicationException();

			// Construct JObject from the object.
			var innerSerializer = new JsonSerializer();
			innerSerializer.ContractResolver = exclusionResolver;
			JToken valueToken;			
			var structureItem = value as StructureItem;
			if( structureItem != null )
			{
				// Is the value stored as structure item?
				if( conversionContext.ResolveAliases )
					valueToken = new JValue( structureItem.Id );
				else
					valueToken = new JValue( structureItem.GetAliasIfAvailable() );
			}
			else if( typeof( IEnumerable< StructureItem > ).IsAssignableFrom( value.GetType() ) )
			{
				// Serialize a collection of structured items.
				var values = new JArray();
				foreach( var item in (IEnumerable< StructureItem >) value )
				{
					// Resolved aliases?
					if( conversionContext.ResolveAliases )
						values.Add( new JValue( item.Id ) );
					else
						values.Add( new JValue( item.GetAliasIfAvailable() ) );					
				}
				valueToken = values;
			}
			else
			{
				// The value is not stored as structure items.
				if( conversionContext.ResolveAliases )
					valueToken = this.ConvertToIdToken( writer.Path, conversionContext.AliasResolver, value );
				else
					valueToken = this.ConvertToToken( writer.Path, value );
				
			}
			valueToken.WriteTo( writer );	
		}

		/// <summary>
		/// This converter is for serializing only.
		/// </summary>
		public override bool CanRead
		{
			get { return true; }
		}

		public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
		{
			// Must have alias resolver available.
			if( serializer.Context.Context == null )
				throw new ApplicationException();
			var conversionContext = serializer.Context.Context as IConversionContext;
			if( conversionContext == null )
				throw new ApplicationException();

			// Null is null.
			if( reader.TokenType == JsonToken.Null )
				return null;

			// Try parsing the target as structure item.
			object target;
			if( StructureItem.TryParse( conversionContext, new AliasCandidateResolver(  conversionContext.ResolveAliases, conversionContext.AliasResolver, this ), reader, objectType, out target ) )
				return target;
			
			// Delegate to base.
			var token = JToken.Load( reader );
			target = token.ToObject( objectType );
			return target;
		}

		public override bool CanConvert( Type objectType )
		{
			// TODO: For now we assume that the converter is only configured via attributes to property types.
			return true;
		}

		/// <summary>
		/// Resolves the aliases.
		/// </summary>
		/// <param name="aliasResolver">Interface for resolving the aliases.</param>
		/// <param name="alias">Alias to resolve</param>
		/// <returns>The id of the object.</returns>
		public abstract int ResolveAlias( IAliasResolver aliasResolver, string alias );

		/// <summary>
		/// Converts the given value to id token.
		/// </summary>
		/// <param name="path">Path to the item being converted.</param>
		/// <param name="aliasResolver">Alias resolver</param>
		/// <param name="value">Value(s) to convert</param>
		/// <returns>Id(s) as JToken</returns>
		private JToken ConvertToIdToken( string path, IAliasResolver aliasResolver, object value )
		{
			// Convert.
			if( value is IEnumerable< object > )
				return this.ConvertToIdArray( path, aliasResolver, (IEnumerable< object >) value );
			else if( value is string )
				return new JValue( this.ResolveAlias( aliasResolver, (string) value ) );
			else
				return this.ConvertToToken( path, value );
		}

		/// <summary>
		/// Converts the given value to token.
		/// </summary>
		/// <param name="path">Path to the item being converted.</param>
		/// <param name="value">Values to convert</param>
		/// <returns>Ids as Json array</returns>
		private JToken ConvertToToken( string path, object value )
		{
			// Convert to plain token.
			if( value is IEnumerable< object > )
				return JArray.FromObject( value );
			else if( value is string )
				return new JValue( (string) value );
			else if( Helper.IsNumber( value ) )
				return new JValue( value );
			else
				throw new ArgumentException( string.Format( "Cannot convert property {0} of type {1} to JToken.", path, value.GetType() ) );
		}

		/// <summary>
		/// Converts the given values to id array
		/// </summary>
		/// <param name="path">Path to the item being converted.</param>
		/// <param name="aliasResolver">Alias resolver</param>
		/// <param name="values">Values to convert</param>
		/// <returns>Ids as Json array</returns>
		private JArray ConvertToIdArray( string path, IAliasResolver aliasResolver, IEnumerable< object > values )
		{
			// Resolve the ids.
			var idArray = new JArray();
			foreach( var value in values )
			{
				// Get the id.
				JValue converted;
				if( value is string )
				{
					// Treat strings as aliases.
					converted = new JValue( this.ResolveAlias( aliasResolver, (string) value ) );
				}
				else if( Helper.IsNumber( value ) )
				{
					// Treat numbers as ids.
					converted = new JValue( value );
				}				
				else
				{
					// Conversion failure.
					throw new ArgumentException( string.Format( InternalResources.Error_UnableToConvertType_X_AndProperty_Y_ToId, value.GetType(), path ) );
				}

				// Collect.				
				idArray.Add( converted );

			} // end foreach.
			return idArray;
		}		
	}
}
