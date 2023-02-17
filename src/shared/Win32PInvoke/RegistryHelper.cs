using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using HANDLE = System.IntPtr;
using HKEY = System.UIntPtr;


namespace MFiles.Internal.Win32PInvoke
{
	public class RegistryHelper
	{
		//! Maximum length for buffers used in registry operations. See "Registry Element Size Limits" from MSDN and M-Files issue 7068.
		private const uint MAX_BUFFER_LENGTH = 512 * 255 + 16383;

		public const uint REG_NONE = 0;
		public const uint REG_SZ = 1;
		public const uint REG_EXPAND_SZ = 2;
		public const uint REG_BINARY = 3;
		public const uint REG_DWORD = 4;
		public const uint REG_DWORD_LITTLE_ENDIAN = 4;
		public const uint REG_DWORD_BIG_ENDIAN = 5;
		public const uint REG_LINK = 6;
		public const uint REG_MULTI_SZ = 7;

		public const string KERNEL32_DLL = @"kernel32.dll";
		public const string USER32_DLL = @"user32.dll";
		public const string ADVAPI32_DLL = @"Advapi32.dll";

		// Error codes
		public const int ERROR_SUCCESS = 0;
		public const int ERROR_FILE_NOT_FOUND = 2;
		public const int ERROR_MORE_DATA = 234;
		public const int ERROR_NO_MORE_ITEMS = 259;

		public static HKEY HKEY_LOCAL_MACHINE = ( HKEY )0x80000002; // the others are in WINREG.H

		[Flags]
		public enum RegOption
		{
			NonVolatile = 0x0,
			Volatile = 0x1,
			CreateLink = 0x2,
			BackupRestore = 0x4,
			OpenLink = 0x8
		}

		[Flags]
		public enum RegSAM
		{
			QueryValue = 0x0001,
			SetValue = 0x0002,
			CreateSubKey = 0x0004,
			EnumerateSubKeys = 0x0008,
			Notify = 0x0010,
			CreateLink = 0x0020,
			WOW64_32Key = 0x0200,
			WOW64_64Key = 0x0100,
			WOW64_Res = 0x0300,
			Read = 0x00020019,
			Write = 0x00020006,
			Execute = 0x00020019,
			AllAccess = 0x000f003f
		}

		[Flags]
		public enum enumRegistryAccessFlags
		{
			eregaccessDefault = 0,  //!< Use default registry (e.g. 32 bit registry on 32-bit binaries, 64-bit registry on x64 binaries).
			eregaccessWoW64_32bit = 1,  //!< Force the use of Win32 registry (used by WoW64 applications) if it is available.
			eregaccessSystemNative = 2,  //!< Force the use of system native registry (e.g. x64 registry if it is available).
		};

		public enum RegResult
		{
			CreatedNewKey = 0x00000001,
			OpenedExistingKey = 0x00000002
		}

		[StructLayout( LayoutKind.Sequential )]
		public struct SECURITY_ATTRIBUTES
		{
			public int nLength;
			public IntPtr lpSecurityDescriptor;
			public int bInheritHandle;
		}

		/// <summary>
		/// Creates a registry key.
		/// </summary>
		/// <param name="hkey"></param>
		/// <param name="dwFlags"></param>
		/// <param name="key"></param>
		/// <param name="samDesired"></param>
		/// <returns></returns>
		public static RegKey CreateKey(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			enumRegistryAccessFlags accessFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			string key,  //!< Key name (e.g. SOFTWARE\Microsoft\Windows).
			RegSAM samDesired // Security access mask (e.g. KEY_READ or KEY_WRITE).
		)
		{
			HKEY hkeyOpened = HKEY.Zero;
			try
			{
				// Move registry access flags to SAM.
				UpdateSecurityAccessMask( accessFlags, ref samDesired );

				// Try to open the key.
				int iResult = RegOpenKeyEx( hkey, key, 0, samDesired, out hkeyOpened );
				if( iResult != ERROR_SUCCESS )
				{
					// The method fails.

					// Check the reason. Only acceptable reason is ERROR_FILE_NOT_FOUND.
					if( iResult == ERROR_FILE_NOT_FOUND )
					{
						// The key doesn't exists so try to create it.
						RegResult result;
						SECURITY_ATTRIBUTES sa = GetEmptySecurityAttributes();
						int iResult2 = RegCreateKeyEx( hkey, key, 0,
												null, RegOption.NonVolatile,
												samDesired, ref sa, out hkeyOpened, out result );
						if( iResult2 != ERROR_SUCCESS )
							throw new Win32Exception( iResult2 );
					}
					else
					{
						// Some other error occurred.
						throw new Win32Exception( iResult );
					}
				}

				// Return the key.
				RegKey regkey = new RegKey( accessFlags, hkeyOpened, key ); hkeyOpened = HKEY.Zero;
				return regkey;

			}
			finally
			{
				// Close key.
				if( hkeyOpened != HKEY.Zero )
					RegCloseKey( hkeyOpened );

			}
		}

