using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using MFiles.Internal.Win32PInvoke.Enums;
using MFiles.Internal.Win32PInvoke.Structs;

namespace MFiles.Internal.Win32PInvoke
{
    public static class Advapi32
    {
        public static int LOGON32_LOGON_INTERACTIVE = 2;
        public static int LOGON32_PROVIDER_DEFAULT = 0;

        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
        public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        public const UInt32 TOKEN_DUPLICATE = 0x0002;
        public const UInt32 TOKEN_IMPERSONATE = 0x0004;
        public const UInt32 TOKEN_QUERY = 0x0008;
        public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
        public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
        public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
        public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
        public const UInt32 TOKEN_READ = ( STANDARD_RIGHTS_READ | TOKEN_QUERY );
        public const UInt32 TOKEN_ALL_ACCESS = ( STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID );

		public const UInt32 SE_GROUP_INTEGRITY = 0x00000020;

		#region Structs

        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
        public struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        #endregion

        #region Enumerations
    

        public enum SECURITY_IMPERSONATION_LEVEL : int
        {
            /// <summary>
            /// The server process cannot obtain identification information about the client,
            /// and it cannot impersonate the client. It is defined with no value given, and thus,
            /// by ANSI C rules, defaults to a value of zero.
            /// </summary>
            SecurityAnonymous = 0,

            /// <summary>
            /// The server process can obtain information about the client, such as security identifiers and privileges,
            /// but it cannot impersonate the client. This is useful for servers that export their own objects,
            /// for example, database products that export tables and views.
            /// Using the retrieved client-security information, the server can make access-validation decisions without
            /// being able to use other services that are using the client's security context.
            /// </summary>
            SecurityIdentification = 1,

            /// <summary>
            /// The server process can impersonate the client's security context on its local system.
            /// The server cannot impersonate the client on remote systems.
            /// </summary>
            SecurityImpersonation = 2,

            /// <summary>
            /// The server process can impersonate the client's security context on remote systems.
            /// NOTE: Windows NT:  This impersonation level is not supported.
            /// </summary>
            SecurityDelegation = 3,
        }

        public enum TOKEN_INFORMATION_CLASS
        {
            /// <summary>
            /// The buffer receives a TOKEN_USER structure that contains the user account of the token.
            /// </summary>
            TokenUser = 1,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the group accounts associated with the token.
            /// </summary>
            TokenGroups,

            /// <summary>
            /// The buffer receives a TOKEN_PRIVILEGES structure that contains the privileges of the token.
            /// </summary>
            TokenPrivileges,

            /// <summary>
            /// The buffer receives a TOKEN_OWNER structure that contains the default owner security identifier (SID) for newly created objects.
            /// </summary>
            TokenOwner,

            /// <summary>
            /// The buffer receives a TOKEN_PRIMARY_GROUP structure that contains the default primary group SID for newly created objects.
            /// </summary>
            TokenPrimaryGroup,

            /// <summary>
            /// The buffer receives a TOKEN_DEFAULT_DACL structure that contains the default DACL for newly created objects.
            /// </summary>
            TokenDefaultDacl,

            /// <summary>
            /// The buffer receives a TOKEN_SOURCE structure that contains the source of the token. TOKEN_QUERY_SOURCE access is needed to retrieve this information.
            /// </summary>
            TokenSource,

            /// <summary>
            /// The buffer receives a TOKEN_TYPE value that indicates whether the token is a primary or impersonation token.
            /// </summary>
            TokenType,

            /// <summary>
            /// The buffer receives a SECURITY_IMPERSONATION_LEVEL value that indicates the impersonation level of the token. If the access token is not an impersonation token, the function fails.
            /// </summary>
            TokenImpersonationLevel,

            /// <summary>
            /// The buffer receives a TOKEN_STATISTICS structure that contains various token statistics.
            /// </summary>
            TokenStatistics,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS structure that contains the list of restricting SIDs in a restricted token.
            /// </summary>
            TokenRestrictedSids,

            /// <summary>
            /// The buffer receives a DWORD value that indicates the Terminal Services session identifier that is associated with the token.
            /// </summary>
            TokenSessionId,

            /// <summary>
            /// The buffer receives a TOKEN_GROUPS_AND_PRIVILEGES structure that contains the user SID, the group accounts, the restricted SIDs, and the authentication ID associated with the token.
            /// </summary>
            TokenGroupsAndPrivileges,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenSessionReference,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token includes the SANDBOX_INERT flag.
            /// </summary>
            TokenSandBoxInert,

            /// <summary>
            /// Reserved.
            /// </summary>
            TokenAuditPolicy,

            /// <summary>
            /// The buffer receives a TOKEN_ORIGIN value.
            /// </summary>
            TokenOrigin,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION_TYPE value that specifies the elevation level of the token.
            /// </summary>
            TokenElevationType,

            /// <summary>
            /// The buffer receives a TOKEN_LINKED_TOKEN structure that contains a handle to another token that is linked to this token.
            /// </summary>
            TokenLinkedToken,

            /// <summary>
            /// The buffer receives a TOKEN_ELEVATION structure that specifies whether the token is elevated.
            /// </summary>
            TokenElevation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has ever been filtered.
            /// </summary>
            TokenHasRestrictions,

            /// <summary>
            /// The buffer receives a TOKEN_ACCESS_INFORMATION structure that specifies security information contained in the token.
            /// </summary>
            TokenAccessInformation,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is allowed for the token.
            /// </summary>
            TokenVirtualizationAllowed,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if virtualization is enabled for the token.
            /// </summary>
            TokenVirtualizationEnabled,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_LABEL structure that specifies the token's integrity level.
            /// </summary>
            TokenIntegrityLevel,

