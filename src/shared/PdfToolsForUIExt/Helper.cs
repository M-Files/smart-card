using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MFiles.Internal.Win32PInvoke;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// A collection of helper methods.
	/// </summary>
	public static class Helper
	{
		/// <summary>
		/// Converts ObjVer to a string.
		/// </summary>
		/// <param name="objver">ObjVer to convert.</param>
		/// <returns>Object version as a string.</returns>
		public static string ObjVerToString( dynamic objver )
		{
			// Convert and return
			string asString = string.Format( "({0}-{1}-{2})", objver.Type, objver.ID, objver.Version );
			return asString;
		}

		/// <summary>
		/// Gets Reason for the signature from the given object version.
		/// </summary>
		/// <param name="objectVersion">Object version to convert.</param>
		/// <returns>Object version as a string.</returns>
		public static string GetReasonForSignatureFromObjectVersion( dynamic objectVersion )
		{
			// Convert and return
			string objectGuid = objectVersion.ObjectGUID;
			string versionGuid = objectVersion.VersionGUID;
			dynamic objver = objectVersion.ObjVer;
			string asString = String.Format( "({0}-{1}-{2}-{3}-{4})", objectGuid, versionGuid, objver.Type, objver.ID, objver.Version );
			return asString;
		}

		/// <summary>
		/// Returns the objVer of the latest checked in version of the given document.
		/// </summary>
		/// <param name="document">ObjectVersion of the document.</param>
		/// <returns></returns>
		internal static dynamic GetLatestCheckedInObjVer( dynamic document )
		{
			// Clone the ObjVer.
			dynamic latestCheckedInVersionObjVer = document.ObjVer.Clone();

			// Change the version.
			latestCheckedInVersionObjVer.Version = document.LatestCheckedInVersion;

			// Return the changed ObjVer.
			return latestCheckedInVersionObjVer;
		}		

		/// <summary>
		/// Compresses and bas64 encodes the given string.
		/// </summary>
		/// <param name="input">Input string.</param>
		/// <returns>Compressed and base64 encoded string.</returns>
		internal static string DeflateBase64( string input )
		{
			// Compress and base64 encode.
			string output;
			using( var outputStream = new MemoryStream() )
			{
				// Compress and base64 encode.
				using( var inputStream = input.ToStream() )
				using( var deflate = new DeflateStream( outputStream, CompressionMode.Compress ) )
				{
					inputStream.CopyTo( deflate );
					deflate.Flush();
					deflate.Close();
				}
				output = Convert.ToBase64String( outputStream.ToArray() );
			}
			return output;
		}

		/// <summary>
		/// Inflates the given base64 encoded string.
		/// </summary>
		/// <param name="input">Input string</param>
		/// <returns>Inflated string.</returns>
		internal static string InflateBase64( string input )
		{
			// Compress and base64 encode.
			string output;
			using( var inputStream = input.Base64Decode() )
			{
				// Compress and base64 encode.
				using( var deflate = new DeflateStream( inputStream, CompressionMode.Decompress ) )
				using( var sr = new StreamReader( deflate ) )
				{
					output = sr.ReadToEnd();
				}
			}
			return output;
		}

		/// <summary>
		/// Hides the specified object.
		/// </summary>
		/// <param name="vault">Vault access</param>
		/// <param name="objectToHide">Object version of an object to be hidden.</param>
		internal static void HideObject( dynamic vault, dynamic objectToHide )
		{
			// We cannot create new objects in this context.
			// => We hide the object by fisrt fetching original ACL and then by modifying that.

			// Get current ACL of the object.
			dynamic currentPermissions = vault.ObjectOperations.GetObjectPermissions( objectToHide );
			Application.DoEvents();

			// Construct new ACL.
			dynamic updatedACL = currentPermissions.AccessControlList.Clone();
			updatedACL.CustomComponent.ResetNamedACLLink();
			updatedACL.CustomComponent.ResetCurrentUserBinding();
			RemoveAllEntriesFromAclComponent( updatedACL.CustomComponent );
			Application.DoEvents();

			// Update the permission of the object.
			vault.ObjectOperations.ChangePermissionsToACL( objectToHide, updatedACL, true );
			Application.DoEvents();
		}
		
		/// <summary>
		/// Removes all access control list entries from the given ACL component.
		/// </summary>
		/// <param name="component">Component from which the entries are removed.</param>
		private static void RemoveAllEntriesFromAclComponent( dynamic component )
		{
			// Remove all entries.
			dynamic entryContainer = component.AccessControlEntries;
			foreach( var entryKey in entryContainer.GetKeys() )
			{
				// Remove the entry corresponding to the current key.
				entryContainer.Remove( entryKey );
			}
		}

		/// <summary>
		/// Converts the given string to a stream.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static Stream ToStream( this string input )
		{
			// Write the string to the stream and seek to the beginning.
			var stream = new MemoryStream();
			var writer = new StreamWriter( stream, Encoding.UTF8 );
			writer.Write(  input );
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		/// <summary>
		///  Decodes the base64 encoded string and converts it to a stream.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private static Stream Base64Decode( this string input )
		{
			// Write the string to the stream and seek to the beginning.
			var decoded = Convert.FromBase64String( input );
			var stream = new MemoryStream();
			stream.Write( decoded, 0, decoded.Length );	
			stream.Position = 0;
			return stream;
		}
	}
}
