using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Enums
{
	/// <summary>
	/// Access mode to use when opening the file. 
	/// The default access mode is READ. The following
	/// access modes can be specified with AVIFileOpen.
	/// </summary>
	[Flags]
	public enum FileAccessMode
	{
		/// <summary>
		/// Opens the file for reading.
		/// </summary>
		OF_READ = 0x00000000,
		/// <summary>
		/// Opens the file for writing.
		/// </summary>
		OF_WRITE = 0x00000001,
		/// <summary>
		/// Opens the file for reading and writing.
		/// </summary>
		OF_READWRITE = 0x00000002,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_SHARE_COMPAT = 0x00000000,
		/// <summary>
		/// Opens the file and denies other processes any access to it. 
		/// AVIFileOpen fails if any other process has opened the file.
		/// </summary>
		OF_SHARE_EXCLUSIVE = 0x00000010,
		/// <summary>
		/// Opens the file nonexclusively. Other processes can open the 
		/// file with read access. AVIFileOpen fails if another process has 
		/// opened the file in compatibility mode or has write access to it.
		/// </summary>
		OF_SHARE_DENY_WRITE = 0x00000020,
		/// <summary>
		/// Opens the file nonexclusively. Other processes can open the 
		/// file with write access. AVIFileOpen fails if another process 
		/// has opened the file in compatibility mode or has read access to it.
		/// </summary>
		OF_SHARE_DENY_READ = 0x00000030,
		/// <summary>
		/// Opens the file nonexclusively. Other processes can open the 
		/// file with read or write access. AVIFileOpen fails if another 
		/// process has opened the file in compatibility mode.
		/// </summary>
		OF_SHARE_DENY_NONE = 0x00000040,
		/// <summary>
		/// Skips time-consuming operations, such as building an index. 
		/// Set this flag if you want the function to return as quickly 
		/// as possible—for example, if you are going to query the file 
		/// properties but not read the file.
		/// </summary>
		OF_PARSE = 0x00000100,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_DELETE = 0x00000200,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_VERIFY = 0x00000400,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_CANCEL = 0x00000800,
		/// <summary>
		/// Creates a new file. If the file already exists, it is truncated to zero length.
		/// </summary>
		OF_CREATE = 0x00001000,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_PROMPT = 0x00002000,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_EXIST = 0x00004000,
		/// <summary>
		/// 
		/// </summary>
		// TODO : Put documentation
		OF_REOPEN = 0x00008000,
	}

}
