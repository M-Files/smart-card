using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace MFilesSmartCard
{
	/// <summary>
	/// A simple class to ensure, that temporary files are deleted after use.
	/// </summary>
	class TempFile: IDisposable
	{
		public string FilePath { get; private set; }

		/// <summary>
		/// Creates a new temporary file by asking a new temp file name from the OS.
		/// </summary>
		public TempFile()
		{
			FilePath = Path.GetTempFileName();
		}

		/// <summary>
		/// Compares the content of this file to the other.
		/// </summary>
		/// <param name="other">True if the content of the files are equal.</param>
		/// <returns>True if the content of the files are equal.</returns>
		public bool ContentEquals( TempFile other )
		{
			// Compare file sizes.
			var thisFile = new FileInfo( this.FilePath );
			var otherFile = new FileInfo( other.FilePath );
			if( thisFile.Length != otherFile.Length )
				return false;

			// Compare content.
			var thisBuffer = new byte[4096];
			var otherBuffer = new byte[4096];
			using( var thisReader = thisFile.OpenRead() )
			using( var otherReader = otherFile.OpenRead() )
			{
				// Keep comparing until all compared.
				int bytesToCompareReference = 0;
				do
				{
					// Read content from the files for comparison.
					bytesToCompareReference = thisReader.Read( thisBuffer, 0, thisBuffer.Length );
					int bytesToCompare = otherReader.Read( otherBuffer, 0, otherBuffer.Length );
					if( bytesToCompare != bytesToCompareReference )
						return false;

					// Compare the buffers.
					if( !thisBuffer.SequenceEqual( otherBuffer ) )
						return false;

				} while( bytesToCompareReference == thisBuffer.Length );

			}  // end using.

			// Comparison done. Files were equal.
			return true;
		}

		/// <summary>
		/// Deletes the temporary file, if it still exists.
		/// </summary>
		public void Dispose()
		{
			// Cleanup.
			if( File.Exists( FilePath ) )
				File.Delete( FilePath );
		}
	}
}
