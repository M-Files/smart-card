using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ShowCertificates
{
	class Program
	{
		static void Main( string[] args )
		{
			// Get and open certificate store similarly to MFilesSmartCard app.
			var store = new X509Store( StoreName.My, StoreLocation.CurrentUser );
			store.Open( OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly );

			// Loop each found certificate.
			foreach( var cert in store.Certificates )
			{
				// Print information from certificate.
				Console.WriteLine( $"Issuer: {cert.Issuer}" );
				Console.WriteLine( $"Subject: {cert.Subject}" );
				//Console.WriteLine( $"SerialNumber: {cert.SerialNumber}" );
				//Console.WriteLine( $"SignatureAlgorithm: {cert.SignatureAlgorithm.FriendlyName}" );
				//Console.WriteLine( $"Thumbprint: {cert.Thumbprint}" );

				// Separate each certificate with new line.
				Console.WriteLine();
			}

			// Wait key press before exit.
			Console.WriteLine( "Press any key to exit." );
			Console.ReadKey();
		}
	}
}
