using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace MFiles.Internal.Win32PInvoke
{
    public class Crypto
    {
        // Flag definitions
        public const Int32 CRYPTUI_WIZ_NO_UI = 1;
        
        public const Int32 CRYPTUI_WIZ_DIGITAL_SIGN_SUBJECT_FILE = 1;
        
        public const Int32 CRYPTUI_WIZ_DIGITAL_SIGN_CERT = 1;
    
        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTUI_WIZ_DIGITAL_SIGN_INFO
        {
            public Int32 dwSize;
            public Int32 dwSubjectChoice;
            
            [MarshalAs(UnmanagedType.LPWStr)] 
            public string pwszFileName;
            
            public Int32 dwSigningCertChoice;
            public IntPtr pSigningCertContext;

            [MarshalAs(UnmanagedType.LPWStr)] 
            public string pwszTimestampURL;

            public Int32 dwAdditionalCertChoice;
            public IntPtr pSignExtInfo;
        };
       
        [StructLayout(LayoutKind.Sequential)]
        public struct CRYPTUI_WIZ_DIGITAL_SIGN_CONTEXT
        {
            public Int32 dwSize;
            public Int32 cbBlob;
            public IntPtr pbBlob;
        };

       
        [DllImport("Cryptui.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public extern static bool CryptUIWizDigitalSign(
            Int32 dwFlags,
            IntPtr hwndParent,
            string pwszWizardTitle,
            ref CRYPTUI_WIZ_DIGITAL_SIGN_INFO pDigitalSignInfo,
            out IntPtr ppSignContext
        );    

       
        [DllImport("Cryptui.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public extern static bool CryptUIWizFreeDigitalSignContext(
            IntPtr pSignContext
        );

        /// <summary>
        /// Digitally signs the given file. The signing is retried three times. Often the sign process cannot access the timestamp server.
        /// </summary>
        public static FileInfo SignWithRetry(X509Certificate certificate, string authenticodeTimestampURL, string targetFilename, int sleepBetweenAttemptsInSeconds )
        {
            // Try again few times because signing can fail due to many temporary problems.
            int attempt = 0;
            bool tryAgain = false;
            FileInfo targetFile = new FileInfo( targetFilename );
            do
            {
                try
                {
                    tryAgain = false;
                    Sign( certificate, authenticodeTimestampURL, targetFile );
                }
                catch( System.Exception )
                {
                    // Check if have reached the limit of attempts
                    if (attempt > 3)
                        throw;

					// Sleep.
	                Thread.Sleep( sleepBetweenAttemptsInSeconds*1000*attempt );

                    attempt++;
                    tryAgain = true;
                }

            } while (tryAgain);

            // Returns information on the signed file.
            return new FileInfo(targetFilename);

        }  // end func

        /// <summary>
        /// Digitally signs the given file.
        /// </summary>
        private static void Sign(X509Certificate certificate, string authenticodeTimestampURL, FileInfo targetFile)
        {
            // Prepare sign info.
            IntPtr signingContext = certificate.Handle;
            Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_INFO digitalSignInfo = new Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_INFO();
            digitalSignInfo.dwSize = Marshal.SizeOf(digitalSignInfo);
            digitalSignInfo.dwSubjectChoice = Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_SUBJECT_FILE;  // Signing a file.
            digitalSignInfo.pwszFileName = targetFile.FullName;
            digitalSignInfo.dwSigningCertChoice = Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_CERT;
            digitalSignInfo.pSigningCertContext = signingContext;
            if (String.IsNullOrEmpty(authenticodeTimestampURL))
                digitalSignInfo.pwszTimestampURL = null;
            else
                digitalSignInfo.pwszTimestampURL = authenticodeTimestampURL;
            digitalSignInfo.dwAdditionalCertChoice = 0;
            digitalSignInfo.pSignExtInfo = IntPtr.Zero;

            Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_CONTEXT signContext;
            IntPtr pSignContext = IntPtr.Zero;
            try
            {                
                if (!Crypto.CryptUIWizDigitalSign(Crypto.CRYPTUI_WIZ_NO_UI, IntPtr.Zero, "", ref digitalSignInfo, out pSignContext))
                    throw new ApplicationException( string.Format( "Signing the file failed: " + Marshal.GetLastWin32Error() ) );
                signContext = ( Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_CONTEXT) Marshal.PtrToStructure(pSignContext, typeof(Crypto.CRYPTUI_WIZ_DIGITAL_SIGN_CONTEXT));
            }            
            finally
            {
                if( pSignContext != IntPtr.Zero )
                    Crypto.CryptUIWizFreeDigitalSignContext( pSignContext );
            }            
        }

        /// <summary>
        /// Loads a certificate from a certificate store.
        /// </summary>
        /// <param name="storeName">The name of the certificate store.</param>
        /// <param name="certificateName">The name of the certificate.</param>
        /// <returns>The certificatel.</returns>
        public static X509Certificate2 LoadCertificateFromStore(string storeName, string certificateName)
        {
            // Open specified certificate store
            // and search for the specified certificate.
            X509Store certstore = new X509Store(storeName);
            certstore.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2 certificateFound = null;
            foreach( X509Certificate2  certificate in certstore.Certificates )
            {
                // Is this the certificate we are looking for.
                X500DistinguishedName dn = certificate.SubjectName;
                string pattern = @"CN=(?<issuedTo>[^/,]+),*";
                Match match = Regex.Match(dn.Name, pattern);
                if (match.Success)
                {
                    // Did we find the certificate?
                    string issuedTo = match.Result("${issuedTo}");
                    if (issuedTo.Equals(certificateName))
                    {
                        certificateFound = certificate;
                    }
                }
                else
                {
                    throw new SystemException( String.Format( "Unrecognized certificate - Subject: '{0}' ", dn.Name ) );
                }

            }  // end foreach

            // Certificate was not found, print all available certificates and their parsed names.
            if (certificateFound == null)
            {
                // Search for the specified certificate.
                foreach (X509Certificate2 certificate in certstore.Certificates)
                {
                    // Is this the certificate we are looking for.
                    X500DistinguishedName dn = certificate.SubjectName;
                    string pattern = @"CN=(?<issuedTo>[^/,]+),*";
                    Match match = Regex.Match(dn.Name, pattern);
                    if (match.Success)
                    {
                        // Did we find the certificate?
                        string issuedTo = match.Result("${issuedTo}");
                        Console.WriteLine("Subject: '{0}' - IssuedTo: '{1}'", dn.Name, issuedTo);
                    }
                    else
                    {
                        // Unrecognized certificate.
                        Console.WriteLine("Unrecognized certificate - Subject: '{0}' ", dn.Name);
                    }

                }  // end foreach

                throw new ArgumentException("Unknown certificate");

            }  // end if
            return certificateFound;
        }
    }
}
