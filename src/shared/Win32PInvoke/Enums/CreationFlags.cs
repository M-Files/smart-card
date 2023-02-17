using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Enums
{
	[Flags]
    public enum CreationFlags
    {
        CREATE_SUSPENDED = 0x00000004,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        CREATE_SEPARATE_WOW_VDM = 0x00000800,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NO_WINDOW = 0x08000000,
        DETACHED_PROCESS = 0x00000008,
    }
}
