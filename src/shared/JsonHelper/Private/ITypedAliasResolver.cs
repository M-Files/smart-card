using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Typed alias resolver interface.
	/// </summary>
	internal interface ITypedAliasResolver
	{
		/// <summary>
		/// Resolves the aliases.
		/// </summary>
		/// <param name="aliasResolver">Interface for resolving the aliases.</param>
		/// <param name="alias">Alias to resolve</param>
		/// <returns>The id of the object.</returns>
		int ResolveAlias( IAliasResolver aliasResolver, string alias );
	}
}
