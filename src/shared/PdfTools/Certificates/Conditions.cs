using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Declares extension methods for filtering certificate collection.
	/// </summary>
	public static class Conditions
	{

		/// <summary>
		/// Applies Thumbprint condition to certificate collection.
		/// </summary>
		/// <param name="input">Filtered collection</param>
		/// <param name="thumbprint">Thumbprint</param>
		/// <returns>Collection.</returns>
		public static X509Certificate2Collection Thumbprint( this X509Certificate2Collection input, string thumbprint )
		{
			// Filter the conditions by subject.
			var condition = new ThumbprintCondition( thumbprint );
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Applies Subject condition to certificate collection.
		/// </summary>
		/// <param name="input">Filtered collection</param>
		/// <param name="subject">Subject</param>
		/// <returns>Collection.</returns>
		public static X509Certificate2Collection Subject( this X509Certificate2Collection input, string subject )
		{
			// Filter the conditions by subject.
			var condition = new SubjectCondition( subject );
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Applies IssuedBy condition to certificate collection.
		/// </summary>
		/// <param name="input">Collection</param>
		/// <param name="issuedBy">CA that issued the certificate</param>
		/// <returns>Filtered collection</returns>
		public static X509Certificate2Collection IssuedBy( this X509Certificate2Collection input, string issuedBy )
		{
			// Filter the conditions by certificate authority.
			var condition = new IssuedByCondition( issuedBy );
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Applies KeyUsage condition to certificate collection.
		/// </summary>
		/// <param name="input">Collection</param>
		/// <param name="keyUsage">The intended key usage of the certificate.</param>
		/// <returns>Filtered collection</returns>
		public static X509Certificate2Collection KeyUsage( this X509Certificate2Collection input, X509KeyUsageFlags keyUsage )
		{
			// Filter the conditions by key usage.
			var condition = new KeyUsageCondition( keyUsage );
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Applies TimeValid condition to certificate collection.
		/// </summary>
		/// <param name="input">Collection</param>		
		/// <returns>Filtered collection</returns>
		public static X509Certificate2Collection TimeValid( this X509Certificate2Collection input )
		{
			// Filter the conditions.
			var condition = new TimeValidCondition();
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Applies Rfc822Name condition to certificate collection.
		/// </summary>
		/// <param name="input">Collection</param>		
		/// <param name="emailAddress">The email address</param>
		/// <returns>Filtered collection</returns>
		public static X509Certificate2Collection Rfc822Name( this X509Certificate2Collection input, string emailAddress )
		{
			// Filter the conditions.
			var condition = new Rfc822NameCondition( emailAddress );
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Applies Rfc822Name condition to certificate collection.
		/// </summary>
		/// <param name="input">Collection</param>
		/// <param name="emailAddressMatcher">Regular expression for identifying valid email addresses</param>
		/// <returns>Filtered collection</returns>
		public static X509Certificate2Collection Rfc822Name( this X509Certificate2Collection input, Regex emailAddressMatcher, string expressionAsString )
		{
			// Filter the conditions.
			var condition = new Rfc822NameCondition( emailAddressMatcher, expressionAsString );
			return ProcessCondition( input, condition );
		}


		/// <summary>
		/// Returns the one and only certificate in the given collection.
		/// </summary>
		/// <param name="input">Collection</param>	
		/// <param name="condition">Returns certificates filtered using the specified condition.</param>
		/// <returns>Filtered collection.</returns>
		public static X509Certificate2Collection Condition( this X509Certificate2Collection input, ICertificateCondition condition )
		{
			return ProcessCondition( input, condition );
		}

		/// <summary>
		/// Returns the one and only certificate in the given collection.
		/// </summary>
		/// <param name="input">Collection</param>		
		/// <returns>The certificate.</returns>
		public static X509Certificate2 Single( this X509Certificate2Collection input )
		{
			// Ensures that we have one and only one certificate.
			if( input.Count == 0 )
				throw new CertificateNotFoundException( InternalResources.Error_Certificates_NoCertificates );
			else if( input.Count > 1 )
				throw new CertificateNotFoundException( string.Format( InternalResources.Error_Certificates_TooManyCertificates, input.Count ) );

			// Return the one and only certificate.
			return input[ 0 ];
		}

		/// <summary>
		/// Processes the specified condition.
		/// </summary>
		/// <param name="input">Collection.</param>
		/// <param name="condition">Condition for filtering the collection.</param>
		/// <returns>Collection filtered using the given condition.</returns>
		internal static X509Certificate2Collection ProcessCondition( X509Certificate2Collection input, ICertificateCondition condition )
		{
			// Process the given condition.
			var result = input.Find( condition.FindType, condition.FindValue, condition.Valid );
			if( result.Count == 0 )
				condition.NoMoreCertificates();

			// Additionally evaluate each certificate separately if needed by the condition.
			if( condition.MatchingRequired )
			{
				// Iterate over the results.
				for( int i = 0; i < result.Count; )
				{
					// Check if the current certificate matches the condition.
					var certificate = result[ i ];
					if( condition.Match( certificate ) )
						i++;
					else
						result.RemoveAt( i );

				}  // end if

			}  // end if
			if( result.Count == 0 )
				condition.NoMoreCertificates();

			// Return the results.
			return result;
		}
	}
}
