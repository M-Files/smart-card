//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MFilesSmartCard {
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
    public class InternalResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InternalResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MFilesSmartCard.InternalResources", typeof(InternalResources).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The configuration is invalid..
        /// </summary>
        public static string Error_Configuration_Invalid {
            get {
                return ResourceManager.GetString("Error_Configuration_Invalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The configuration of the &apos;VerifyWhen.Properties&apos; property &apos;{0}&apos; is invalid..
        /// </summary>
        public static string Error_Configuration_VerifyPropertyHasInvalidProperty_X {
            get {
                return ResourceManager.GetString("Error_Configuration_VerifyPropertyHasInvalidProperty_X", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Multiple evidence PDF documents were found..
        /// </summary>
        public static string Error_MultiplePDFEvidenceObjectsFound {
            get {
                return ResourceManager.GetString("Error_MultiplePDFEvidenceObjectsFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Signed document must be in PDF format..
        /// </summary>
        public static string Error_PDFEvidenceMustBePDF {
            get {
                return ResourceManager.GetString("Error_PDFEvidenceMustBePDF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The PDF is not a single-file document..
        /// </summary>
        public static string Error_PDFEvidenceNotSingleFileDocument {
            get {
                return ResourceManager.GetString("Error_PDFEvidenceNotSingleFileDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This state transition requires smart card authentication. Change the state via the &apos;Move into State&apos; section in the task pane..
        /// </summary>
        public static string Error_PDFEvidenceObjectInvalid {
            get {
                return ResourceManager.GetString("Error_PDFEvidenceObjectInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The evidence PDF was not found..
        /// </summary>
        public static string Error_PDFEvidenceObjectMissing {
            get {
                return ResourceManager.GetString("Error_PDFEvidenceObjectMissing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The evidence PDF does not contain a signature matching the user &apos;{0}&apos;..
        /// </summary>
        public static string Error_PDFSignatureVerificationFailed {
            get {
                return ResourceManager.GetString("Error_PDFSignatureVerificationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This state transition requires smart card authentication. Changing the property values &apos;{0}&apos; of the object is prohibited..
        /// </summary>
        public static string Error_SmartCardRequiredButPropertiesChanged_X {
            get {
                return ResourceManager.GetString("Error_SmartCardRequiredButPropertiesChanged_X", resourceCulture);
            }
        }
    }
}
