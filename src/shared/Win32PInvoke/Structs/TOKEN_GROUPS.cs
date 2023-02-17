using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Structs
{
	/// <summary>
	/// The TOKEN_GROUPS structure contains information about the group security identifiers (SIDs) in an access token.
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct TOKEN_GROUPS {

		/// <summary>
		/// Specifies the number of groups in the access token. 
		/// </summary>
		public uint GroupCount;

		/// <summary>
		/// Specifies an array of SID_AND_ATTRIBUTES structures that contain a set of SIDs and corresponding attributes. 
		/// </summary>
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
		public SID_AND_ATTRIBUTES[] Groups;

		/// <summary>
		/// Gets groups.
		/// </summary>
		/// <remarks>
		/// Use this function to actually resolve are groups from IntPtr received from GetTokenInformation.
		/// </remarks>
		/// <returns></returns>
		public static SID_AND_ATTRIBUTES[] GetGroups( IntPtr tokenGroups )
		{
			var us = ( TOKEN_GROUPS ) Marshal.PtrToStructure( tokenGroups, typeof( TOKEN_GROUPS ) );
			var attributes = new SID_AND_ATTRIBUTES[ us.GroupCount ];
			IntPtr initialOffest = Marshal.OffsetOf( typeof( TOKEN_GROUPS ), "Groups" );
			IntternalHelper.PtrToStructureArray( attributes, new IntPtr( tokenGroups.ToInt64() + initialOffest.ToInt64() ) );
			return attributes;
		}
	};
}
