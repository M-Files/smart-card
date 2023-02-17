using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MFiles.Internal.Json
{
	/// <summary>
	/// Common helper methods.
	/// </summary>
	internal static class Helper
	{
		/// <summary>
		/// Checks if the specified value is a nubmer.
		/// </summary>
		/// <param name="value">Value</param>
		/// <returns>True if the value is a number.</returns>
		internal static bool IsNumber( object value )
		{
			// Is number?
			switch( Type.GetTypeCode( value.GetType() ) )
			{
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.UInt16:
			case TypeCode.UInt32:
			case TypeCode.UInt64:
			case TypeCode.Int16:
			case TypeCode.Int32:
			case TypeCode.Int64:
			case TypeCode.Decimal:
			case TypeCode.Double:
			case TypeCode.Single:
				return true;
			default:
				return false;
			}
		}
	}
}
