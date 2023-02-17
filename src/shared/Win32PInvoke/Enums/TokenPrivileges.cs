﻿using System;


namespace MFiles.Internal.Win32PInvoke.Enums
{
	[Flags]
	public enum TokenPrivileges : uint
	{

		SE_PRIVILEGE_ENABLED = 0x00000002,
		SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001,
		SE_PRIVILEGE_REMOVED = 0x00000004,
		SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000,
	}
}