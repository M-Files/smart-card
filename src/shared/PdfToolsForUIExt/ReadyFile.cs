using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Represents the ready file.
	/// </summary>
	internal class ReadyFile
	{
		/// <summary>
		/// The error code read from the ready file.
		/// </summary>
		internal int ErrorCode { get; private set;  }

		/// <summary>
		/// Error message read from the ready file.
		/// </summary>
		internal string ErrorMessage { get; private set; }

		/// <summary>
		/// Error stack read from the ready file.
		/// </summary>
		internal string ErrorStack { get; private set; }

		/// <summary>
		/// Initializes the ready file.
		/// </summary>
		/// <param name="path"></param>
		internal ReadyFile( string path )
		{
			// Open the ready file and read the data from it.
			using( var sr = new StreamReader( path ) )
			{
				// Parse the possible error message from the ready file.
				this.ErrorCode = int.Parse( sr.ReadLine() );
				if( this.ErrorCode != ApplicationErrorCodes.Success )
				{
					// An error has occurred. Display the error.
					this.ErrorMessage = sr.ReadLine();
					this.ErrorStack = sr.ReadToEnd();
				}

			}  // end using.
		}

		/// <summary>
		/// Attempts to initialize the ready file.
		/// </summary>
		/// <param name="path">Path to the ready fiel.</param>
		/// <returns>Parsed ready file.</returns>
		internal static ReadyFile TryRead( string path )
		{
			// Try reading.
			try
			{
				// Something written to the ready file?
				var fileInfo = new FileInfo( path );
				if( fileInfo.Length == 0 )
					return null;

				// Return the ready file.
				return new ReadyFile( path );
			}
			catch( Exception )
			{
				// Assume that the ready file was still locked.
				return null;
			}
	
		}
	}
}
