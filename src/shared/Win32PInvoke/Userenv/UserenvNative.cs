using MFiles.Internal.Win32PInvoke.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MFiles.Internal.Win32PInvoke.Userenv
{
	/// <summary>
	/// Methods imported from userenv.dll.
	/// </summary>
	public static class UserenvNative
	{
		/// <summary>
		/// Retrieves the environment variables for the specified user. This block can then be passed to the CreateProcessAsUser function.
		/// </summary>
		/// <param name="lpEnvironment">When this function returns, receives a pointer to the new environment block. The environment block is an array of null-terminated Unicode strings. The list ends with two nulls (\0\0).</param>
		/// <param name="hToken">Token for the user, returned from the LogonUser function.</param>
		/// <param name="bInherit">Specifies whether to inherit from the current process' environment.</param>
		/// <returns></returns>
		[DllImport( "userenv.dll", SetLastError = true )]
		public static extern bool CreateEnvironmentBlock( out IntPtr lpEnvironment, IntPtr hToken, bool bInherit );

		/// <summary>
		/// Loads the specified user's profile. The profile can be a local user profile or a roaming user profile.
		/// </summary>
		/// <param name="hToken">Token for the user, which is returned by the LogonUser, CreateRestrictedToken, DuplicateToken, OpenProcessToken, or OpenThreadToken function. </param>
		/// <param name="lpProfileInfo">Pointer/reference to a PROFILEINFO structure.</param>
		/// <returns></returns>
		[DllImport( "userenv.dll", SetLastError = true )]
		public static extern bool LoadUserProfile( IntPtr hToken, ref PROFILEINFO lpProfileInfo );
	}
}
