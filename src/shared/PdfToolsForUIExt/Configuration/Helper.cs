using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MFiles.PdfTools.UIExt.Configuration
{
	/// <summary>
	/// Helper class for resolving placeholder values.
	/// </summary>
	internal static class Helper
	{
		/// <summary>
		/// Regular expressoin for replacing the placeholder values.
		/// </summary>		
		private readonly static Regex PlacholderReplacer = new Regex( @"(?<placeholder>[$][{](?<placeholderName>\w+)[}])" );

		/// <summary>
		/// Replaces specified placeholders in the given input.
		/// </summary>
		/// <param name="input">Input string.</param>
		/// <param name="placeholders">Replacements for the placeholders</param>
		/// <returns>Output value.</returns>
		internal static string ReplacePlaceholders( string input, IDictionary<string, string> placeholders )
		{
			// Make the replacements.
			var output = PlacholderReplacer.Replace( input, m =>
			{

				// Should be always success when called.
				if( !m.Success )
					throw new InvalidOperationException();

				// Make the replacement.
				var placeholderName = m.Groups[ "placeholderName" ].Value;
				string replacement;
				if( placeholders.TryGetValue( placeholderName, out replacement ) )
					return replacement;

				// Faile.
				throw new ArgumentException( string.Format( InternalResources.Error_UnrecognizedPlaceholder, placeholderName ) );
			} );
			return output;
		}
	}
}
