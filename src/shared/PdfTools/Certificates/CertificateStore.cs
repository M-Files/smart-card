using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MFiles.PdfTools.Certificates
{
	/// <summary>
	/// Manages certificates installed on the system.
	/// </summary>
	public class CertificateStore : IDisposable
	{	
		/// <summary>
		/// The certificate store.
		/// </summary>
		private X509Store Store { get; set; }
		
		/// <summary>
		/// Initializes the certificate store.
		/// </summary>
		/// <remarks>Currently defaults user's personal store.</remarks>
		public CertificateStore()
		{
			// Open the store.
			this.Store = this.OpenStore( OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly );
		}

		/// <summary>
		/// Finds a certificate matching the specified criteria.
		/// </summary>
		/// <param name="conditions">Search conditions.</param>
		/// <returns>Certificates matching the conditions</returns>
		public X509Certificate2Collection FindCertificates( params ICertificateCondition[] conditions )
		{
			// Process all conditions.
			var result = this.Store.Certificates;
			foreach( var condition in conditions )
			{
				// Process current condition.
				result = Conditions.ProcessCondition( result, condition );				
				if( result.Count == 0 )				
					break;
			}
			return result;
		}

		/// <summary>
		/// Removes the specified certificate from the key store.
		/// </summary>
		/// <param name="certificate">The certificate to remove.</param>
		public void RemoveCertificate( X509Certificate2 certificate )
		{
			// Open the store for writing.
			X509Store store = null;
			try
			{
				// Open the store for writing and remove the certificate.
				store = this.OpenStore( OpenFlags.ReadWrite | OpenFlags.OpenExistingOnly );
				store.Remove( certificate );
			}
			finally 
			{
				if( store != null )
					store.Close();
				store = null;
			}
		}
		
		public void Dispose()
		{
			if( this.Store != null )
				this.Store.Close();
			this.Store = null;
		}

		/// <summary>
		/// Opens the key store with the specified access flags.
		/// </summary>
		/// <param name="accessFlags">Required access.</param>
		/// <returns>Opened key store.</returns>
		private X509Store OpenStore( OpenFlags accessFlags )
		{
			// Open the key store.
			var store = new X509Store( StoreName.My, StoreLocation.CurrentUser );
			store.Open( accessFlags );
			return store;
		}
	}
}
