using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Constants
{
	/// <summary>
	/// Generic access right constants.
	/// </summary>
	/// <remarks>
	///  Securable objects use an access mask format in which the four high-order bits specify generic access rights.
	///  Each type of securable object maps these bits to a set of its standard and object-specific access rights. 
	/// For example, a Windows file object maps the GENERIC_READ bit to the READ_CONTROL and SYNCHRONIZE standard access rights and to the FILE_READ_DATA, FILE_READ_EA, 
	/// and FILE_READ_ATTRIBUTES object-specific access rights. Other types of objects map the GENERIC_READ bit to whatever set of access rights is appropriate for that type of object.
	/// 
	/// You can use generic access rights to specify the type of access you need when you are opening a handle to an object. 
	/// This is typically simpler than specifying all the corresponding standard and specific rights.
	/// </remarks>
	public static class AccessRights
	{
		public const uint GENERIC_READ = 0x80000000;
		public const uint GENERIC_WRITE = 0x40000000;
		public const uint GENERIC_EXECUTE = 0x20000000;
		public const uint GENERIC_ALL = 0x10000000;
	}
}
