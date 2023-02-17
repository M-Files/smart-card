using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ionic.Zip;
using MFiles.PdfTools.UIExt;

namespace Builder
{
	class Program
	{
		static void Main( string[] args )
		{
			// Check arguments.
			if( args.Length < 1 )
				throw new ArgumentException( "Missing arguments: <solution directory>" );
			string outputDirectory = Path.Combine( args[ 0 ], "Output" );
			if( !Directory.Exists( outputDirectory ) )
				throw new DirectoryNotFoundException( string.Format( "Output directory {0} does not exist", outputDirectory ) );

			// Locate the ui extension.
			var fileSigner = Path.Combine( args[ 0 ], "..\\FileSigner" );
			if( !Directory.Exists( fileSigner ) )
				throw new DirectoryNotFoundException( string.Format( "The file signer UI Extension {0} does not exist", fileSigner ) );

			// Cleanup the output directory.
			var packageOutput = Path.Combine( outputDirectory, "FileSigner" );
			if( Directory.Exists( packageOutput ) )
				Directory.Delete( packageOutput, true );
			var targetZip = Path.Combine( outputDirectory, "filesigner.mfappx" );
			if( File.Exists( targetZip ) )
				File.Delete( targetZip );

			// Copy UI Extension to output.
			DirectoryCopy( fileSigner, packageOutput, true );

			// Copy the .NET application
			var managedOutput = Path.Combine( packageOutput, "managed" );
			
			// Copy PDF Tools executiuon binaries.
			var currentAssembly = new FileInfo( Assembly.GetExecutingAssembly().Location );
			{
				var binSource = Path.Combine( currentAssembly.DirectoryName, "bin" );
				var binTarget = Path.Combine( managedOutput, "bin" );
				DirectoryCopy( binSource, binTarget, true );
			}

			// Copy the launcher.
			{
				var binariesToCopy = Launcher.GetRequiredBinaries();
				foreach( var binary in binariesToCopy )
				{
					var sourcePath = Path.Combine( currentAssembly.DirectoryName, binary );
					var targetPath = Path.Combine( managedOutput, binary );
					var target = new FileInfo( targetPath );
					if( !target.Directory.Exists )
						target.Directory.Create();
					File.Copy( sourcePath, targetPath );
				}
			}

			// Package the UI Ext application.
			using( var zip = new ZipFile( targetZip ) )
			{
				zip.AddDirectory( packageOutput );
				zip.Save();
			}

			// Test.
			// var launch = new Launcher();
			// launch.SignFile( "Sample PDF.pdf", true );
		}

		private static void DirectoryCopy( string sourceDirName, string destDirName, bool copySubDirs )
		{
			// Get the subdirectories for the specified directory.
			var current = new DirectoryInfo( sourceDirName );
			var subDirs = current.GetDirectories();

			if( !current.Exists )
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName );
			}

			// If the destination directory doesn't exist, create it.
			if( !Directory.Exists( destDirName ) )
				Directory.CreateDirectory( destDirName );

			// Get the files in the directory and copy them to the new location.
			FileInfo[] files = current.GetFiles();
			foreach( FileInfo file in files )
			{
				string temppath = Path.Combine( destDirName, file.Name );
				file.CopyTo( temppath, false );
			}

			// If copying subdirectories, copy them and their contents to new location. 
			if( copySubDirs )
			{
				foreach( DirectoryInfo subdir in subDirs )
				{
					string temppath = Path.Combine( destDirName, subdir.Name );
					DirectoryCopy( subdir.FullName, temppath, copySubDirs );
				}
			}
		}
	}
}
