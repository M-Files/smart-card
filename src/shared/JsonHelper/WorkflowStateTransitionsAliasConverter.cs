using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Converter for converting workflow state transitions.
	/// </summary>
	public class WorkflowStateTransitionAliasConverter : AliasConverterBase<WorkflowStateTransitionAliasConverter>
	{
		public override int ResolveAlias( IAliasResolver aliasResolver, string alias )
		{
			// Delegate.
			int id = aliasResolver.GetWorkflowStateTransitionIdByAlias( alias );
			if( id == -1 )
				throw new AliasResolutionFailedException( string.Format( InternalResources.Error_UnableToLocateWorkflowStateByAlias_X, alias ) );
			return id;
		}
	}
}