		/// <summary>
		/// Tries to open registry key. If the key cannot be found, pbExists receives false.
		/// </summary>
		/// <param name="hkey"></param>
		/// <param name="accessFlags"></param>
		/// <param name="key"></param>
		/// <param name="samDesired"></param>
		/// <param name="?"></param>
		public static RegKey OpenKey(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			enumRegistryAccessFlags accessFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			string key,  //!< Key name (e.g. SOFTWARE\Microsoft\Windows).
			RegSAM samDesired // Security access mask (e.g. KEY_READ or KEY_WRITE).
		)
		{
			HKEY hkeyOpened = HKEY.Zero;
			try
			{
				// Move registry access flags to SAM.
				UpdateSecurityAccessMask( accessFlags, ref samDesired );

				// Try to open the key.
				int iResult = RegOpenKeyEx( hkey, key, 0, samDesired, out hkeyOpened );
				if( iResult != ERROR_SUCCESS )
				{
					// Opening the key failed.

					// Check the reason. Only acceptable reason is ERROR_FILE_NOT_FOUND.
					if( iResult == ERROR_FILE_NOT_FOUND )
					{
						// Not found.
						return null;
					}
					else
					{
						// Some other error occured.
						throw new Win32Exception( Marshal.GetLastWin32Error() );
					}
				}
				else
				{
					// Opening the key succeeded.

					// Set out params.
					RegKey regkey = new RegKey( accessFlags, hkeyOpened, key ); hkeyOpened = HKEY.Zero;  // regkey owns the handle now.
					return regkey;
				}
			}
			finally
			{
				// Close key.
				if( hkeyOpened != HKEY.Zero )
					RegCloseKey( hkeyOpened );
			}
		}

		/// <summary>
		/// Reads RegValue from registry. If the key or value is missing, tells it.
		/// </summary>
		/// <param name="HKEY"></param>
		/// <returns></returns>
		public static RegValue ReadRegValue(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			enumRegistryAccessFlags accessFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			string key,  //!< Key name relative to hkey. If NULL, reads from hkey. (e.g. SOFTWARE\Microsoft\Windows).
			string valueName  //!< The name of the value to fetch. If NULL, reads the default value.
		)
		{
			RegKey regkey = null;
			try
			{
				// Try to open the key that contains the value.
				regkey = OpenKey( hkey, accessFlags, key, RegSAM.Read );
				if( regkey != null )
				{
					// Try to open the specified value. Set the lpData member to NULL in
					// order to get the amount of total bytes we have to allocate.
					// This lets us determine the best way to allocate a buffer for
					// the value's data.
					uint dwValueType = 0;
					uint dwDataSize = 0;
					int iResult = RegQueryValueEx( regkey.Handle, valueName, 0,
													out dwValueType, null, ref dwDataSize );
					if( iResult != ERROR_SUCCESS )
					{
						// The method fails. Check reason. Only acceptable reason
						// is that the value is missing.
						if( iResult != ERROR_FILE_NOT_FOUND )
						{
							// Handle other errors.
							throw new Win32Exception( Marshal.GetLastWin32Error() );
						}

						// The value was not found.
						return null;
					}
					else
					{
						// Get the data.
						byte[] keyBuffer = new byte[ dwDataSize ];
						iResult = RegQueryValueEx( regkey.Handle, valueName, 0,
														out dwValueType, keyBuffer, ref dwDataSize );
						if( iResult != ERROR_SUCCESS )
							throw new Win32Exception( Marshal.GetLastWin32Error() );

						// Return the data.
						RegValue regvalue = new RegValue( valueName, dwValueType, keyBuffer );
						return regvalue;

					}  // end if - else (RegQueryValueEx)

				}  // end if (key exists)

				// The key was not found.
				return null;
			}
			finally
			{

				// Release reserved resources.
				if( regkey != null )
					regkey.Close();
				regkey = null;
			}
		}

