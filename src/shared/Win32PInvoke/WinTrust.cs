using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// Source:
// http://www.pinvoke.net/default.aspx/wintrust.winverifytrust

namespace MFiles.Internal.Win32PInvoke
{
    using System;
    using System.Runtime.InteropServices;
    using System.IO;

    #region WinTrustData struct field enums
    enum WinTrustDataUIChoice : uint
    {
        All = 1,
        None = 2,
        NoBad = 3,
        NoGood = 4
    }

    enum WinTrustDataRevocationChecks : uint
    {
        None = 0x00000000,
        WholeChain = 0x00000001
    }

    enum WinTrustDataChoice : uint
    {
        File = 1,
        Catalog = 2,
        Blob = 3,
        Signer = 4,
        Certificate = 5
    }

    enum WinTrustDataStateAction : uint
    {
        Ignore = 0x00000000,
        Verify = 0x00000001,
        Close = 0x00000002,
        AutoCache = 0x00000003,
        AutoCacheFlush = 0x00000004
    }

    [FlagsAttribute]
    enum WinTrustDataProvFlags : uint
    {
        UseIe4TrustFlag = 0x00000001,
        NoIe4ChainFlag = 0x00000002,
        NoPolicyUsageFlag = 0x00000004,
        RevocationCheckNone = 0x00000010,
        RevocationCheckEndCert = 0x00000020,
        RevocationCheckChain = 0x00000040,
        RevocationCheckChainExcludeRoot = 0x00000080,
        SaferFlag = 0x00000100,
        HashOnlyFlag = 0x00000200,
        UseDefaultOsverCheck = 0x00000400,
        LifetimeSigningFlag = 0x00000800,
        CacheOnlyUrlRetrieval = 0x00001000      // affects CRL retrieval and AIA retrieval
    }

    enum WinTrustDataUIContext : uint
    {
        Execute = 0,
        Install = 1
    }
    #endregion

    #region WinTrust structures
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class WinTrustFileInfo
    {
        UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustFileInfo));
        IntPtr pszFilePath;                     // required, file name to be verified
        IntPtr hFile = IntPtr.Zero;             // optional, open handle to FilePath
        IntPtr pgKnownSubject = IntPtr.Zero;    // optional, subject type if it is known

        public WinTrustFileInfo(String _filePath)
        {
            pszFilePath = Marshal.StringToCoTaskMemAuto(_filePath);
        }
        ~WinTrustFileInfo()
        {
            Marshal.FreeCoTaskMem(pszFilePath);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class WinTrustData
    {
        UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustData));
        IntPtr PolicyCallbackData = IntPtr.Zero;
        IntPtr SIPClientData = IntPtr.Zero;
        // required: UI choice
        WinTrustDataUIChoice UIChoice = WinTrustDataUIChoice.None;
        // required: certificate revocation check options
        WinTrustDataRevocationChecks RevocationChecks = WinTrustDataRevocationChecks.None;
        // required: which structure is being passed in?
        WinTrustDataChoice UnionChoice = WinTrustDataChoice.File;
        // individual file
        IntPtr FileInfoPtr;
        WinTrustDataStateAction StateAction = WinTrustDataStateAction.Ignore;
        IntPtr StateData = IntPtr.Zero;
        String URLReference = null;
        WinTrustDataProvFlags ProvFlags = 0;
        WinTrustDataUIContext UIContext = WinTrustDataUIContext.Execute;

        // constructor for silent WinTrustDataChoice.File check
        public WinTrustData(String _fileName)
        {
            WinTrustFileInfo wtfiData = new WinTrustFileInfo(_fileName);
            FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
            Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);
        }
        ~WinTrustData()
        {
            Marshal.FreeCoTaskMem(FileInfoPtr);
        }
    }
    #endregion

    public enum WinVerifyTrustResult : uint
    {
        Success = 0,
        ProviderUnknown = 0x800b0001,           // The trust provider is not recognized on this system
        ActionUnknown = 0x800b0002,             // The trust provider does not support the specified action
        SubjectFormUnknown = 0x800b0003,        // The trust provider does not support the form specified for the subject
        SubjectNotTrusted = 0x800b0004          // The subject failed the specified verification action
    }

    public class WinTrust
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        // GUID of the action to perform
        private const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

        [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern WinVerifyTrustResult WinVerifyTrust(
            [In] IntPtr hwnd,
            [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
            [In] WinTrustData pWVTData
        );

        // call WinTrust.WinVerifyTrust() to check embedded file signature
        public static WinVerifyTrustResult VerifyEmbeddedSignature(string fileName)
        {
            WinTrustData wtd = new WinTrustData(fileName);
            Guid guidAction = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
            WinVerifyTrustResult result = WinVerifyTrust(INVALID_HANDLE_VALUE, guidAction, wtd);
            return result;
        }
        
        private WinTrust() { }


        /// <summary>
        /// Verifies the digital signature of this item.
        /// </summary>
        public static void VerifyDigitalSignature(string filename )
        {
            // Verify digital signature.
            string signer = "";
            try
            {
                // Verify the signature.
                signer = VerifyAuthenticodeSignature(filename);
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException("Signature verification failed. (" + filename + ")", ex);
            }
            if (signer.Equals("Motive Systems Oy") == false)
                throw new InvalidOperationException("Signature verification failed. Invalid signer. (" + filename + ")");
        }

        /// <summary>
        /// Verifies the digital signature (Authenticode signature) of a file (typically an EXE, DLL, or MSI file).
        /// </summary>
        private static string VerifyAuthenticodeSignature(string filename)
        {
            // File was not found for verification.
            FileInfo verifiedFile = new FileInfo(filename);
            if (verifiedFile.Exists == false)
                throw new FileNotFoundException("File not found: " + verifiedFile.FullName);

            // The signature is invalid.
            // string verifiedFile = "\\\\file01\\ReleaseArea\\M-Files\\PublishedOfficialVersions\\7.0.2589.6\\M-Files_x64_eng_7_0_2589_6.msi";            
            WinVerifyTrustResult result = WinTrust.VerifyEmbeddedSignature(verifiedFile.FullName);
            if (result != WinVerifyTrustResult.Success)
                throw new SystemException("Verification failed: " + result.ToString());

            /*// Prepare information on the file to verify.
            // string verifiedFile = m_physicalPath.FullName;
            // string verifiedFile = m_physicalPath.FullName;
            // string verifiedFile = "\\\\file01\\ReleaseArea\\M-Files\\PublishedOfficialVersions\\7.0.2589.6\\M-Files_x64_eng_7_0_2589_6.msi";
            string verifiedFile = "e:\\temp\\M-Files_x86_eng_client_7_0_2591_14.msi";

            // Read the certificate information from the file.
            X509Certificate signingCertificateFromFile = X509Certificate.CreateFromSignedFile(verifiedFile);
            X509Certificate2 signingCertificate = new X509Certificate2(signingCertificateFromFile);
            string signer = signingCertificate.GetNameInfo( X509NameType.SimpleName, false );
            return signer;*/

            // TODO: For now we assume Motive Systems.
            return "Motive Systems Oy";
        }
    }
}
