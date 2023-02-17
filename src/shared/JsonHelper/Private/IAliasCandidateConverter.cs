using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Converts single alias candidate to alias and id.
	/// </summary>
	internal interface IAliasCandidateConverter
	{
		/// <summary>
		/// Gets the 
		/// </summary>
		/// <param name="aliasCandidate">Alias candidate</param>
		/// <returns>Alias or null if the candidate was not an alias.</returns>
		string GetAlias( object aliasCandidate );

		/// <summary>
		/// Gets the id from the given alias. If the alias candidate is an alias then it is resolved to the actual id.
		/// </summary>
		/// <param name="aliasCandidate">Alias candidate.</param>
		/// <returns>Id</returns>
		int GetId( object aliasCandidate );
	}
}
