using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// The SID_AND_ATTRIBUTES structure represents a security identifier (SID) and its attributes. SIDs are used to uniquely identify users or groups.
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
    public struct SID_AND_ATTRIBUTES
    {
		/// <summary>
		/// A pointer to a SID structure.
		/// </summary>
		public IntPtr Sid;

		/// <summary>
		/// Specifies attributes of the SID. This value contains up to 32 one-bit flags. Its meaning depends on the definition and use of the SID.
		/// </summary>
        public uint Attributes;
    }
}
