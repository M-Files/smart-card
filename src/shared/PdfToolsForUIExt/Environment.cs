using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace MFiles.PdfTools.UIExt
{
	/// <summary>
	/// Provides general information about the environment for the UI EXt application.
	/// </summary>
	public class Environment
	{
		/// <summary>
		/// Gets the current culture.
		/// </summary>
		/// <returns>Culture as ISO string.</returns>
		public string GetCurrentUICulture()
		{
			// Return the UI culture.
			return CultureInfo.CurrentUICulture.Name;
		}

		/// <summary>
		/// Gets the current culture.
		/// </summary>
		/// <returns>Culture as ISO string.</returns>
		public string GetCurrentCulture()
		{
			// Return the UI culture.
			return CultureInfo.CurrentCulture.Name;
		}
	}
}
