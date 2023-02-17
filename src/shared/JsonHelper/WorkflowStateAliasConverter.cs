using System;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Converter for converting workflow aliases.
	/// </summary>
	public class WorkflowStateAliasConverter : AliasConverterBase< WorkflowStateAliasConverter >
	{
		public override int ResolveAlias( IAliasResolver aliasResolver, string alias )
		{
			// Delegate.
			int id = aliasResolver.GetWorkflowStateIdByAlias( alias );
			if( id == -1 )
				throw new AliasResolutionFailedException( string.Format( InternalResources.Error_UnableToLocateWorkflowStateByAlias_X, alias ) );
			return id;
		}
	}
}