		/// <summary>
		/// Writes the specified value to the registry.
		/// </summary>
		/// <param name="hkey"></param>
		/// <param name="keyName"></param>
		/// <param name="valueName"></param>
		/// <param name="rvk"></param>
		/// <param name="value"></param>
		public static void WriteRegValue(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			enumRegistryAccessFlags accessFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			string keyName,   //!< The name of the key relative to HKEY.
			string valueName,  //!< The name of the value
			RegistryValueKind rvk,  //!< The type of the value we are writing.
			byte[] value  //!< The actual value as a byte array.
		)
		{
			RegKey regkey = null;
			try
			{
				// Try to open the key that contains the value.
				regkey = OpenKey( hkey, accessFlags, keyName, RegSAM.Write );
				if( regkey != null )
				{
					// Try to write the specified value.
					uint dwValueType = GetValueTypeAsInt( rvk );
					int iResult = RegSetValueEx( regkey.Handle, valueName, 0,
												   dwValueType, value, value.Length );
					if( iResult != ERROR_SUCCESS )
					{
						throw new Win32Exception( Marshal.GetLastWin32Error() );
					}

				}  // end if (key exists)
			}
			finally
			{

				// Release reserved resources.
				if( regkey != null )
					regkey.Close();
				regkey = null;
			}
		}

		/// <summary>
		/// Enumerates all subkeys. If key is missing, return empty vector.
		/// </summary>
		/// <param name="HKEY"></param>
		/// <returns></returns>
		public static IList<string> EnumSubkeys(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			enumRegistryAccessFlags accessFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			string szKey  //!< Key name relative to hkey (e.g. SOFTWARE\Microsoft\Windows). If NULL, enumerates the subkeys of hkey.
		)
		{
			RegKey regkey = null;
			List<string> listSubKeys = new List<string>();
			try
			{
				// Try to open the key.
				regkey = OpenKey( hkey, accessFlags, szKey, RegSAM.Read );

				// Check if exists.
				if( regkey != null )
				{
					// Variables.
					uint uiIndex = 0;
					bool bNoMoreItems = false;

					// Loop until all keys are retrieved from registry.
					while( true )
					{
						// Get the next key name. dwIndex is incremented in EnumRegKeyByIndex2.
						string subKeyName = EnumSubkeyByIndex( hkey, ref uiIndex, out bNoMoreItems );

						// If there are no more items, break out.
						if( bNoMoreItems )
							break;

						// Add to list.
						listSubKeys.Add( subKeyName );

					} // end while

				}  // end if
			}
			finally
			{
			}

			// Return the subkeys.
			return listSubKeys;
		}

		/*!
		Enumerates a subkey of the given key specified by index.
		*/
		public static string EnumSubkeyByIndex(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			ref uint dwIndex,  //!< Specifies subkey index to enumerata. This is icremented here.
			out bool bNoMoreItemsOut  //!< Specifies the status of ptszName and the enumeration. If this is true the caller shouldn't call EnumKeyByIndex anymore and cannot use ptszName.
		)
		{
			// Set initially the no more items value to false.
			bNoMoreItemsOut = false;

			// Loop until the name buffer size is sufficient.
			int iRetVal = ERROR_SUCCESS;
			bool bLoop = true;
			uint uiNameBufferLenInTchars = 512;
			StringBuilder nameBuffer = null;
			string subkeyName = "";
			while( bLoop )
			{
				// Reserve memory for name buffer.
				nameBuffer = new StringBuilder( ( int )uiNameBufferLenInTchars );

				// Enumerate the key.
				uint uiNameLen = uiNameBufferLenInTchars;
				iRetVal = RegEnumKeyEx(
						hkey,  // Handle to key to enumerate (opened).
						dwIndex,  // Subkey index.
						nameBuffer,  // Subkey name buffer.
						ref uiNameLen,  // The size of subkey name buffer.
						IntPtr.Zero,  // Reserved (must be null).
						IntPtr.Zero,  // Class string buffer (can be null).
						IntPtr.Zero,  // Size of class string buffer (must be null if previous parameter is NULL).
						IntPtr.Zero  // Last write time (can be NULL if class string buffer is NULL).
				);

				// Check if the name buffer was too small.
				if( iRetVal == ERROR_MORE_DATA )
				{
					// The name buffer was too small, so free old buffer and
					// reserve more memory.
					uiNameBufferLenInTchars = uiNameBufferLenInTchars * 2;
					bLoop = true;

					// Check if we are about to overflow the name buffer.
					if( uiNameBufferLenInTchars > MAX_BUFFER_LENGTH )
					{
						// It is not possible for the name to be this long.
						bLoop = false;
						iRetVal = ERROR_NO_MORE_ITEMS;
					}
				}
				else
				{
					// Stop the loop.
					bLoop = false;
				}

			}  // end while

			// Check result.
			if( iRetVal == ERROR_NO_MORE_ITEMS )
			{
				// No more items to enumerate.
				bNoMoreItemsOut = true;
			}
			else if( iRetVal == ERROR_SUCCESS )
			{
				// Item enumerated successfully. There is still more items to enumerate.
				dwIndex++;
				subkeyName = nameBuffer.ToString();
			}
			else
			{
				// An error occured.
				throw new Win32Exception( Marshal.GetLastWin32Error() );
			}

			// Return the subkey name.
			return subkeyName;
		}

