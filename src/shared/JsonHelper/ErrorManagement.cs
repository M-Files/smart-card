using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// General error managment helper functions.
	/// </summary>
	public static class ErrorManagement
	{
		/// <summary>
		/// Represents single configuration option.
		/// </summary>
		private struct JsonValue
		{
			/// <summary>
			/// Path to this option.
			/// </summary>
			public string Path;

			/// <summary>
			/// Configuration value at this position.
			/// </summary>
			public object Value;

			/// <summary>
			/// Possible error related to this configuration option.
			/// </summary>
			public string Error;

			/// <summary>
			/// True if this option is known to be a leaf node.
			/// </summary>
			public bool Leaf;
		}

		/// <summary>
		/// Gathers all errors from the given JSON structure.
		/// </summary>
		/// <param name="structure">JSON structure from which the errors are collected.</param>
		/// <returns>Errors</returns>
		public static List<ValidationError> GatherErrors( object structure )
		{
			// Check arguments.
			if( ! IsSerializable( structure ) )
				throw new ArgumentException( "Object is not serializable.", "structure" );

			// Process the structure
			var root = new JsonValue {Value = structure};
			var options = new Stack< JsonValue >();
			options.Push( root );
			var errors = new List< ValidationError >();
			do
			{
				// Process all child items of those values that may have children.
				var current = options.Pop();				
				if( ! current.Leaf )
				{
					// Collect children for processing.
					foreach( var child in GetChildValues( current ) )
					{
						options.Push( child );
					}
				}					

				// Collect errors.
				if( ! string.IsNullOrEmpty( current.Error ) )
				{
					var error = new ValidationError {ErrorMessage = current.Error, Path = current.Path};
					errors.Add( error );
				}

			} while( options.Count > 0 );

			// Return the errors.
			return errors;
		}

		/// <summary>
		/// Gets child values
		/// </summary>
		/// <param name="option"></param>
		/// <returns></returns>
		private static IEnumerable< JsonValue > GetChildValues( JsonValue option )
		{
			// Process all chiled properties.
			foreach( var property in GetProperties( option.Value ) )
			{
				// Construct path to the current property.
				var path = property.Name;
				if( ! string.IsNullOrEmpty( option.Path ))
					path = string.Concat( option.Path, ".", path );

				// Read the value.
				var propertyValue = property.GetValue( option.Value, null );
				if( propertyValue == null )
				{
					// Empty / null value.
					yield return new JsonValue {Path = path, Leaf = true, Value = null};
					continue;
				}

				// Try reading the possible error.
				var validatedProperty = propertyValue as IValidatedValue;				
				if( validatedProperty != null )
				{
					// Read the error.
					string errorMessage = "";
					if( !validatedProperty.IsValid )
						errorMessage = validatedProperty.ErrorMessage;

					// Return the value.
					yield return new JsonValue {Error = errorMessage, Path = path, Value = propertyValue};
				}
				else if( typeof( IEnumerable< IValidatedValue > ).IsAssignableFrom( propertyValue.GetType() ) )
				{
					// Read possible errors that occurred while parsing the current value.
					var configurationCollection = (IEnumerable< IValidatedValue >) propertyValue;
					var error = configurationCollection.Where( vc => ! vc.IsValid ).Select( vc => vc.ErrorMessage ).Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ? next : string.Concat( current, "; ", next ) );

					// Return the value.
					yield return new JsonValue {Error = error, Path = path, Leaf = true, Value = propertyValue};
				}
				else
				{
					// Non-validated value
					yield return new JsonValue {Path = path, Value = propertyValue};

				}  // end if

			}  // end if.
		}

		/// <summary>
		/// Gets value related properties of the configuration .
		/// </summary>
		/// <param name="value">Value from which to read the properties.</param>
		private static IEnumerable< PropertyInfo > GetProperties( object value )
		{
			// Get the child properties.
			var childProperties = value.GetType()
									.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy )  // Only public properties
									.Where( p => p.GetCustomAttributes( false ).Any( attr =>  // Only properties that are actually marked as serializable.
																					attr is DataMemberAttribute || attr is JsonContainerAttribute ) );
			return childProperties;
		}

		/// <summary>
		/// Checks if the specified object is marked as serializable.
		/// </summary>
		/// <param name="objectToCheck">The object to check.</param>
		/// <returns>True if the object is serializable.</returns>
		private static bool IsSerializable( object objectToCheck )
		{
			// The object is serializable if it represents data contract.
			bool serializable = objectToCheck.GetType().GetCustomAttributes( true ).Any( attr => attr is DataContractAttribute );
			return serializable;
		}
	}
}
