using System;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Converter for resolving property definition aliases.
	/// </summary>
	public class PropertyDefAliasConverter : AliasConverterBase< PropertyDefAliasConverter >
	{
		public override int ResolveAlias( IAliasResolver aliasResolver, string alias )
		{
			// Delegate.
			int id = 
				aliasResolver.GetPropertyDefIdByAlias( alias );			
			if( id == -1 )
				throw new AliasResolutionFailedException( string.Format( InternalResources.Error_UnableToLocatePropertyDefinitionByAlias_X, alias ) );
			return id;
		}
	}
}
