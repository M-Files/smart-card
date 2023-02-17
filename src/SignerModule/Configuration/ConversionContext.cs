using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MFiles.Internal.Json;

namespace MFiles.SmartCard.Configuration
{
	/// <summary>
	/// 
	/// </summary>
	internal class ConversionContext : IConversionContext, IAliasResolver
	{
		/// <summary>
		/// Vault object.
		/// </summary>
		private MFilesAPI.Vault Vault { get; set; }

		/// <summary>
		/// Initializes new conversion context.
		/// </summary>
		/// <param name="vault">Vault for performing the necessary operations.</param>
		/// <param name="resolveAliases">Are aliases resolved during this conversion?</param>
		public static StreamingContext CreateSerializationContext( MFilesAPI.Vault vault, bool resolveAliases )
		{
			var context = new StreamingContext( StreamingContextStates.Other, new ConversionContext( vault, resolveAliases, Enumerations.ErrorReporting.Exception ) );
			return context;
		}

		/// <summary>
		/// Initializes new conversion context for deserializing the json configuration.
		/// </summary>
		/// <param name="vault">Vault for performing the necessary operations.</param>		
		/// p<param name="errorReporting">How errors are reported.</param>
		public static StreamingContext CreateDeserializationContext( MFilesAPI.Vault vault, Enumerations.ErrorReporting errorReporting )
		{
			var context = new StreamingContext( StreamingContextStates.Other, new ConversionContext( vault, true, errorReporting ) );
			return context;
		}

		/// <summary>
		/// Initializes new conversion context.
		/// </summary>		
		public static StreamingContext CreateSyntaxValidationContext()
		{
			var context = new StreamingContext( StreamingContextStates.Other, new ConversionContext( null, false, Enumerations.ErrorReporting.Exception ) );
			return context;
		}

		/// <summary>
		/// Initializes new conversion context.
		/// </summary>
		/// <param name="vault">Vault for performing the necessary operations.</param>
		/// <param name="resolveAliases">Are aliases resolved during this conversion?</param>
		private ConversionContext( MFilesAPI.Vault vault, bool resolveAliases, Enumerations.ErrorReporting errorReporting )
		{
			this.Vault = vault;
			this.ResolveAliases = resolveAliases;
			this.ErrorReporting = errorReporting;
		}

		public bool ResolveAliases { get; private set; }

		public IAliasResolver AliasResolver { get { return this; } }

		public Enumerations.ErrorReporting ErrorReporting { get; private set; }

		public int GetObjectTypeIdByAlias( string alias )
		{
			// Delegate to vault.
			return this.Vault.ObjectTypeOperations.GetObjectTypeIDByAlias( alias );
		}

		public int GetPropertyDefIdByAlias( string alias )
		{
			// Delegate to vault.
			return this.Vault.PropertyDefOperations.GetPropertyDefIDByAlias( alias );
		}

		public int GetWorkflowStateIdByAlias( string alias )
		{
			// Delegate to vault.
			return this.Vault.WorkflowOperations.GetWorkflowStateIDByAlias( alias );
		}

		public int GetWorkflowStateTransitionIdByAlias( string alias )
		{
			// Delegate to vault.
			return this.Vault.WorkflowOperations.GetWorkflowStateTransitionIDByAlias( alias );
		}
	}
}
