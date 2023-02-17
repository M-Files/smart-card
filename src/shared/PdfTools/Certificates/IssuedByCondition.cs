using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Condition for searching certificates issued by specific certificate authority.
	/// </summary>
	public class IssuedByCondition : CertificateConditionBase
	{
		/// <summary>
		///  Parser for the name-value pair.
		/// </summary>
		private static readonly Regex NameValueParser = new Regex( @"^(?<name>.+)\s*=\s*(?<value>.+)$" );

		/// <summary>
		/// Initializes new issued by condition.
		/// </summary>
		/// <param name="issuedBy">The certificate authority that has issued the certificate</param>
		public IssuedByCondition( string issuedBy ) : base( X509FindType.FindByIssuerDistinguishedName )
		{
			// Normalize and set the condition.
			var normalized = NormalizeIssuedBy( issuedBy );
			this.FindValue = normalized;
		}

		public override void NoMoreCertificates()
		{
			throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoCertificatesIssuedBy );
		}

		/// <summary>
		/// Normalize the IssuedBy string.
		/// - 
		/// </summary>
		/// <returns></returns>
		private static string NormalizeIssuedBy( string issuedBy )
		{
			// Parse into name-value pairs and then normalize.
			var matches = issuedBy
								.Split( ',' )
								.Select( s => s.Trim() )
								.Where( s => !string.IsNullOrEmpty( s ) )  // Only valid name-values.
								.Select( s => NameValueParser.Match( s ) )  // Parse
								.Where( m => m.Success ).ToList();  // Accept only matching items.
			var normalized = matches.Select( m => new { Name = m.Groups[ "name" ].Value, Value = m.Groups[ "value" ].Value } )  // Get the name and the value from the match.
									.Aggregate( "", ( current, next ) => string.IsNullOrEmpty( current ) ?
																			AsNameValuePair( next.Name, next.Value ) : // First name-value pair
																			string.Format( "{0}, {1}", current, AsNameValuePair( next.Name, next.Value ) ) );  // Following name-value pairs.

			// Basic verifcation. Count the separators. They should match.
			if( issuedBy.Count( c => c == ',' ) != normalized.Count( c => c == ',' ) )
				throw new ArgumentException( string.Format( InternalResources.Error_Certificates_InvalidIssuedBy, issuedBy ), "issuedBy" );
			return normalized;
		}

		/// <summary>
		/// Returns the name-value pair as a single string separated by '='
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="value">Value</param>
		/// <returns></returns>
		private static string AsNameValuePair( string name, string value )
		{
			var pair = string.Format( "{0}={1}", name, value );
			return pair;
		}
	}
}