            /// <summary>
            /// The buffer receives a DWORD value that is nonzero if the token has the UIAccess flag set.
            /// </summary>
            TokenUIAccess,

            /// <summary>
            /// The buffer receives a TOKEN_MANDATORY_POLICY structure that specifies the token's mandatory integrity policy.
            /// </summary>
            TokenMandatoryPolicy,

            /// <summary>
            /// The buffer receives the token's logon security identifier (SID).
            /// </summary>
            TokenLogonSid,

            /// <summary>
            /// The maximum value for this enumeration
            /// </summary>
            MaxTokenInfoClass
        }

        #endregion

		/// <summary>
		/// Enables the speciied privilege for the token.
		/// </summary>
		/// <param name="privilege">Privilege to enable.</param>
		/// <param name="token">Token.</param>
        public static void EnablePrivilege( string privilege, IntPtr token )
        {
            var locallyUniqueIdentifier = new LUID();            
            if( !Advapi32.LookupPrivilegeValue( null, privilege, ref locallyUniqueIdentifier ) )
                throw new Win32Exception();

            var TOKEN_PRIVILEGES = new TOKEN_PRIVILEGES();
            TOKEN_PRIVILEGES.PrivilegeCount = 1;
            TOKEN_PRIVILEGES.Attributes = (uint) TokenPrivileges.SE_PRIVILEGE_ENABLED;
            TOKEN_PRIVILEGES.Luid = locallyUniqueIdentifier;

            if( !Advapi32.AdjustTokenPrivileges( token, false,
                                                ref TOKEN_PRIVILEGES, 1024, IntPtr.Zero, 0 ) )
                throw new Win32Exception();
        }



		[DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int LogonUser(String lpszUserName, 
	        String lpszDomain, 
	        String lpszPassword,
	        int dwLogonType, 
	        int dwLogonProvider, 
	        out System.IntPtr phToken
        );

        [DllImport( "advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        internal static extern bool LookupPrivilegeValue( string lpsystemname, string lpname, [MarshalAs( UnmanagedType.Struct )] ref LUID lpLuid );

        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true) ]
        public static extern int DuplicateToken(System.IntPtr hToken,
	        int impersonationLevel,
            out System.IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool RevertToSelf();

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool OpenProcessToken( IntPtr handle, uint DesiredAccess, out IntPtr TokeHandle );

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public extern static bool DuplicateTokenEx( IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes, SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken );

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool ConvertStringSidToSid( string StringSid, out IntPtr ptrSid );

        [DllImport( "advapi32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern Boolean SetTokenInformation( IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref TOKEN_MANDATORY_LABEL TokenInformation, UInt32 TokenInformationLength );

        [DllImport( "advapi32.dll" )]
        public static extern uint GetLengthSid( IntPtr pSid );

        [DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool CreateProcessAsUser( IntPtr hToken, string lpApplicationName, string lpCommandLine, SECURITY_ATTRIBUTES lpProcessAttributes, SECURITY_ATTRIBUTES lpThreadAttributes,
                bool bInheritHandles, CreationFlags dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation );

        [DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool CreateProcessWithLogonW( string lpUsername, IntPtr domain, string lpPassword, uint dwLogonFlags, string lpApplicationName, string lpCommandLine,
                uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInfo );

        [DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool LogonUser( string lpszUsername, IntPtr lpszDomain, string lpszPassword, LogonType dwLogonType, LogonProvider dwLogonProvider, out IntPtr handle );

        [DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool ImpersonateLoggedOnUser( IntPtr hToken );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool TerminateProcess( IntPtr hProcess, uint uExitCode );

        [DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool OpenThreadToken( IntPtr ThreadHandle, uint DesiredAccess, bool OpenAsSelf, out IntPtr TokenHandle );

        [DllImport( "advapi32.dll", SetLastError = true, CharSet = CharSet.Auto )]
        public static extern bool AdjustTokenPrivileges( IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, uint ReturnLength );

        [DllImport( "kernel32", SetLastError = true, CharSet = CharSet.Unicode )]
        public static extern uint ResumeThread( IntPtr hThread );

		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern uint SetNamedSecurityInfo( string pObjectName, SE_OBJECT_TYPE ObjectType, SECURITY_INFORMATION SecurityInfo, IntPtr psidOwner, IntPtr psidGroup, IntPtr pDacl, IntPtr pSacl );

		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern bool GetSecurityDescriptorSacl( IntPtr SecurityDescriptor, out bool lpbSaclPresent, out IntPtr pSacl, out bool lpbSaclDefaulted );

		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor( string StringSecurityDescriptor, uint StringSDRevision, out IntPtr sd, out uint SecurityDescriptorSize );

		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern bool CreateRestrictedToken( IntPtr ExistingTokenHandle, uint Flags, uint DisableSidCount, IntPtr SidsToDisable, uint DeletePrivilegeCount, IntPtr PrivilegesToDelete, uint RestrictedSidCount, IntPtr SidsToRestrict, out IntPtr NewTokenHandle );

		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern bool GetTokenInformation( IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength );

		[DllImport( "advapi32.dll", SetLastError = true )]
		public static extern bool ConvertSidToStringSid( IntPtr Sid, out string StringSid );
	}
}
