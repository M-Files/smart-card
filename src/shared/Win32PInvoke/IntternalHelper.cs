using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke
{
	internal static class IntternalHelper
	{
		

		/// <summary>
		/// Unpacks an array of structures from unmanaged memory
		/// arr.Length is the number of items to unpack. don't overrun.
		/// </summary>
		/// <typeparam name="T">Target array.</typeparam>
		/// <param name="arr"></param>
		/// <param name="start"></param>
		/// <param name="stride"></param>
		internal static void PtrToStructureArray< T >( T[] arr, IntPtr start ) where T : new()
		{
			// Nothing to do?
			if( arr.Length == 0 )
				return;

			// Read all elements.
			long ptr = start.ToInt64();
			long elementSize = Marshal.SizeOf( new T() );
			for ( int i = 0; i < arr.Length; i++, ptr += elementSize )
			{				
				arr[ i ] = ( T ) Marshal.PtrToStructure( new IntPtr( ptr ), typeof( T ) );
			}
		}		
	}
}
