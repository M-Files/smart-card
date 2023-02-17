using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// The TOKEN_PRIVILEGES structure contains information about a set of privileges for an access token.
	/// </summary>
	/// <remarks>
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/aa379630%28v=vs.85%29.aspx
	/// </remarks>
	[StructLayout( LayoutKind.Sequential )]
	public struct TOKEN_PRIVILEGES
	{
		/// <summary>
		/// This must be set to the number of entries in the Privileges array.
		/// </summary>
		public UInt32 PrivilegeCount;

		/// <summary>
		/// LUID_AND_ATTRIBUTES
		/// </summary>
		/// <remarks>
		/// The last member of this struct is actually an array LUID_AND_ATTRIBUTES[ANYSIZE_ARRAY].
		/// 
		/// For convenience the first item is extracted here.
		/// </remarks>
		public LUID Luid;
		public UInt32 Attributes;
	}
}
