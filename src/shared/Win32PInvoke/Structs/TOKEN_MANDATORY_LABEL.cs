using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// The TOKEN_MANDATORY_LABEL structure specifies the mandatory integrity level for a token.
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct TOKEN_MANDATORY_LABEL
	{
		/// <summary>
		/// A SID_AND_ATTRIBUTES structure that specifies the mandatory integrity level of the token.
		/// </summary>
		public SID_AND_ATTRIBUTES Label;
	}
}