		/// <summary>
		/// Deletes the registry key. Deals with missing implementation on pre-XP SP2 systems. The key must not have subkeys.
		/// </summary>
		/// <param name="hkey"></param>
		/// <param name="keyName"></param>
		/// <param name="valueName"></param>
		/// <param name="rvk"></param>
		/// <param name="value"></param>
		public static int DeleteRegKeyEx_Wrapper(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			string lpSubKey,   //!< The name of the key relative to HKEY.
			RegSAM samDesired,  //!< REGSAM samDesired.
			uint Reserved  //!< DWORD reserved
		)
		{
			HANDLE hAdvapi32 = HANDLE.Zero;
			int iResult = -1;
			try
			{
				// RegDeleteKeyEx available?
				// It is only available in XP x64 and later.
				hAdvapi32 = LoadLibraryWin32( "Advapi32.DLL" );
				if( hAdvapi32 == HANDLE.Zero )
					throw new Win32Exception( Marshal.GetLastWin32Error() );

				// Check if the RegDeleteKeyEx method is available.
				UIntPtr uiPtrRegDeleteKeyEx = GetProcAddress( hAdvapi32, "RegDeleteKeyExW" );
				if( uiPtrRegDeleteKeyEx != UIntPtr.Zero )
				{
					// Available.
					iResult = RegDeleteKeyEx( hkey, lpSubKey, samDesired, Reserved );
				}
				else
				{
					// Not available. Call RegDeleteKey instead.
					iResult = RegDeleteKey( hkey, lpSubKey );
				}
			}
			finally
			{
				if( hAdvapi32 != HANDLE.Zero )
					FreeLibraryWin32( hAdvapi32 );
			}

			return iResult;
		}


		/// <summary>
		/// Deletes the registry key. The key must not have subkeys.
		/// </summary>
		/// <param name="hkey"></param>
		/// <param name="keyName"></param>
		/// <param name="valueName"></param>
		/// <param name="rvk"></param>
		/// <param name="value"></param>
		public static void DeleteRegKey(
			HKEY hkey,  //!< The predefined or opened registry key (e.g. HKEY_LOCAL_MACHINE).
			enumRegistryAccessFlags accessFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			string keyName   //!< The name of the key relative to HKEY.
		)
		{
			// Move registry access flags to SAM.
			RegSAM samDesired = 0;
			UpdateSecurityAccessMask( accessFlags, ref samDesired );

			// Delegate for Win32 method.
			int iResult = DeleteRegKeyEx_Wrapper( hkey, keyName, samDesired, 0 );
			if( iResult != ERROR_SUCCESS )
			{
				// An error occured.
				throw new Win32Exception( Marshal.GetLastWin32Error() );
			}
		}

		#region Win32API

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegOpenKeyExW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		private static extern int RegOpenKeyEx( HKEY hkey,
					string subKey, uint options, RegSAM samDesired, out HKEY result );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegCreateKeyExW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		private static extern int RegCreateKeyEx(
					HKEY hKey, string lpSubKey, int Reserved, string lpClass, RegOption dwOptions, RegSAM samDesired,
					ref SECURITY_ATTRIBUTES lpSecurityAttributes, out HKEY phkResult, out RegResult lpdwDisposition );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegQueryValueExW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		private static extern int RegQueryValueEx( HKEY hKey, string lpValueName, int lpReserved, out uint lpType,
											byte[] lpData, ref uint lpcbData );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegSetValueExW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		private static extern int RegSetValueEx( HKEY hKey, string lpValueName, int Reserved,
											uint dwType, byte[] lpData, int cbData );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegEnumKeyExW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		extern private static int RegEnumKeyEx( HKEY hkey, uint index, StringBuilder lpName,
										ref uint lpcbName, IntPtr reserved, IntPtr lpClass, IntPtr lpcbClass,
										IntPtr nullParam );

