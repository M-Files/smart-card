using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// Contains information used when loading or unloading a user profile.
	/// </summary>
	/// <remarks>
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/bb773378%28v=vs.85%29.aspx
	/// </remarks>
	[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
	public struct PROFILEINFO
	{
		/// <summary>
		/// The size of this structure, in bytes.
		/// </summary>
		public uint dwSize;

		/// <summary>
		/// This member can be one of the following flags:
		/// PI_NOUI
		/// Prevents the display of profile error messages.
		///
		/// PI_APPLYPOLICY
		/// Not supported.
		/// </summary>
		public uint dwFlags;

		/// <summary>
		/// A pointer to the roaming user profile path. If the user does not have a roaming profile, this member can be NULL. To retrieve the user's roaming profile path, call the NetUserGetInfo function, specifying information level 3 or 4. For more information, see Remarks.
		/// </summary>
		public string lpUserName;

		/// <summary>
		/// A pointer to the default user profile path. This member can be NULL.
		/// </summary>
		public string lpProfilePath;

		/// <summary>
		/// A pointer to the default user profile path. This member can be NULL.
		/// </summary>
		public string lpDefaultPath;

		/// <summary>
		/// A pointer to the name of the validating domain controller, in NetBIOS format.
		/// </summary>
		public string lpServerName;

		/// <summary>
		/// Not used, set to NULL.
		/// </summary>
		public string lpPolicyPath;

		/// <summary>
		/// A handle to the HKEY_CURRENT_USER registry subtree. For more information, see the Remarks section in MSDN.
		/// </summary>
		IntPtr hProfile;
	}	
}
