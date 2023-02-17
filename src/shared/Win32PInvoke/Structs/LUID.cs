using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// An LUID is a 64-bit value guaranteed to be unique only on the system on which it was generated. The uniqueness of a locally unique identifier (LUID) is guaranteed only until the system is restarted.
	/// </summary>
	/// <remarks>
	/// https://msdn.microsoft.com/en-us/library/windows/desktop/aa379261%28v=vs.85%29.aspx
	/// </remarks>
	[StructLayout( LayoutKind.Sequential )]
    public struct LUID
    {
		/// <summary>
		/// Low-order bits.
		/// </summary>
        public uint LowPart;

		/// <summary>
		/// High-order bits.
		/// </summary>
        public int HighPart;
    }
}
