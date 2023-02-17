using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Pdftools.PdfSecure;

namespace MFiles.PdfTools
{
	/// <summary>
	/// Configuration manager for the PDF tools.
	/// </summary>
	public static class ConfigurationManager
	{
		/// <summary>
		/// Mutex protecting the initialization.
		/// </summary>
		private static readonly object InitializationMutex = new object();

		/// <summary>
		///  Has this module been initialized?
		/// </summary>
		private static bool initialized;

		/// <summary>
		/// Handle to the loaded PDF secure API.
		/// </summary>
		private static IntPtr pdfSecureApi = IntPtr.Zero;

		/// <summary>
		/// Initializes the configuration.
		/// </summary>
		public static void Initialize()
		{
			// Initializes the module.
			lock( InitializationMutex )
			{
				// Other initiliazation operations already done?
				if( ConfigurationManager.initialized )
					return;

				// Pre-load native DLLs.
				// This is done for the following two reasons:
				// 1. Support for both x86 and x64 architectures. By pre-loading the DLL we can select the correct one for the curretn architecture.
				// 2. Avoid problems when Windows has been hardened against DLL injection attacks. 
				//	Loading the PDFSecureAPI.dll  fails in one of our customer's environment if we allow PDFToolsNET assembly to load that DLL 
				//    unless the PDFSecureAPI.dll is copied to c:\Windows\system32
				//	- We assume here that the PDFToolsNET attempts to load the DLL by calling LoadLibrary with the name of the DLL only. 
				//	- - This is a bad practice and may introduce vulnerabilities. => 
				//	- We assume that the customers environment has been hardened agains this types of attacks. PDFToolsNET is unable to load PDFSecureAPI.dll.				
				//	By loading pre-loading the DLL using absolute path we avoid the problem.
				//
				// See also Dynamic-Link Library Security in http://msdn.microsoft.com/en-us/library/windows/desktop/ff919712%28v=vs.85%29.aspx
				LoadPdfSecureApi();

				// Initialization completed.
				ConfigurationManager.initialized = true;

			}  // end if
		}

		/// <summary>
		/// Uninitializes the configuration.
		/// </summary>
		public static void Uninitialize()
		{
			// Initializes the module.
			lock( InitializationMutex )
			{
				// Already uninitialized?
				if( ! ConfigurationManager.initialized )
					return;

				// Unload PDFSecureAPI.dll.
				if( ConfigurationManager.pdfSecureApi != IntPtr.Zero )
					FreeLibrary( ConfigurationManager.pdfSecureApi );
				ConfigurationManager.pdfSecureApi = IntPtr.Zero;

				// Uninitialization completed.
				ConfigurationManager.initialized = false;

			}  // end if
		}

		/// <summary>
		/// Gets path to the application.
		/// </summary>
		/// <returns>Path to the application.</returns>
		private static string GetApplicationDirectory()
		{
			var executingAssembly = new FileInfo( Assembly.GetExecutingAssembly().Location );
			return executingAssembly.Directory.FullName;
		}

		/// <summary>
		/// Determines the redist directory.
		/// </summary>
		/// <returns>redist directory.</returns>
		private static string GetRedistDirectoryForThisPlatform()
		{
			var executingAssembly = new FileInfo( Assembly.GetExecutingAssembly().Location );
			string redist = Path.Combine( executingAssembly.Directory.FullName, "redist" );
			if( IntPtr.Size == 8 )
			{
				redist = Path.Combine( redist, "x64" );
			}
			else
			{
				redist = Path.Combine( redist, "x86" );
			}
			return redist;
		}

		/// <summary>
		/// Gets path to the PDFSecureAPI.dll for this platform.
		/// </summary>
		/// <returns>Returns the API.</returns>
		private static string GetPdfSecureApiForThisPlatform()
		{
			// Return the API.
			var pdfSecureApi = Path.Combine( GetRedistDirectoryForThisPlatform(), "PdfSecureAPI.dll" );
			if( !File.Exists( pdfSecureApi ) )
				throw new FileNotFoundException( InternalResources.Error_General_ComComponentRegistrationFailed, pdfSecureApi );
			return pdfSecureApi;
		}

		/// <summary>
		/// Loads the PDF 
		/// </summary>
		private static void LoadPdfSecureApi()
		{
			// Avoid loading the library multiple times.
			if( ConfigurationManager.pdfSecureApi != IntPtr.Zero )
				FreeLibrary( ConfigurationManager.pdfSecureApi );
			ConfigurationManager.pdfSecureApi = IntPtr.Zero;

			// Load the PDFSecureAPI.dll
			var pdfSecureApiDllPath = GetPdfSecureApiForThisPlatform();
			ConfigurationManager.pdfSecureApi = LoadLibraryW( pdfSecureApiDllPath );
			if( ConfigurationManager.pdfSecureApi  == IntPtr.Zero )
				throw new Win32Exception( string.Format( "Loading PDFSecureAPI.dll from {0} failed.", pdfSecureApiDllPath ) );
		}

		/// <summary>
		/// LoadLibrary.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		[DllImport( "kernel32", SetLastError = true, CharSet = CharSet.Unicode )]
		private static extern IntPtr LoadLibraryW( string path );

		/// <summary>
		/// Frees library loaded with LoadLibrary.
		/// </summary>
		/// <param name="hModule"></param>
		/// <returns></returns>
		[DllImport( "kernel32.dll", SetLastError = true )]
		private static extern bool FreeLibrary( IntPtr hModule );
	}
}
