using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Represents a serialized alias.
	/// </summary>
	public class StructureItem : IValidatedValue
	{
		/// <summary>
		/// The alias of this structure item.
		/// 
		/// Can be null
		/// </summary>
		public string Alias { get; private set; }

		/// <summary>
		/// The id of this structure item.
		/// </summary>
		public int Id { get; private set; }

		/// <summary>
		/// Is this structure item valid?
		/// </summary>
		public bool IsValid { get; private set; }

		/// <summary>
		/// Possible error message in case this structure item is not valid.
		/// </summary>
		public string ErrorMessage { get; private set; }

		/// <summary>
		/// Initializes new structure item based on id.
		/// </summary>
		/// <param name="id">Id of the item.</param>
		public StructureItem( int id )
		{
			this.Alias = "";
			this.Id = id;
			this.IsValid = true;
			this.ErrorMessage = "";
		}

		/// <summary>
		/// Initializes new structure item.
		/// </summary>
		/// <param name="alias">Alias of the item.</param>
		/// <param name="id">Id of the item.</param>
		/// <param name="errors">Errors that occurred while parsing the structure item.</param>
		private StructureItem( string alias, int id, List< string > errors )
		{
			this.Alias = alias;
			this.Id = id;
			this.IsValid = errors.Count == 0;
			this.ErrorMessage = errors.Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ? next : string.Concat( current, "; ", next ) );
		}

		/// <summary>
		/// Gets the alias of this item if available.
		/// </summary>
		/// <returns>Alias if available or id?</returns>
		public object GetAliasIfAvailable()
		{
			// No alias?
			if( string.IsNullOrEmpty( this.Alias ) )
				return this.Id;

			// Alias.
			return this.Alias;
		}

		/// <summary>
		/// Attempts to read the structure item from the given object.
		/// </summary>
		/// <returns></returns>
		internal static bool TryParse( IConversionContext context, IAliasCandidateConverter  candidateConverter,  JsonReader reader, Type outputType, out object output )
		{
			// Try parsing the structure item(s).
			if( outputType == typeof( StructureItem ) )
			{
				// The wanted output represents single alias.
				var serializedItem = JToken.Load( reader );
				var aliasCandidate = serializedItem.ToObject( typeof( object ) );
				output = StructureItem.Parse( context, candidateConverter, aliasCandidate );
				return true;
			}
			else if( typeof( IEnumerable< StructureItem > ).IsAssignableFrom( outputType ) )
			{
				// The output is wanted as a collection of aliases.
				var serializedItems = JArray.Load( reader );
				var items = new List< StructureItem >();
				foreach( var serializedItem in serializedItems )
				{
					// Convert the current serialized alias.
					var aliasCandidate = serializedItem.ToObject( typeof( object ) );
					var item = StructureItem.Parse( context, candidateConverter, aliasCandidate );
					items.Add( item );
				}
				output = items;
				return true;
			}
			else
			{
				output = null;
				return false;
			}
		}

		/// <summary>
		/// Can we parse the given type as structure items.
		/// </summary>
		/// <param name="outputType">Output type.</param>
		/// <returns>True if we can parse the given type.</returns>
		internal bool CanParse( Type outputType )
		{
			// Can we parse?
			if( outputType == typeof( StructureItem ) ||
				typeof( IEnumerable< StructureItem > ).IsAssignableFrom( outputType ) )
				return true;

			// We cannot parse this.
			return false;
		}

		/// <summary>
		/// Parses structure item from the given alias candidate.
		/// </summary>
		/// <returns>Structure item.</returns>
		private static StructureItem Parse( IConversionContext context, IAliasCandidateConverter candidateConverter, object aliasCandidate )
		{
			// Resolve alias
			var errors = new List< string >();
			string alias;
			{
				string error;
				if( ! TryResolveCandidate( context, candidateConverter.GetAlias, aliasCandidate, out alias, out error ) )
					errors.Add( error );
			}

			// Resolve id.
			int id;
			{
				string error;
				if( !TryResolveCandidate( context, candidateConverter.GetId, aliasCandidate, out id, out error ) )
					errors.Add( error );
			}

			// Return the parsed structure item.
			var item = new StructureItem( alias, id, errors );
			return item;
		}

		/// <summary>
		/// Attempts to resolve id for the given alias.
		/// </summary>
		/// <param name="context">conversion context</param>
		/// <param name="resolver">Alias candidate resolver function</param>
		/// <param name="aliasCandidate">The alias candidate.</param>
		/// <param name="resolvedCandidate">Receives the resovled candidate</param>
		/// <param name="error">Receives the error message if resolving the alias fialed.</param>
		/// <returns></returns>
		private static bool TryResolveCandidate< T >( IConversionContext context, Func< object, T > resolver, object aliasCandidate, out T resolvedCandidate, out string error )
		{
			// Attempt fetching the id.
			bool idResolved = false;
			try
			{
				// Try converting the alias candidate to id.
				resolvedCandidate = resolver( aliasCandidate );
				idResolved = true;
				error = "";
			}
			catch( Exception ex )
			{
				// Handle the error
				switch( context.ErrorReporting )
				{
				// Handle the exception.
				case Enumerations.ErrorReporting.Exception:
					throw;

				// Collect the exception.
				case Enumerations.ErrorReporting.Collect:
					resolvedCandidate = default( T );
					error = ex.Message;
					break;
				
				default:
					throw new InvalidOperationException( string.Format( "Unrecognized error reporting mode {0}.", context.ErrorReporting ));
				}
				if( context.ErrorReporting == Enumerations.ErrorReporting.Exception )
					throw;
			}
			return idResolved;
		}
	}
}
