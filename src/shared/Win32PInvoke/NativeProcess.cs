using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace MFiles.Internal.Win32PInvoke
{
    public class NativeProcess : IDisposable
    {
        private IntPtr process;
        private SafeWaitHandle handle;
       	
        /// <summary>
        /// Initializes new process.
        /// </summary>
        /// <param name="process"></param>
        public NativeProcess( uint processId, Kernel32Native.ProcessAccessFlags access )
        {
            // Open the process.
            this.process = Kernel32Native.OpenProcess( access, false, processId );
			if( process == IntPtr.Zero )
				throw new Win32Exception();
            this.handle = new SafeWaitHandle( this.process, true );
        }

        /// <summary>
        /// Reads the memory of the process.
        /// </summary>
        /// <param name="targetBuffer"></param>
        /// <returns></returns>
        public long ReadMemory( IntPtr baseAddress, byte[] targetBuffer )
        {
            IntPtr bytesRead;         
            if( !Kernel32Native.ReadProcessMemory( this.process, baseAddress, targetBuffer, ( IntPtr ) targetBuffer.Length, out bytesRead ) )
                throw new Win32Exception();
            return (long) bytesRead;
        }

        /// <summary>
        /// Reserves memory from the address space of the process.
        /// </summary>
        /// <returns></returns>
        public IntPtr ReserveMemory( long size )
        {
            IntPtr baseAddress = Kernel32Native.VirtualAllocEx( this.process, IntPtr.Zero, (IntPtr) size, Kernel32Native.AllocationType.Commit, Kernel32Native.MemoryProtection.ExecuteReadWrite );
            if( baseAddress == IntPtr.Zero )
                throw new Win32Exception();
            return baseAddress;
        }

        /// <summary>
        /// Gets the modules of the process.
        /// </summary>
        /// <returns>Handles to the modules.</returns>
        public IntPtr[] GetModuleHandles()
        {
            // Determine the number modules.
            var empty = new IntPtr[ 0 ];
            uint bytesRequired;            
            Kernel32Native.EnumProcessModules( this.process, empty, 0, out bytesRequired );

            // Get the handles.
            var handlesRequired = bytesRequired / IntPtr.Size;
            var handles = new IntPtr[ handlesRequired ];
            uint bytesReserved = bytesRequired;
            if( !Kernel32Native.EnumProcessModules( this.process, handles, bytesReserved, out bytesRequired ) )
                throw new Win32Exception();

            return handles;
        }

        /// <summary>
        /// Gets information about a module in the process.
        /// </summary>
        /// <param name="module">Handle to the module.</param>
        public Kernel32Native.MODULEINFO GetModuleInformation( IntPtr module )
        {
            Kernel32Native.MODULEINFO moduleInfo = new Kernel32Native.MODULEINFO();
            if( !Kernel32Native.GetModuleInformation( this.process, module, out moduleInfo, (uint) Marshal.SizeOf( moduleInfo ) ) )
                throw new Win32Exception();
            return moduleInfo;
        }

        public void Dispose()
        {
            if( this.handle != null )
                this.handle.Dispose();
            this.handle = null;
        }
        
    }
}
