using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke
{
    /// <summary>
    /// Native methods in kernel32.dll
    /// </summary>
    public static class Kernel32Native
    {
        [StructLayout( LayoutKind.Sequential )]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }
       
        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool ReadProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead );

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr OpenProcess( ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, uint dwProcessId );

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr VirtualAllocEx( IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect );

        [DllImport( "psapi.dll", SetLastError = true )]
        public static extern bool EnumProcessModules( IntPtr hProcess, IntPtr[] lphModule, uint cb, out uint lpcbNeeded );

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This function is in Kernel32.dll since Windows 7.
        /// </remarks>
        [DllImport( "psapi.dll", SetLastError = true )]
        public static extern bool GetModuleInformation( IntPtr hProcess, IntPtr hModule, out MODULEINFO lpmodinfo, uint cb );

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr CreateEvent( IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string name );

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool ResetEvent( IntPtr hEvent );

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool SetEvent( IntPtr hEvent );


        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern uint WaitForSingleObject( IntPtr hHandle, uint dwMilliseconds );

		[DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
		public static extern bool FreeConsole();

	}
}