		[DllImport( RegistryHelper.KERNEL32_DLL, EntryPoint = "LoadLibraryW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		private static extern HANDLE LoadLibraryWin32( string file );

		[DllImport( RegistryHelper.KERNEL32_DLL, EntryPoint = "FreeLibrary", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		private static extern bool FreeLibraryWin32( HANDLE handle );

		[DllImport( RegistryHelper.KERNEL32_DLL, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		public static extern UIntPtr GetProcAddress( HANDLE hModule, string procName );

		[DllImport( RegistryHelper.KERNEL32_DLL, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		[return: MarshalAs( UnmanagedType.Bool )]
		public static extern bool IsWow64Process( HANDLE hProcess, [Out] out bool wow64Process );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegCloseKey", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern int RegCloseKey( HKEY hKey );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegDeleteKeyExW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		public static extern int RegDeleteKeyEx( HKEY hKey, string lpSubKey, RegSAM samDesired, uint Reserved );

		[DllImport( RegistryHelper.ADVAPI32_DLL, EntryPoint = "RegDeleteKeyW", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
		public static extern int RegDeleteKey( HKEY hKey, string lpSubKey );

		#endregion

		/// <summary>
		/// Checks if this process is a WoW64 process.
		/// </summary>
		/// <returns></returns>
		private static bool IsWoW64Process()
		{
			bool wow64 = false;
			HANDLE hKernel32 = HANDLE.Zero;
			try
			{
				hKernel32 = LoadLibraryWin32( "KERNEL32.DLL" );
				if( hKernel32 == HANDLE.Zero )
					throw new Win32Exception( Marshal.GetLastWin32Error() );

				// Check if the IsWoW64Process method is available.
				UIntPtr uiPtrIsWoW64PRocess = GetProcAddress( hKernel32, "IsWow64Process" );
				if( uiPtrIsWoW64PRocess != UIntPtr.Zero )
				{
					HANDLE hCurrentProcess = System.Diagnostics.Process.GetCurrentProcess().Handle;
					if( !IsWow64Process( hCurrentProcess, out wow64 ) )
						throw new Win32Exception( Marshal.GetLastWin32Error() );
				}
			}
			finally
			{
				if( hKernel32 != HANDLE.Zero )
					FreeLibraryWin32( hKernel32 );
			}
			return wow64;
		}

		private static void UpdateSecurityAccessMask(
			enumRegistryAccessFlags dwFlags,  //!< Options for accessing the reqistry, see enumRegistryAccessFlags.
			ref RegSAM samDesired // Security access mask (e.g. KEY_READ or KEY_WRITE).
		)
		{
			// Test for OS version. Registry access flags are used only on 64-bit OS.
			bool is64BitProcess = ( IntPtr.Size == 8 );
			bool isWW64Process = IsWoW64Process();
			if( is64BitProcess || isWW64Process )
			{
				// Add WoW64 flag to security access mask if Win32 registry is required.
				if( ( dwFlags & enumRegistryAccessFlags.eregaccessWoW64_32bit ) != enumRegistryAccessFlags.eregaccessDefault )
					samDesired |= RegSAM.WOW64_32Key;

				// Add native 64-bit registry flag to security access mask if X64 registry is required.
				if( ( dwFlags & enumRegistryAccessFlags.eregaccessSystemNative ) != enumRegistryAccessFlags.eregaccessDefault )
					samDesired |= RegSAM.WOW64_64Key;
			}
		}

		private static SECURITY_ATTRIBUTES GetEmptySecurityAttributes()
		{
			SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
			sa.nLength = Marshal.SizeOf( sa ); // declared in System.Runtime.InteropServices
			sa.lpSecurityDescriptor = IntPtr.Zero;
			sa.bInheritHandle = 0;
			return sa;
		}

		private static uint GetValueTypeAsInt( RegistryValueKind rvk )
		{
			switch( rvk )
			{
			case RegistryValueKind.DWord:
				return REG_DWORD;
			case RegistryValueKind.String:
				return REG_SZ;
			case RegistryValueKind.MultiString:
				return REG_MULTI_SZ;
			default:
				throw new Exception( "Unsupported registry value type." );
			}
		}
	}
}
