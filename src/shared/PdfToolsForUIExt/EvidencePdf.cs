using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MFiles.PdfTools.UIExt.Exceptions;
using Newtonsoft.Json.Linq;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Represents the PDF evidence object.
	/// </summary>
	internal class EvidencePdf : IDisposable
	{
		/// <summary>
		/// Vault for manipulating the evidence.
		/// </summary>
		private dynamic vault;

		/// <summary>
		/// The object version of the evidence PDF.
		/// </summary>
		internal dynamic CheckedOutObjVer { get; private set; }

		/// <summary>
		/// The checked out ObjVer as a string.
		/// </summary>
		internal string DocumentObjectVersionAsString { get; private set; }

		/// <summary>
		/// Path to the evidence PDF.
		/// </summary>
		internal string Path { get; private set; }

		/// <summary>
		/// Initializes new evidence PDF object.
		/// </summary>
		/// <param name="vault">Vault</param>
		/// <param name="objectVersion">Object version.</param>
		/// <param name="documentObjectVersion">The object version of the document for which the evidence PDF was created.</param>
		/// <param name="path">Path to the PDF file</param>
		private EvidencePdf( dynamic vault, dynamic objectVersion, dynamic documentObjectVersion, string path )
		{
			this.vault = vault;
			this.CheckedOutObjVer = objectVersion.ObjVer;
			this.DocumentObjectVersionAsString = Helper.GetReasonForSignatureFromObjectVersion( documentObjectVersion );
			this.Path = path;
		}

		/// <summary>
		/// Finalizes the evidence PDF.
		/// </summary>
		/// <param name="signingTask">Pdf signing task</param>
		/// <param name="progressBarDialog">Progress dialog for reporting progress.</param>
		internal void Finalize( Task<ErrorObject> signingTask, Progress progressBarDialog, bool bCreateEvidencePDF )
		{
			// Capture possible error.
			var errorObject = signingTask.Result;

			// Invoke in the UI thread.
			progressBarDialog.Invoke( (MethodInvoker) ( () =>
				{
					// Check in, if successful.			
					if( errorObject == null || errorObject.ErrorCode == ApplicationErrorCodes.Success )
					{
						// Check in the evidence pdf.
						progressBarDialog.SetPhase( Progress.ProgressPhase.FinalizingEvidencePDF );
						if( bCreateEvidencePDF == true )
							vault.ObjectOperations.CheckIn( this.CheckedOutObjVer );
						
						Application.DoEvents();

						// Check in successful.
						this.CheckedOutObjVer = null;
					}
					else
					{
						// Undo checkout for evidence pdf, if check in was not successful.
						if( this.CheckedOutObjVer != null && bCreateEvidencePDF == true )
						{
							// Undo the attempted change.
							dynamic reverted = vault.ObjectOperations.UndoCheckout( this.CheckedOutObjVer );
							Application.DoEvents();
							this.CheckedOutObjVer = null;

							// Hide the evidence PDF from the user.
							Helper.HideObject( vault, reverted.ObjVer );
						}
						
					}  // end if

				} ) );  // end invoke
		}
		/// <summary>
		/// Creates evidence PDF for the specified object.
		/// </summary>
		/// <param name="vault">Vault</param>
		/// <param name="documentObjVer">Object for which the PDF evidence object is created.</param>
		/// <returns>Evidence PDF</returns>
		internal static EvidencePdf CreateFor( dynamic vault, dynamic documentObjVer, bool bCreateEvidencePDF )
		{
			// Initialize to point current object.
			dynamic pdfObjVer = documentObjVer;

			// Request new evidence PDF from the server.
			if( bCreateEvidencePDF == true )
				pdfObjVer = createEvidencePDF( vault, documentObjVer );

			// Checkout the evidence for modification.
			dynamic checkedOutPDFObjectVersion = null;
			try
			{
				// Check out the created evidence pdf for signature.
				if( bCreateEvidencePDF == true )
					checkedOutPDFObjectVersion = vault.ObjectOperations.CheckOut( pdfObjVer.ObjID );
				else
					checkedOutPDFObjectVersion = vault.ObjectOperations.GetObjectInfo( pdfObjVer, true, true );

				Application.DoEvents();

				// Check, that checked out pdf object version is the checked out version of the created evidence pdf.
				if( checkedOutPDFObjectVersion.LatestCheckedInVersion != pdfObjVer.Version )
					throw new Exception( InternalResources.Error_EvidencePDFChangedAfterCreation );

				// Get object version data for the object which the evidence PDF was created.
				dynamic documentObjectVersion = vault.ObjectOperations.GetObjectInfo( documentObjVer, false );
				Application.DoEvents();

				// Get path to the checked out evidence pdf.
				string pdfPath = getPathToTheFileToSign( vault, checkedOutPDFObjectVersion );
				return new EvidencePdf( vault, checkedOutPDFObjectVersion, documentObjectVersion, pdfPath );
			}
			catch( Exception )
			{
				// Cleanup.
				if( checkedOutPDFObjectVersion != null )
				{
					// Undo the attempted signing.
					dynamic reverted = vault.ObjectOperations.UndoCheckout( checkedOutPDFObjectVersion.ObjVer );
					Application.DoEvents();

					// Hide the evidence PDF from the user.
					if( bCreateEvidencePDF == true )
						Helper.HideObject( vault, reverted.ObjVer );
				}

				throw;
			}			
		}

		/// <summary>
		/// Creates an evidence pdf for the given object version.
		/// </summary>
		/// <param name="vault">The M-Files Vault Object.</param>
		/// <param name="objVer">The ObjVer of the object version, for which the evidence pdf is created.</param>
		/// <returns>The ObjVer of the created evidence pdf.</returns>
		private static dynamic createEvidencePDF( dynamic vault, dynamic objVer )
		{
			// Prepare parameters for creating the evidence pdf with combo rendition.
			string parameters = String.Format( (string) "[\"ComboRendition.CreateRendition\", \"{0}\",\"{1}\"]", (object) Helper.ObjVerToString( objVer ), (object) "Rendition" );

			// Execute the extension method to create the evidence pdf.
			string result = vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod( "MFiles.ComplianceKit.MFEventHandlerVaultExtensionMethod", parameters );
			Application.DoEvents();

			// Check validity of result. Empty is returned if e.g. PDF processor is not configured.
			if( result.Length == 0 )
				throw new Exception( InternalResources.Error_CKNotEnabled );

			// Parse json to dynamic object.
			dynamic d = JObject.Parse( result );

			// Clone the existing ObjVer to get another ObjVer.
			dynamic pdfObjVer = objVer.Clone();
			Application.DoEvents();

			// Replace all the values of the clone with the values of the dynamic json object.
			pdfObjVer.Type = d.rendition.Type;
			pdfObjVer.ID = d.rendition.ID;
			pdfObjVer.Version = d.rendition.Version;
			Application.DoEvents();

			// Ensure that the rendition refers to the version for which we requested the rendition to be created.
			dynamic pdfProperties = vault.ObjectPropertyOperations.GetProperties( pdfObjVer, true );
			Application.DoEvents();

			// Locate the property refering to this object.
			int referredVersion = getEvidencePdfReferredObjectVersion( pdfProperties, objVer.ObjID );
			if( referredVersion != objVer.Version )
				throw new NewObjectVersionException();

			// Return the changed ObjVer.
			return pdfObjVer;
		}

		/// <summary>
		/// Returns the path in default view to the first file of the given object version.
		/// </summary>
		private static string getPathToTheFileToSign( dynamic vault, dynamic objectVersion )
		{
			// Ensure, that we have a file.
			if( objectVersion.FilesCount != 1 )
				throw new Exception( InternalResources.Error_PDFNotSingleFileDocument );

			// Get filever of the first file.
			dynamic objVer = objectVersion.ObjVer;
			dynamic fileVer = objectVersion.Files[ 1 ].FileVer;
			Application.DoEvents();

			// Return the path in default view.
			var path = vault.ObjectFileOperations.GetPathInDefaultView( objVer.ObjID, objVer.Version, fileVer.ID, fileVer.Version, 1, false );
			Application.DoEvents();
			return path;
		}

		/// <summary>
		/// Gets the version of the object that is referenced by the evidence PDF.
		/// </summary>
		/// <param name="properties">Referring property values of the evidence PDF</param>
		/// <param name="objId">Referenced object</param>
		/// <returns>The version of the object was referenced in the given property value collection or -1 if referring to the latest version.</returns>
		private static int getEvidencePdfReferredObjectVersion( dynamic properties, dynamic objId )
		{
			// Search through the property values and try find a property value that references the object.
			int propertyCount = properties.Count;
			for( int i = 1; i <= propertyCount; i++ )
			{
				// Keep pumping.
				Application.DoEvents();

				// Only lookups are interesting.
				dynamic property = properties[ i ];
				dynamic typedValue = property.TypedValue;
				if( typedValue.DataType == 9 ||  // Single-select lookup.
					typedValue.DataType == 10 )  // Multi-select lookup.
				{
					// Get the version the object is referring.
					dynamic lookups = typedValue.GetValueAsLookups();
					int referredVersion = getReferredObjectVersionFromLookups( lookups, objId );
					if( referredVersion == 0 )  // Not referenced?
						continue;

					// Found a reference. Return the version.
					return referredVersion;
				}
				
			}  // end for

			// The properties were not referenced.
			throw new ApplicationException( string.Format( "Evidence PDF did not include reference to the object {0}.", objId ) );
		}

		/// <summary>
		/// Gets the object version which the lookups are referring or 0 if not referenced by the given lookups.
		/// </summary>
		/// <param name="lookups">Referring lookups</param>
		/// <param name="objId">Referenced object</param>
		/// <returns>The version of the object that was referenced or -1 if referring to the latest version. 0 if the object was not found.</returns>
		private static int getReferredObjectVersionFromLookups( dynamic lookups, dynamic objId )
		{
			// Do the lookups refer to an object that looks like our object?
			int referencingLookupIndex = lookups.GetLookupIndexByItem( objId.ID );
			if( referencingLookupIndex == -1 )
				return 0;

			// Ensure object types match.
			dynamic referencingLookup = lookups[ referencingLookupIndex ];
			if( referencingLookup.ObjectType != objId.Type )
				return 0;

			// Return the version.
			int version = referencingLookup.Version;
			return version;
		}

		public void Dispose()
		{
			// Undo checkout.
			if( this.vault != null && this.CheckedOutObjVer != null )
			{
				// Undo the attempted signing.
				dynamic reverted = this.vault.ObjectOperations.UndoCheckout( this.CheckedOutObjVer );

				// Hide the evidence PDF from the user.
				Helper.HideObject( vault, reverted.ObjVer );
			}
		}
	}
}
