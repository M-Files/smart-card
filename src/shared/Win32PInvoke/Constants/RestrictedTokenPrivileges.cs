using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Constants
{
	public static class RestrictedTokenPrivileges
	{
		
		public static uint DISABLE_MAX_PRIVILEGE =  0x1;

		public static uint SANDBOX_INERT = 0x2;

		public static uint LUA_TOKEN = 0x4;

		public static uint WRITE_RESTRICTED = 0x8;

	}
}
