
namespace MFiles.Internal.Json
{
	/// <summary>
	/// Definition for an alias resolver.
	/// </summary>
	public interface IAliasResolver
	{
		/// <summary>
		/// Gets object type id by alias.
		/// </summary>
		/// <param name="alias">Alias</param>
		/// <returns>ID of the object type.</returns>
		int GetObjectTypeIdByAlias( string alias );

		/// <summary>
		/// Gets property definition id by alias.
		/// </summary>
		/// <param name="alias">Alias of the property definition.</param>
		/// <returns>Id of the property definition.</returns>
		int GetPropertyDefIdByAlias( string alias );

		/// <summary>
		/// Gets workflow state id by alias.
		/// </summary>
		/// <param name="alias">Alias of the workflow state</param>
		/// <returns>Id of the workflow state</returns>
		int GetWorkflowStateIdByAlias( string alias );

		/// <summary>
		/// Gets workflow state transition id by alias.
		/// </summary>
		/// <param name="alias">Alias of the workflow state transitions</param>
		/// <returns>Id of the workflow state</returns>
		int GetWorkflowStateTransitionIdByAlias( string alias );
	}
}
