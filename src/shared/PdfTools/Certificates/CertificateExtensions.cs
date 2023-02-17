using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Extension methods for working with X509Certificates
	/// </summary>
	public static class CertificateExtensions
	{
		/// <summary>
		/// If the certificate has a Subject Alternative Name extension or Issuer Alternative Name, uses the first rfc822Name choice. If no rfc822Name choice is found in the extension, uses the Subject Name field for the Email OID. If either rfc822Name or the Email OID is found, uses the string. Otherwise, returns an empty string (returned character count is 1). pvTypePara is not used and is set to NULL.
		/// </summary>
		private const uint CERT_NAME_EMAIL_TYPE = 1;

		private const uint CERT_NAME_UPN_TYPE = 8;

		private static readonly Regex CommonNameParser = new Regex( @"CN\s*=\s*(?<cn>[\w\s]+)(,|$)");

		/// <summary>
		/// Gets the Rfc-822 name of the certificate. 
		/// 
		/// The e-mail address of the user.
		/// </summary>
		/// <param name="certificate">Certificate.</param>
		/// <returns></returns>
		public static string Rfc822Name( this X509Certificate2 certificate )
		{
			// Delegate to private helper.
			return GetRfc822Email( certificate );
		}

		/// <summary>
		/// Gets the common name from the given distinguished name.
		/// </summary>
		/// <param name="dn">The distinguished name.</param>
		/// <returns></returns>
		public static string CommonName( this X500DistinguishedName dn )
		{
			var decoded = dn.Decode( X500DistinguishedNameFlags.None );
			var m = CommonNameParser.Match( decoded );
			if( !m.Success )
				return null;
			var g = m.Groups[ "cn" ];
			return g.Value;			
		}

		/// <summary>
		/// Reads the email address from the given certificate.
		/// </summary>
		/// <param name="certificate">The certificate</param>
		/// <returns>Email address</returns>
		private static string GetRfc822Email( X509Certificate2 certificate )
		{
			// Determine the size of the requested data.			
			List<uint> typeIds = new List<uint>() { CERT_NAME_EMAIL_TYPE, CERT_NAME_UPN_TYPE };
			string emailAddress = null;

			// Read in different types.
			foreach( uint typeId in typeIds )
			{
				uint length = CertGetNameStringW( certificate.Handle, typeId, 0, IntPtr.Zero, null, 0 );

				// Read the actual name.			
				var output = new byte[ ( length - 1 ) * 2 ];
				if( output.Length == 0 )
					continue;
				if( CertGetNameStringW( certificate.Handle, typeId, 0, IntPtr.Zero, output, length ) == 1 )
					throw new Win32Exception( Marshal.GetLastWin32Error() );
				emailAddress = System.Text.Encoding.Unicode.GetString( output );
				break;
			}
			// Return the result.
			return emailAddress;
		}

		/// <summary>
		/// The CertGetNameString function obtains the subject or issuer name from a certificate CERT_CONTEXT structure and converts it to a null-terminated character string.
		/// </summary>
		/// <param name="pCertContext">A pointer to a CERT_CONTEXT certificate context that includes a subject and issuer name to be converted.</param>
		/// <param name="dwType">DWORD indicating how the name is to be found and how the output is to be formatted.</param>
		/// <param name="dwFlags">Indicates the type of processing needed. </param>
		/// <param name="pvTypePara">A pointer to either a DWORD containing the dwStrType or an object identifier (OID) specifying the name attribute. The type pointed to is determined by the value of dwType.</param>
		/// <param name="pszNameString">A pointer to an allocated buffer to receive the returned string. If pszNameString is not NULL and cchNameString is not zero, pszNameString is a null-terminated string.</param>
		/// <param name="cchNameString">Size, in characters, allocated for the returned string. The size must include the terminating NULL character.</param>
		/// <returns>Returns the number of characters converted, including the terminating zero character.</returns>
		[DllImport( "crypt32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true )]
		private static extern uint CertGetNameStringW(
			IntPtr pCertContext,
			uint dwType, uint dwFlags,
			IntPtr pvTypePara,
			byte[] pszNameString,
			uint cchNameString );
	}
}
