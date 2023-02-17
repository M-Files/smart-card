using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Converts alias candidates to aliases.
	/// </summary>
	internal class AliasCandidateResolver : IAliasCandidateConverter
	{
		/// <summary>
		/// Are we allowed to resolve aliases.
		/// </summary>
		private bool ResolveAliases { get; set; }

		/// <summary>
		/// Generic alias resolver.
		/// </summary>
		private IAliasResolver Resolver { get; set; }

		/// <summary>
		/// Typed alias resolver.
		/// </summary>
		private ITypedAliasResolver TypedResolver { get; set; }

		/// <summary>
		/// Initializes new alias candidate converter.
		/// </summary>
		/// <param name="resolveAliases">True to allow resolving of aliases</param>
		/// <param name="resolver">Alias resolver for resolving aliases</param>
		/// <param name="typedResolver">Typed alias resolver.</param>
		internal AliasCandidateResolver(  bool resolveAliases, IAliasResolver resolver, ITypedAliasResolver typedResolver )
		{
			// Store paremeters.
			this.ResolveAliases = resolveAliases;
			this.Resolver = resolver;
			this.TypedResolver = typedResolver;
		}

		public string GetAlias( object aliasCandidate )
		{
			// Convert to string if possible.
			var alias = aliasCandidate as string;
			return alias;
		}

		public int GetId( object aliasCandidate )
		{
			// Is the alias candidate an integer.
			if( Helper.IsNumber( aliasCandidate ) )
				return Int32.Parse( aliasCandidate.ToString() );

			// Is alias resolution allowed?
			if( !this.ResolveAliases )
				return -1;

			// The alias should be an actual alias.
			string alias = GetAlias( aliasCandidate );
			if( string.IsNullOrEmpty( alias ) )
				throw new ArgumentException( string.Format( InternalResources.Error_UnableToConvertAlias_X_OfType_Y, aliasCandidate, aliasCandidate.GetType() ) );
			return this.TypedResolver.ResolveAlias( this.Resolver, alias );
		}
	}
}
