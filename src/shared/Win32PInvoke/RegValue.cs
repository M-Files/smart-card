using System;

namespace MFiles.Internal.Win32PInvoke
{
	public class RegValue
	{
		private byte[] m_valueBuffer;  // The actual value.
		private uint m_valueType;  // The type of the value.
		private string m_valueName;  // The name of the value.

		/// <summary>
		/// Creates new object representing a registry value.
		/// </summary>
		/// <param name="valueName"></param>
		/// <param name="valueType"></param>
		/// <param name="valueBuffer"></param>
		public RegValue( string valueName, uint valueType, byte[] valueBuffer )
		{
			m_valueBuffer = valueBuffer;
			m_valueType = valueType;
			m_valueName = valueName;
		}

		/// <summary>
		/// Returns the value.
		/// </summary>
		/// <returns></returns>
		public object GetValue()
		{
			// Determine how the value should be deserialized and then deserialize it.
			if( m_valueType == RegistryHelper.REG_DWORD )
			{
				// Sanity check.
				if( m_valueBuffer.Length != 4 )
					throw new Exception( "Invalid DWORD value '" + m_valueName + "' in registry." );

				// Return the value.
				Int32 value = BitConverter.ToInt32( m_valueBuffer, 0 );
				return value;
			}
			else if( m_valueType == RegistryHelper.REG_SZ || m_valueType == RegistryHelper.REG_MULTI_SZ )
			{
				// Parse the buffer as string.
				System.Text.UnicodeEncoding enc = new System.Text.UnicodeEncoding();
				string value = enc.GetString( m_valueBuffer );

				// Discard possible null-terminators.
				while( value.EndsWith( "\0" ) )
					value = value.Substring( 0, value.Length - 1 );

				// Return the value.
				return value;
			}
			else
			{
				throw new Exception( "The value type of registry value '" + m_valueName + "' is not supported." );
			}
		}
	}
}
