using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using MFiles.Internal.Json;
using MFiles.SmartCard.Configuration;
using MFiles.PdfTools;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFilesAPI;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MFilesSmartCard
{

	/// <summary>
	/// Simple vault application to demonstrate VAF.
	/// </summary>
	public class VaultApplication : VaultApplicationBase
	{
		// Configuration of signatures.
		private SignerConfiguration config = new SignerConfiguration() { };

		// Configuration strings.
		private const string configKey = "config";
		private const string configNamespace = "M-Files.SmartCard";

		/// <summary>
		/// Properties exculed in comparison.
		/// </summary>
		private static readonly HashSet<int> ExcludedProperties = new HashSet<int>( new int[] {
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModified,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModifiedBy,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefStatusChanged,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefSizeOnServerThisVersion,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefVersionComment,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefState,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefStateTransition,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefStateEntered,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignedTo,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefAssignmentDescription,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefMonitoredBy,
			( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefDeadline
		} );

		/// <summary>
		/// Validate signatures at AfterCheckInChanges from ObjIDs in this dictionary.
		/// </summary>
		private ConcurrentDictionary<string, bool> objIDsToValidateSignature = new ConcurrentDictionary<string, bool>();

		/// <summary>
		/// Security provider for handling the signatures with PDF Tools.
		/// </summary>
		private string Provider
		{
			get
			{
				// Get the provider.
				// Default to empty if not specified.
				if( this.config != null &&
					this.config.PdfTools != null &&
					!string.IsNullOrEmpty( this.config.PdfTools.Provider ) )
				{
					// Use the specified provider.
					return this.config.PdfTools.Provider;
				}
				else
				{
					// No provider specified, use default.
					return null;
				}
			}
		}

		protected override void InitializeApplication( Vault vault )
		{
			// Try to install the client application.
			try
			{
				vault.CustomApplicationManagementOperations.InstallCustomApplication( @"FileSigner.mfappx" );
			}
			catch( Exception ex )
			{
				// Report the error. Already exists error is expected if the same version of the client application was already installed.
				if( !MFUtils.IsMFilesAlreadyExistsError( ex ) )
					SysUtils.ReportErrorToEventLog( this.EventSourceIdentifier, ex );
			}
		}

		protected override void UninitializeApplication( Vault vault )
		{
			// Uninitialize PDF Tools.
			MFiles.PdfTools.ConfigurationManager.Uninitialize();
		}

		protected override void StartApplication()
		{
			// Load configuration from NamedValueStorage.
			LoadConfiguration( this.PermanentVault );
			
			// Ensure that PDFTools module has been initialized.
			MFiles.PdfTools.ConfigurationManager.Initialize();

			// Try updating the license if the configuration has been set.
			if( this.config != null &&
				this.config.PdfTools != null )
			{
				// Set license only if available and the if signature verification is required.
				string licenseKey = this.config.PdfTools.LicenseKey;
				if( !string.IsNullOrEmpty( licenseKey ) )
					Licensing.LicenseKey = this.config.PdfTools.LicenseKey;

			}  // end if

			// Set default license if the license is still not set.
			if( string.IsNullOrEmpty( Licensing.LicenseKey ) )
				Licensing.LicenseKey = Licensing.OemLicenseKey;

		}
		
		/// <summary>
		/// Method to load in the configuration.
		/// </summary>
		/// <param name="vault"> The document vault. </param>
		public void LoadConfiguration( Vault vault )
		{
			// Read the configuration. The configuration may contain comments which we need to strip here.
			NamedValues nvs = vault.NamedValueStorageOperations.GetNamedValues( MFNamedValueType.MFConfigurationValue, configNamespace );

			// Get configuration.
			string configuration = "";
			if( nvs.Contains( configKey ) )
				configuration = ( string )nvs[ configKey ];
			else
			{
				// Configuration not found, get from file and save to named value storage.
				configuration = System.IO.File.ReadAllText( @"MFiles.SmartCard.Configuration.json" );
				nvs[ configKey ] = configuration;
				vault.NamedValueStorageOperations.SetNamedValues( MFNamedValueType.MFConfigurationValue, configNamespace, nvs );
			}

			// Remove comments from configuration.
			configuration = Regex.Replace( configuration, @"(.*?)//.*$", @"$1", RegexOptions.Multiline );

			// Deserialize the configuration.
			// Deserialize json into configuraiton object
			var settings = new JsonSerializerSettings
				{
					Context = ConversionContext.CreateDeserializationContext( vault, Enumerations.ErrorReporting.Collect )
				};
			var deserializedConfiguration = JsonConvert.DeserializeObject<SignerConfiguration>( configuration, settings );

			// Verify we got the configuration.
			if( deserializedConfiguration == null )
				return;

			config = deserializedConfiguration;
		}

		[EventHandler( MFEventHandlerType.MFEventHandlerAfterSetProperties )]
		public void VerifySignedPDF( EventHandlerEnvironment env )
		{
			// Quick-check configuration need for this event handler.
			if( config == null ||
				config.Authentication == null )
				return;

			if( config.Authentication.SignPDFForStateTransitions == null )
				return;

			if( config.Authentication.SignPDFForStateTransitions.Count == 0 )
				return;

			// Check, if we are entering a state, that needs signing with smartcard.
			int newStateTrans;
			{
				// Try to get the state.
				int? statetrans = Helpers.TryGetStateTransition( env.PropertyValues );

				// No state, no need for signing.
				if( statetrans.HasValue == false )
					return;

				newStateTrans = statetrans.Value;
			}

			// Is validation done in this transition.
			bool signPDF = IsSignPDFDoneInThisStateTransition( newStateTrans );
			if( signPDF == false )
				return;

			// Check the validity of signatrue in PDF at AfterCheckInChanges.
			objIDsToValidateSignature.TryAdd( MFUtils.ObjIDString( env.ObjVer.ObjID ), true );

		}  // end event handler

		[EventHandler( MFEventHandlerType.MFEventHandlerAfterCheckInChanges )]
		[EventHandler( MFEventHandlerType.MFEventHandlerAfterCreateNewObjectFinalize )]
		public void VerifySignature( EventHandlerEnvironment env )
		{
			// Check, that we have some states, that need signing with smartcard.
			if( config == null ||
				config.Authentication == null )
				return;

			if( config.Authentication.RequiredForStates == null &&
				config.Authentication.SignPDFForStateTransitions == null )
				return;

			if( config.Authentication.RequiredForStates.Count == 0 &&
				config.Authentication.SignPDFForStateTransitions.Count == 0 )
				return;

			// Apply proper culture and continue with the verification.
			Vault vault = env.Vault;
			using( var tempCulture = new TemporaryCulture( vault, config.Locale ) )
			{
				// Get the data of the new version.
				ObjectVersionAndProperties newVersionAndProperties = vault.ObjectOperations.GetObjectVersionAndProperties( env.ObjVer );
				
				// Check, if we are entering a state, that needs signing with smartcard.
				int newState;
				{
					// Try to get the state.
					int? state = Helpers.TryGetState( newVersionAndProperties.Properties );
					
					// No state, no need for signing.
					if( state.HasValue == false )
						return;

					newState = state.Value;
				}

				// Is this a state that does not need signing.
				bool authenticateState = IsAuthenticationNeededForState( newState );
				bool validateSignPDF = objIDsToValidateSignature.ContainsKey( MFUtils.ObjIDString( env.ObjVer.ObjID ) );
				
				// No validation done in this state/transition.
				if( authenticateState == false && validateSignPDF == false )
					return;

				// Try-finally to clear objID from validation.
				try
				{
					// Get the data of the previous version.
					ObjectVersionAndProperties oldVersionAndProperties = Helpers.GetPreviousObjectVersionAndProperties( vault, newVersionAndProperties.ObjVer );

					// We need a check for a smartcard signature.
					// Prevent changes to any files or other metadata than state.
					VerifyOnlyStateChange( vault, newVersionAndProperties, oldVersionAndProperties );

					if( authenticateState == true )
					{
						// Check the existance of Evidence PDF.
						ObjectVersion evidencePDF = GetEvidencePDF( vault, oldVersionAndProperties.ObjVer );

						// Check the validity of Evidence PDF.
						VerifySignatureInPDF( vault, evidencePDF, env.CurrentUserID, oldVersionAndProperties.VersionData, config.ValidateSignature );

						// Finally hide the evidence PDF.
						this.HideEvidencePdf( vault, evidencePDF.ObjVer );
					}

					if( validateSignPDF == true )
					{
						// Validate file has not changed, validate signature if allowed via config.
						VerifySignatureInPDF( vault, newVersionAndProperties.VersionData, env.CurrentUserID, oldVersionAndProperties.VersionData, config.ValidateSignature );
					}
				}
				finally
				{
					// Clear objId from validation list.
					objIDsToValidateSignature.TryRemove( MFUtils.ObjIDString( env.ObjVer.ObjID ), out validateSignPDF );
				}
			
			}  // end using culture

		}  // end event handler

		/// <summary>
		/// Returns true, if authentication is needed for the given state.
		/// </summary>
		private bool IsAuthenticationNeededForState( int newState )
		{
			return config.Authentication.RequiredForStates.Any( item => item.Id == newState );
		}

		/// <summary>
		/// Returns true, if sign pdfis done  is needed for the given state.
		/// </summary>
		private bool IsSignPDFDoneInThisStateTransition( int statetrans )
		{
			return config.Authentication.SignPDFForStateTransitions.Any( item => item.Id == statetrans );
		}

		/// <summary>
		/// Verifies, that only excluded properties have been changed between two versions.
		/// </summary>
		private static void VerifyOnlyStateChange( Vault vault, ObjectVersionAndProperties newVersionAndProperties, ObjectVersionAndProperties oldVersionAndProperties )
		{
			// Get the properties of the current and previous version of the object.
			var newPropertyValues = GetPropertyValuesForComparison( newVersionAndProperties.Properties ).ToList();
			var oldPropertyValues = GetPropertyValuesForComparison( oldVersionAndProperties.Properties ).ToList();

			// Check, that there are no removed property values.
			// Note: This error is also shown to the user if he tries to edit the properties of the object and simultanously change the state from the metadata card.
			var propertyValueEqualityComparer = new PropertyValueEqualityComparer();
			var changedPropertyValues = oldPropertyValues.Except( newPropertyValues, propertyValueEqualityComparer ).ToList();
			PreventStateChangeIfNormalPropertyWasUpdated( vault, newVersionAndProperties.VersionData.Class, changedPropertyValues );

			// Check that no property definitions were added or removed.
			if( ! oldPropertyValues.Select( pv => pv.PropertyDef ).OrderBy( id => id ).SequenceEqual(
					newPropertyValues.Select( pv => pv.PropertyDef ).OrderBy( id => id ) ) )
			{
				// Property values were either added or removed.
				// Prevent the state transition.
				throw new Exception( String.Format( InternalResources.Error_SmartCardRequiredButPropertiesChanged_X, -1 ) );
			}
		}

		/// <summary>
		/// Prevents the state change if the user has changed a property value. Automatically calculated properties are ignoerd.
		/// </summary>
		/// <param name="vault">Vault access</param>
		/// <param name="classId">Class id</param>
		/// <param name="changedPropertyValues">Property values that were changed.</param>
		private static void PreventStateChangeIfNormalPropertyWasUpdated( Vault vault, int classId, IEnumerable< MFilesAPI.PropertyValue > changedPropertyValues )
		{
			// Check each property that was changed and ignore the change it is automatically calculated property value.
			Dictionary<int, MFilesAPI.PropertyDef> propertyDefsById = null;
			foreach( var changedPropertyValue in changedPropertyValues )
			{
				// Get property definitions.
				if( propertyDefsById == null )
					propertyDefsById = vault.PropertyDefOperations.GetPropertyDefs().Cast<PropertyDef>().ToDictionary( key => key.ID, value => value );

				// Get the property definition for the value that was changed.
				PropertyDef changedPropertyDef = null;
				if( !propertyDefsById.TryGetValue( changedPropertyValue.PropertyDef, out changedPropertyDef ) )
					throw new Exception( String.Format( InternalResources.Error_SmartCardRequiredButPropertiesChanged_X, changedPropertyDef.ID ) );

				// Prevent changes to the title only if it is not automatically calculated value.
				if( changedPropertyDef.ID == ( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle )
				{
					// Check if the name property of the class.
					var classOfObject = vault.ClassOperations.GetObjectClass( classId );
					if( classOfObject.NamePropertyDef != ( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefNameOrTitle )
					{
						// The name property of the class has been changed.
						// Ignore the difference if the current name propery is calculated automatically.
						PropertyDef namePropertyDef = null;
						if( !propertyDefsById.TryGetValue( classOfObject.NamePropertyDef, out namePropertyDef ) )
							throw new Exception( String.Format( InternalResources.Error_SmartCardRequiredButPropertiesChanged_X, changedPropertyDef.ID ) );

						// If the name property is updated automatically, then do not prevent the operation because of it.
						if( namePropertyDef.AutomaticValueType != MFAutomaticValueType.MFAutomaticValueTypeNone )
							continue;

					}  // end if

				}  // end if

				// Only prevent change if the update type is normal.
				if( changedPropertyDef.AutomaticValueType == MFAutomaticValueType.MFAutomaticValueTypeNone )
					throw new Exception( String.Format( InternalResources.Error_SmartCardRequiredButPropertiesChanged_X, changedPropertyDef.ID ) );

			}  // end foreach.
		}

		/// <summary>
		/// Returns the property values, that are not excluded.
		/// </summary>
		/// <param name="propertyValues">Property values that will be compared</param>
		/// <returns>Property values with properties that cannot be compared removed.</returns>
		private static IEnumerable<MFilesAPI.PropertyValue> GetPropertyValuesForComparison( PropertyValues propertyValues )
		{
			// Return property values that we can use in comparison.
			// Property values that always change regardless of the user actions are ignored.
			foreach( MFilesAPI.PropertyValue propertyValue in propertyValues )
			{
				// Can we compate this property?
				if( ExcludedProperties.Contains( propertyValue.PropertyDef ) == false )
					yield return propertyValue;
			}
		}

		/// <summary>
		/// Returns the Evidence PDF, that refers to the given version of the object.
		/// </summary>
		/// <param name="vault">Vault interface</param>
		/// <param name="objVer">Object version for which the PDF Evidence object was created.</param>
		/// <returns>Returns the object version of the PDF Evidence object.</returns>
		private static ObjectVersion GetEvidencePDF( Vault vault, ObjVer objVer )
		{
			// Not deleted.
			var conditions = new SearchConditions();
			conditions.Add( -1, Helpers.CreateNotDeletedCondition() );

			// Class has alias MFiles.SmartCard.Class.Signature.
			{
				int classID = vault.ClassOperations.GetObjectClassIDByAlias( "MFiles.SmartCard.Class.Signature" );
				conditions.Add( -1, Helpers.CreateClassSearchCondition( classID ) );
			}

			// Has the reference to the checked in version of the object.
			{
				int defaultPropDefIDForObjectType = vault.ObjectTypeOperations.GetObjectType( objVer.Type ).DefaultPropertyDef;
				conditions.Add( -1, Helpers.CreateLookupSearchCondition( defaultPropDefIDForObjectType, Helpers.ObjVerToLookup( objVer ) ) );
			}

			// Find the specific version.
			var searchflags = MFSearchFlags.MFSearchFlagDisableRelevancyRanking;
			ObjectSearchResults results = vault.ObjectSearchOperations.SearchForObjectsByConditions( conditions, searchflags, false );

			// Check, that we have results.
			if( results.Count < 1 )
				throw new Exception( InternalResources.Error_PDFEvidenceObjectMissing );

			// Check, that we have a single result.
			if( results.Count > 1 )
				throw new Exception( InternalResources.Error_MultiplePDFEvidenceObjectsFound );

			// The result is the evidence pdf.
			return results[ 1 ];
		}

		/// <summary>
		/// Verifies, that the Evidence PDF is signed with smartcard by the current user.
		/// </summary>
		/// <param name="vault">Vault interface</param>
		/// <param name="evidencePdf">PDF Evidence object which is verified</param>
		/// <param name="currentUserID">The id of the user performing the operation. This user should have signed the PDF.</param>		
		/// <param name="signedObject">The objver of the object that was signed.</param>
		private void VerifySignatureInPDF( Vault vault, ObjectVersion evidencePdf, int currentUserID, ObjectVersion signedObject, bool validateSignature )
		{
			// Valid Evidence PDF has exactly one file.
			if( evidencePdf.FilesCount != 1 )
				throw new Exception( InternalResources.Error_PDFEvidenceNotSingleFileDocument );

			// Download the file for signature verification.
			using( var tempFile = VerifyFileContentChanged( vault, evidencePdf ) )
			{
				if( !validateSignature )
					return;

				// Verify the signature.
				using( var pdf = new PdfSecurity( tempFile.FilePath, this.Provider ) )
				{
					// Get information about the current user.
					var login = vault.UserOperations.GetLoginAccountOfUser( currentUserID );

					// Create placeholders for verification.
					var placeholders = new Dictionary<string, string>();
					placeholders.Add( "accountName", login.AccountName );
					placeholders.Add( "username", GetUsername( login.AccountName ) );
					string domain = GetDomain( login.AccountName );
					if( !string.IsNullOrEmpty( domain ) )
						placeholders.Add( "domain", domain );

					// Get the expected reason.
					var expectedReason = MFiles.PdfTools.UIExt.Helper.GetReasonForSignatureFromObjectVersion( signedObject );
										
					// Get validation result from the first config that matches the signature.
					ValidationResult result = null;
					string signedBy = null;
					foreach( var certificateConfig in config.Authentication.Certificates )
					{
						// Get common name of the issuer.
						string commonNameOfIssuer = certificateConfig.GetCommonNameOfIssuer();

						// Resolve the contact info of the user who must have signed the PDF.
						var subject = certificateConfig.SubjectRfc822Name;
						signedBy = subject.ResolveValue( placeholders );

						// The signature must have been signed by the correct user.
						result = pdf.ValidateSignature( commonNameOfIssuer, expectedReason, signedBy, subject.IsRegularExpression );

						// Match found. Stop searching.
						if( result.IsValid )
							break;
					}
					
					// Throw exception if the signature is not valid. Use the result from the last config.
					// TODO: Use the results from all configs.
					if( ! result.IsValid )
					{
						// The signature is not valid. Determine the error based on the presence of valid digital signatures.
						// If there are no valid signatures we assume that the user launched the operation from the metadata card.
						// Reasoning for this:
						//  1. User starts making the state transition requiring signature from the task pane. 
						//		* The object has been already signed once before. So the signature object already exists.
						//  2. A new version of the PDF Evidence object is created invalidating the previous signatures.
						//  3. PIN prompt is shown to the user.
						//  4. User cancels the signing.
						//  5. User activates the state transition from the metadata card.
						// Now at this point the PDF Evidence object exists but it does not have any valid signatures.
						// => The validation fails. But in this particular situation we also want to guide the user to use the correct command.
						//
						// However, if the PDF has some valid signatures then we display an erro explaining that no valid signature
						// signed by the current user was found.
						if( result.ValidSignatures == 0 )
							throw new Exception( InternalResources.Error_PDFEvidenceObjectInvalid );
						else
						{
							throw new SignatureVerificationFailedException(
									string.Format( InternalResources.Error_PDFSignatureVerificationFailed, signedBy ) );
						}

					}  // end if

				}  // end using

			}  // end using
		}

		/// <summary>
		/// Hides the evidence PDF from the user.
		/// </summary>
		/// <param name="vault">Vault access</param>
		/// <param name="evidencePdf">Evidence PDF that will be hidden.</param>
		private void HideEvidencePdf( Vault vault, ObjVer evidencePdf )
		{
			// Hide the PDF.
			var emptyAcl = new AccessControlList();
			vault.ObjectOperations.ChangePermissionsToACL( evidencePdf, emptyAcl, true );
		}

		/// <summary>
		/// Verifies that the file content has changed.
		/// </summary>
		/// <param name="vault">Vault access</param>
		/// <param name="evidencePdf">Evidence PDS</param>
		/// <returns>The latest file for signature verification.</returns>
		private static TempFile VerifyFileContentChanged( Vault vault, ObjectVersion evidencePdf )
		{
			// The file has to be PDF.
			ObjectFile file = evidencePdf.Files[ 1 ];
			if( file.Extension.ToLowerInvariant() != "pdf" )
				throw new Exception( InternalResources.Error_PDFEvidenceMustBePDF );

			// Verify that the file has changed.
			TempFile latestEvidencePdf = new TempFile();
			try
			{
				// Download the file for signature verification.
				vault.ObjectFileOperations.DownloadFile( file.ID, file.Version, latestEvidencePdf.FilePath );

				// Re-using old evidence PDF?
				if( evidencePdf.ObjVer.Version > 1 )
				{
					// Verify that the evidence PDF was changed to the latest version.
					// => Prevent the re-use of the evidence PDF with valid signature by a simple metadata modification.
					using( var previousEvidenecPdfFile = new TempFile() )
					{
						// Download the previous version.
						var previousObjVer = evidencePdf.ObjVer.Clone();
						previousObjVer.Version = previousObjVer.Version - 1;
						var previousEvidencePdf = vault.ObjectOperations.GetObjectInfo( previousObjVer, false );
						if( previousEvidencePdf.FilesCount != 1 )
							throw new Exception( InternalResources.Error_PDFEvidenceObjectInvalid );
						var previousFile = previousEvidencePdf.Files[ 1 ];
						vault.ObjectFileOperations.DownloadFile( previousFile.ID, previousFile.Version, previousEvidenecPdfFile.FilePath );

						// Compare.
						if( latestEvidencePdf.ContentEquals( previousEvidenecPdfFile ) )
							throw new Exception( InternalResources.Error_PDFEvidenceObjectInvalid );

					}  // end using

				}  // end if
			}
			catch( Exception )
			{
				// Cleanup.
				latestEvidencePdf.Dispose();
				throw;
			}
			return latestEvidencePdf;
		}

		/// <summary>
		/// Gets the configuration for the client. Aliases are resolved.
		/// </summary>
		/// <param name="env">The EventHandlerEnvironment.</param>
		/// <returns></returns>
		[VaultExtensionMethod( "M-Files.SmartCard.GetConfigurationForClient", RequiredVaultAccess = MFVaultAccess.MFVaultAccessNone )]
		public string GetClientConfig( EventHandlerEnvironment env )
		{
			// No configuration if the module is not enabled.
			if( !config.Enabled )
				return "";

			// Make a copy of the configuration.
			SignerConfiguration configuration = null;
			{
				// Serialize the configuration.
				var serialize = new JsonSerializerSettings
					{
						Context = ConversionContext.CreateSerializationContext( env.Vault, true )
					};
				string str = JsonConvert.SerializeObject( config, serialize );

				// Deserialize the configuration to create the copy.
				var deserialize = new JsonSerializerSettings
					{
						Context = ConversionContext.CreateDeserializationContext( env.Vault, Enumerations.ErrorReporting.Exception )
					};
				configuration = JsonConvert.DeserializeObject< SignerConfiguration >( str, deserialize );
			}

			// Add PDF Tools section if it is missing.
			if( configuration.PdfTools == null )
			{
				configuration.PdfTools = new MFiles.PdfTools.UIExt.Configuration.PdfTools
					{
						LicenseKey = Licensing.LicenseKey
					};

			}  // end if

			// Resolve every transition from vault.
			Dictionary<int, List<int>> transitionsByState = ResolveTransitionsOfStates( env.Vault );
			
			// Resolve the state transitions that can move objects into states which require signatures.
			{
				var transitionsRequiringSignature = new List<int>();
				foreach( var state in config.Authentication.RequiredForStates )
				{
					// Skip invalid states.
					if( state.IsValid == false )
						continue;

					// Get the transitions which lead to a state which requires signatures. Those transitions require signature.
					List<int> transitionsToState;
					if( transitionsByState.TryGetValue( state.Id, out transitionsToState ) )
						transitionsRequiringSignature.AddRange( transitionsToState );

				}  // end foreach.

				// Add the transitions to the configuration.
				if( configuration.Authentication.RequiredForStateTransitions == null )
				{
					// Transition configuration does not exist. Create it.
					configuration.Authentication.RequiredForStateTransitions = transitionsRequiringSignature
										.Select( t => new StructureItem( t ) )
										.ToList();
				}
				else
				{
					// Transition configuration exists. Add the implicit transitions to it.
					configuration.Authentication.RequiredForStateTransitions.AddRange( transitionsRequiringSignature
										.Select( t => new StructureItem( t ) ) );

				}  // end if
			}

			// Read the configuration.
			string serializedClientConfiguration;
			{
				var settings = new JsonSerializerSettings
				{
					Context = ConversionContext.CreateSerializationContext( env.Vault, true )
				};
				serializedClientConfiguration = JsonConvert.SerializeObject( configuration, settings );
			}
			return serializedClientConfiguration;
		}

		/// <summary>
		/// Resolves all state transitions that lead the specific states.
		/// </summary>
		/// <param name="vault">Access to the vault.</param>
		/// <returns>State transitions leading to a state by the state.</returns>
		private static Dictionary<int, List<int>> ResolveTransitionsOfStates( MFilesAPI.Vault vault )
		{
			// Read all workflows and state transitions within them.
			Dictionary<int, List<int>> transitionsByState = new Dictionary<int, List<int>>();
			foreach( MFilesAPI.WorkflowAdmin workflow in vault.WorkflowOperations.GetWorkflowsAdmin() )
			{
				// Read the transitions from the workflow.
				foreach( MFilesAPI.StateTransition transition in workflow.StateTransitions )
				{
					// Get the list of transitions leading to the target state of the current transitions.
					List< int > transitions = null;
					if( !transitionsByState.TryGetValue( transition.ToState, out transitions ) )
					{
						// Create new transition collection for the state.
						transitions = new List<int>();
						transitionsByState[ transition.ToState ] = transitions;
					}

					// Collect the id of the transition.
					transitions.Add( transition.ID );

				}  // end foreach

			}  // end foreach.

			return transitionsByState;
		}

		/// <summary>
		/// Gets the username from the account name.
		/// </summary>
		/// <param name="accountName">The account name</param>
		/// <returns>Username</returns>
		private static string GetUsername( string accountName )
		{
			// Extract the username from the account name.
			var domainSeparator = accountName.IndexOf( '\\' );
			string username;
			if( domainSeparator != -1 )
				username = accountName.Substring( domainSeparator + 1 );
			else
				username = accountName;

			// Return the username.
			return username;
		}

		/// <summary>
		/// Gets the domain from the account name.
		/// </summary>
		/// <param name="accountName">The account name</param>
		/// <returns>Username</returns>
		private static string GetDomain( string accountName )
		{
			// Extract the username from the account name.
			var domainSeparator = accountName.IndexOf( '\\' );
			string domain;
			if( domainSeparator != -1 )
				domain = accountName.Substring( 0, domainSeparator );
			else
				domain = null;

			// Return the username.
			return domain;
		}

	}
}