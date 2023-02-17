using System;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Class that expands object type aliases to object type ids.
	/// </summary>
	public class ObjectTypeAliasConverter : AliasConverterBase< ObjectTypeAliasConverter >
	{	
		public override int ResolveAlias( IAliasResolver aliasResolver, string alias )
		{
			// Delegate to the resolver.
			int id = aliasResolver.GetObjectTypeIdByAlias( alias );
			if( id == -1 )
				throw new AliasResolutionFailedException( string.Format( InternalResources.Error_UnableToLocateObjectTypeByAlias_X, alias ) );
			return id;
		}		
	}
}
