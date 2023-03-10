//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MFiles.PdfTools.UIExt {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class InternalResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InternalResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MFiles.PdfTools.UIExt.InternalResources", typeof(InternalResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sign with smart card.
        /// </summary>
        internal static string Commands_SignWithSmartCard {
            get {
                return ResourceManager.GetString("Commands_SignWithSmartCard", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find a compatible certificate. Remove and reinsert your card, and then try again..
        /// </summary>
        internal static string Error_CertificateNotFound {
            get {
                return ResourceManager.GetString("Error_CertificateNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The M-Files Compliance Kit application has not been configured..
        /// </summary>
        internal static string Error_CKNotEnabled {
            get {
                return ResourceManager.GetString("Error_CKNotEnabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The configuration option &apos;Certificate.IssuedBy&apos; has not been set. Specify the certificate authority in order for M-Files SmartCard to be able to select the certificate from the smart card..
        /// </summary>
        internal static string Error_Configuration_IssuedByMissing {
            get {
                return ResourceManager.GetString("Error_Configuration_IssuedByMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The PDF has been changed after it was created and cannot be used for signing..
        /// </summary>
        internal static string Error_EvidencePDFChangedAfterCreation {
            get {
                return ResourceManager.GetString("Error_EvidencePDFChangedAfterCreation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is a newer version of the object on the server than the version you are signing..
        /// </summary>
        internal static string Error_NewObjectVersionExists {
            get {
                return ResourceManager.GetString("Error_NewObjectVersionExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There is a more recent version available on the server. Refresh the view and reselect the object..
        /// </summary>
        internal static string Error_NewVersionAvailable {
            get {
                return ResourceManager.GetString("Error_NewVersionAvailable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The M-Files SmartCard application has not been enabled..
        /// </summary>
        internal static string Error_NotEnabled {
            get {
                return ResourceManager.GetString("Error_NotEnabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signing was interrupted. Signed file is not PDF..
        /// </summary>
        internal static string Error_NotInPDFFormat {
            get {
                return ResourceManager.GetString("Error_NotInPDFFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signing failed. The object is checked out..
        /// </summary>
        internal static string Error_ObjectCheckedOut {
            get {
                return ResourceManager.GetString("Error_ObjectCheckedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Object is not a single-file document..
        /// </summary>
        internal static string Error_PDFNotSingleFileDocument {
            get {
                return ResourceManager.GetString("Error_PDFNotSingleFileDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signing was interrupted. The certificate could not be used..
        /// </summary>
        internal static string Error_PrivateKeyNotFound {
            get {
                return ResourceManager.GetString("Error_PrivateKeyNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signing the PDF failed. Unexpected error &apos;{0}&apos;, error code &apos;{1}&apos;..
        /// </summary>
        internal static string Error_SigningFailedWithUnexpectedError_X_Code_Y {
            get {
                return ResourceManager.GetString("Error_SigningFailedWithUnexpectedError_X_Code_Y", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The placeholder &apos;{0}&apos; is not supported..
        /// </summary>
        internal static string Error_UnrecognizedPlaceholder {
            get {
                return ResourceManager.GetString("Error_UnrecognizedPlaceholder", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Initializing smart card authentication for {0}....
        /// </summary>
        internal static string ProgressPhase_CreateEvidencePDF {
            get {
                return ResourceManager.GetString("ProgressPhase_CreateEvidencePDF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Finalizing smart card authentication for {0}....
        /// </summary>
        internal static string ProgressPhase_FinalizingEvidencePDF {
            get {
                return ResourceManager.GetString("ProgressPhase_FinalizingEvidencePDF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Waiting for smart card authentication for {0}....
        /// </summary>
        internal static string ProgressPhase_SignEvidencePDF {
            get {
                return ResourceManager.GetString("ProgressPhase_SignEvidencePDF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown progress phase &apos;{0}&apos;..
        /// </summary>
        internal static string ProgressPhase_Unknown {
            get {
                return ResourceManager.GetString("ProgressPhase_Unknown", resourceCulture);
            }
        }
    }
}
