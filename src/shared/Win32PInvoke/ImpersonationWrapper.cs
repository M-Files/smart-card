using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.ComponentModel;

namespace MFiles.Internal.Win32PInvoke
{
    /// <summary>
    /// Class that encapsulates information required to impersonate other users.
    /// </summary>
    public class ImpersonationWrapper : IDisposable
    {
        /// <summary>
        /// Identity of the user we are impersonating.
        /// </summary>
        private WindowsIdentity Identity { get; set; }

        /// <summary>
        /// Impersonation context.
        /// </summary>
        private WindowsImpersonationContext Context { get; set; }

        /// <summary>
        /// Impersenotaes a user.
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="domain">Domain of the user.</param>
        public void ImpersonateUser(string userName, string password, string domain)
        {
            IntPtr userToken = IntPtr.Zero;
            IntPtr userTokenImpersonate = IntPtr.Zero;
            try
            {
                // Revert to self.
                RevertToSelf();
                if (Advapi32.RevertToSelf() == false)
                    throw new Win32Exception();

                // Login as the user.
                if (Advapi32.LogonUser(userName, domain, password, Advapi32.LOGON32_LOGON_INTERACTIVE,
                        Advapi32.LOGON32_PROVIDER_DEFAULT, out userToken) == 0 )
                    throw new Win32Exception();

                // Duplicate the token we received for impersonation.
                if (Advapi32.DuplicateToken(userToken, 2, out userTokenImpersonate) == 0)
                    throw new Win32Exception();
                
                // Impersonate the user.
                Identity = new WindowsIdentity(userTokenImpersonate);
                Context = Identity.Impersonate();
            }
            finally
            {
                // Close the tokens.
                if( userToken != IntPtr.Zero )
                    Advapi32.CloseHandle(userToken);
                if( userTokenImpersonate != IntPtr.Zero )
                    Advapi32.CloseHandle(userTokenImpersonate);

            }  // end finally
            
        }

        /// <summary>
        /// Reverts the impersonation.
        /// </summary>
        public void RevertToSelf()
        {
            // Undo the impersonation.
            if( Context != null )
                Context.Undo();

            // Dispoes the additional information.
            if( Identity != null )
                Identity.Dispose(); Identity = null;
            if( Context != null )
                Context.Dispose(); Context = null;
        }


        #region IDisposable Members

        public void Dispose()
        {
            // Remove the impersonation.
            RevertToSelf();
        }

        #endregion        
    }
}
