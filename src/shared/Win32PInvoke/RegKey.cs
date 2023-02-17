using System;
using System.Collections.Generic;
using Microsoft.Win32;
using HKEY = System.UIntPtr;

namespace MFiles.Internal.Win32PInvoke
{
	public class RegKey : IDisposable
	{
		public RegKey( RegistryHelper.enumRegistryAccessFlags accessFlags, HKEY hkey, string keyName )
		{
			m_accessFlags = accessFlags;
			m_hkey = hkey;
			m_keyName = keyName;
		}

		~RegKey()
		{
			Close();
		}

		/// <summary>
		/// Opens the specified subkey.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="regsam"></param>
		/// <returns></returns>
		public RegKey OpenSubKey( string key, RegistryHelper.RegSAM samDesired )
		{
			RegKey subkey = RegistryHelper.OpenKey( m_hkey, m_accessFlags, key, samDesired );
			return subkey;
		}

		public object GetValue( string valueName )
		{
			return GetValue( valueName, null );
		}

		public object GetValue( string valueName, object defaultValue )
		{
			// Read the value.
			RegValue regvalue = RegistryHelper.ReadRegValue( m_hkey, m_accessFlags, "", valueName );
			if( regvalue == null )
				return defaultValue;
			return regvalue.GetValue();
		}

		/// <summary>
		/// Sets the data of the specified value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="rvk"></param>
		public void SetValue( string valueName, object value, RegistryValueKind rvk )
		{
			// Serialize the value and write it to the registry. The serialization
			// method is dependant on the value type.
			byte[] valueBytes = null;
			if( rvk == RegistryValueKind.DWord )
			{
				// Serialize the DWORD.
				valueBytes = GetValueAsBytes( value );
			}
			else if( rvk == RegistryValueKind.String || rvk == RegistryValueKind.MultiString )
			{
				// Convert the string value to byte array.
				string valueStr = ( string )value;
				System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
				valueBytes = enc.GetBytes( valueStr );
			}
			else
			{
				throw new Exception( "Unsupported registry value type: " + rvk.ToString() );
			}

			// Store the value.
			RegistryHelper.WriteRegValue( m_hkey, m_accessFlags, "", valueName, rvk, valueBytes );
		}

		/// <summary>
		/// Gets the registry keys under this key.
		/// </summary>
		/// <returns></returns>
		public IList<string> GetSubKeyNames()
		{
			IList<string> listSubKeys = RegistryHelper.EnumSubkeys( m_hkey, m_accessFlags, "" );
			return listSubKeys;
		}

		/// <summary>
		/// Deletes all subkeys under this key. Subkeys must not have subkeys.
		/// </summary>
		/// <returns></returns>
		public void DeleteSubKeys()
		{
			IList<string> listSubKeys = RegistryHelper.EnumSubkeys( m_hkey, m_accessFlags, "" );
			foreach( string szKey in listSubKeys )
				RegistryHelper.DeleteRegKey( m_hkey, m_accessFlags, szKey );
		}

		/// <summary>
		/// Closes the registry key.
		/// </summary>
		public void Close()
		{
			if( m_hkey != HKEY.Zero )
				RegistryHelper.RegCloseKey( m_hkey );
			m_hkey = HKEY.Zero;
		}

		public virtual void Dispose()
		{
			Close();
		}

		public HKEY Handle { get { return m_hkey; } }

		public string KeyName { get { return m_keyName; } }

		/// <summary>
		/// Gets the specified value as bytes.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static byte[] GetValueAsBytes( object value )
		{
			// Convert the value to byte array.
			byte[] valueAsBytes = null;
			Int32 valueInt = Convert.ToInt32( value );
			valueAsBytes = BitConverter.GetBytes( valueInt );
			return valueAsBytes;
		}

		private RegistryHelper.enumRegistryAccessFlags m_accessFlags;
		private HKEY m_hkey;  // The registry key this object represents.
		private string m_keyName;
	}
}
