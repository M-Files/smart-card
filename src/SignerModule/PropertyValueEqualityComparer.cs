using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFilesAPI;

namespace MFilesSmartCard
{
	/// <summary>
	/// Simple comparer for M-Files Property Values.
	/// </summary>
	public class PropertyValueEqualityComparer : IEqualityComparer<PropertyValue>
	{
		/// <summary>
		/// Runs a deep comparison between two property values.
		/// </summary>
		public bool Equals( PropertyValue pv1, PropertyValue pv2 )
		{
			if( pv1.PropertyDef != pv2.PropertyDef )
				return false;

			if( pv1.Value.DataType != pv2.Value.DataType )
				return false;

			if( pv1.Value.IsUninitialized() != pv2.Value.IsUninitialized() )
				return false;

			if( pv1.Value.IsNULL() != pv2.Value.IsNULL() )
				return false;

			if( pv1.Value.CompareTo( pv2.Value ) != 0 )
				return false;

			return true;
		}

		/// <summary>
		/// Generates a hash code from property value.
		/// </summary>
		public int GetHashCode( PropertyValue pv )
		{
			return pv.PropertyDef ^ ( int )pv.Value.DataType ^ pv.GetValueAsUnlocalizedText().GetHashCode();
		}
	}
}
