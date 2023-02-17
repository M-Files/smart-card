using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// e SECURITY_ATTRIBUTES structure contains the security descriptor for an object and specifies whether the handle retrieved by specifying this structure is inheritable. This structure provides security settings for objects created by various functions, such as CreateFile, CreatePipe, CreateProcess, RegCreateKeyEx, or RegSaveKeyEx.
	/// </summary>
	[ StructLayout( LayoutKind.Sequential )]
    public struct SECURITY_ATTRIBUTES
    {
		/// <summary>
		/// The size, in bytes, of this structure. Set this value to the size of the SECURITY_ATTRIBUTES structure.
		/// </summary>
        public int nLength;

		/// <summary>
		/// A pointer to a SECURITY_DESCRIPTOR structure that controls access to the object. 
		/// </summary>
		/// <remarks>
		/// No automatic deallocation functionality provided. Use with care.
		/// </remarks>
        public IntPtr lpSecurityDescriptor;

		/// <summary>
		/// A Boolean value that specifies whether the returned handle is inherited when a new process is created.
		/// </summary>
        public int bInheritHandle;
    }
}
