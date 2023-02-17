using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// VAF doesn't like static variables so we define the NameValueParser here.
	///
	/// See Tracker issue: #125506.
	/// </summary>
	internal static class CertificateHelper
	{
		/// <summary>
		/// Regular expression for parsing the name-value pair of a 
		/// </summary>
		internal static readonly Regex NameValueParser = new Regex( @"^(?<name>\w+)\s*=\s*(?<value>.+)$" );	
	}

	/// <summary>
	/// Describes the certificate that is loaded.
	/// </summary>
	[DataContract( Namespace = "" )]
	public class Certificate
	{
		/// <summary>
		/// Can this certificate be used in multiple signatures.
		/// </summary>
		[DataMember]
		public bool AllowDuplicates { get; set; }

		/// <summary>
		/// The certificate authority that has issued the certificate.
		/// </summary>
		[DataMember]
		public string IssuedBy { get; set; }

		/// <summary>
		/// Key usage.
		/// </summary>
		[DataMember]
		public X509KeyUsageFlags KeyUsage { get; set; }

		/// <summary>
		/// Rfc-822 name for identifying the corret certificate.
		/// </summary>
		[DataMember]
		public SubjectRfc822Name SubjectRfc822Name { get; set; }

		/// <summary>
		/// Set to true to remove the certificate used in signing from the user's personal key store after usage.
		/// </summary>
		/// <remarks>This can be used to force the user to remove the SmartCard from the reader and re-insert it before signing again.</remarks>
		[DataMember]
		public bool RemoveCertificateFromKeyStoreAfterUsage { get; set; }		

		/// <summary>
		/// Gets the common name of the issuer.
		/// </summary>
		/// <returns>Common name of the certificate.</returns>
		public string GetCommonNameOfIssuer()
		{
			// IssuedBy specified?
			if( string.IsNullOrEmpty( this.IssuedBy ) )
				return null;

			// Get the common name.
			var commonName = this.IssuedBy
								.Split( ',' )
								.Select( s => s.Trim() )
								.Where( s => !string.IsNullOrEmpty( s ) ) // Only valid name-values.
								.Select( s => CertificateHelper.NameValueParser.Match( s ) ) // Parse
								.Where( m => m.Success )
								.Single( m => m.Groups[ "name" ].Value == "CN" )  // Common name part.
								.Groups[ "value" ].Value;  // Value of the commont name.
			return commonName;
		}
	}
}
