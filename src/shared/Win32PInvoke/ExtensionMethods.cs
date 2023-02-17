using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke
{
	/// <summary>
	/// Extenstion methods
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Converta an array to ptr.
		/// </summary>
		/// <returns></returns>
		public static IntPtr ToPtr< T >( this T[] array ) where T : new()
		{
			// Allocate buffer.
			var itemLength = Marshal.SizeOf( new T() );
			IntPtr buffer = Marshal.AllocHGlobal( itemLength * array.Length );
			for( int i = 0; i < array.Length; i++ )
			{
				IntPtr item = new IntPtr( buffer.ToInt64() + i * itemLength );
				Marshal.StructureToPtr( array[ i ], item, false );
			}
			return buffer;
		}
	}
}
