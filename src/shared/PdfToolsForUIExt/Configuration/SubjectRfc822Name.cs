using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using MFiles.PdfTools.Certificates;
using Newtonsoft.Json;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Represents the Rfc-822 or email address of the user the certificate is issued to.
	/// </summary>
	[DataContract( Namespace = "" )]
	public class SubjectRfc822Name
	{
		/// <summary>
		/// The value of the Rfc822Name.
		/// </summary>
		[DataMember]
		public string Value { get; set; }

		/// <summary>
		/// True if the value represents regular expression.
		/// </summary>
		[DataMember]
		public bool IsRegularExpression { get; set; }

		/// <summary>
		/// Type of Subject Alternative Name. https://msdn.microsoft.com/en-us/library/windows/desktop/aa376086(v=vs.85).aspx
		/// </summary>
		//[DataMember]
		//public uint Type { get; set; }

		/// <summary>
		/// Resolves the value and the associated placeholders.
		/// </summary>
		/// <param name="placeholders">Placeholders.</param>
		/// <returns>Value with placeholders replaced.</returns>
		public string ResolveValue( IDictionary< string, string > placeholders )
		{
			// Replace the placeholders
			var updatedValue = this.Value;
			if( placeholders != null )
				updatedValue = Helper.ReplacePlaceholders( this.Value, placeholders );
			return updatedValue;
		}

		/// <summary>
		/// Gets the Rfc-822 name as a search condition.
		/// </summary>
		/// <param name="placeholders">Replacement values for possible placeholders in the condition string.</param>
		/// <returns>Condition</returns>
		internal Rfc822NameCondition AsCondition( IDictionary< string, string > placeholders )
		{
			// Replace the placeholders and process possible errors.
			var updatedValue = this.Value;
			try
			{
				// Replace the placeholders
				if( placeholders != null )
					updatedValue = Helper.ReplacePlaceholders( this.Value, placeholders );
			}
			catch( Exception ex )
			{
				// Unable to resolve the certificate.
				throw new CertificateNotFoundException( "Not all placholders of Rfc-822 name could be resolved.", ex );

			}  // end if
			
			// Return the condition.
			if( this.IsRegularExpression )
				return new Rfc822NameCondition( new Regex( updatedValue ), updatedValue );
			else
				return new Rfc822NameCondition( updatedValue );
		}
	}
}
