using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFilesAPI;

namespace MFilesSmartCard
{
	static class Helpers
	{
		/// <summary>
		/// Converts ObjVer to Lookup.
		/// </summary>
		public static Lookup ObjVerToLookup( ObjVer objVer )
		{
			// Create lookup to specific version.
			return new Lookup()
			{
				ObjectType = objVer.Type,
				Item = objVer.ID,
				Version = objVer.Version
			};
		}

		/// <summary>
		/// Creates a not deleted condition.
		/// </summary>
		public static SearchCondition CreateNotDeletedCondition()
		{
			SearchCondition condition = new SearchCondition();
			condition.ConditionType = MFConditionType.MFConditionTypeEqual;
			condition.Expression.SetStatusValueExpression( MFStatusType.MFStatusTypeDeleted, null );
			condition.TypedValue.SetValue( MFDataType.MFDatatypeBoolean, false );
			return condition;
		}

		/// <summary>
		/// Creates a class condition.
		/// </summary>
		public static SearchCondition CreateClassSearchCondition( int classID )
		{
			SearchCondition condition = new SearchCondition();
			condition.ConditionType = MFConditionType.MFConditionTypeEqual;
			condition.Expression.SetPropertyValueExpression( ( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefClass, MFParentChildBehavior.MFParentChildBehaviorNone, null );
			condition.TypedValue.SetValue( MFDataType.MFDatatypeLookup, classID );
			return condition;
		}

		/// <summary>
		/// Creates a lookup condition.
		/// </summary>
		public static SearchCondition CreateLookupSearchCondition( int propertyDefID, Lookup lookup )
		{
			SearchCondition condition = new SearchCondition();
			condition.ConditionType = MFConditionType.MFConditionTypeEqual;
			condition.Expression.SetPropertyValueExpression( propertyDefID, MFParentChildBehavior.MFParentChildBehaviorNone, null );
			condition.TypedValue.SetValueToLookup( lookup );
			return condition;
		}

		/// <summary>
		/// Returns ObjectVersionAndProperties of the previous version.
		/// </summary>
		/// <param name="vault"></param>
		/// <param name="currentObjVer"></param>
		/// <returns></returns>
		public static ObjectVersionAndProperties GetPreviousObjectVersionAndProperties( Vault vault, ObjVer currentObjVer )
		{
			// Get the history of the current version.
			ObjectVersions history = vault.ObjectOperations.GetHistory( currentObjVer.ObjID );

			// Get the previous version.
			int currentVersion = currentObjVer.Version;
			ObjVer oldObjVer = Helpers.GetPreviousObjVer( history, currentVersion );

			// Get the properties.
			return vault.ObjectOperations.GetObjectVersionAndProperties( oldObjVer );
		}

		/// <summary>
		/// Returns the previous version from the current version.
		/// </summary>
		public static ObjVer GetPreviousObjVer( ObjectVersions history, int currentVersion )
		{
			ObjVer objVer = null;
			foreach( ObjectVersion version in history )
				if( version.ObjVer.Version < currentVersion && ( objVer == null || objVer.Version < version.ObjVer.Version ) )
					objVer = version.ObjVer;

			if( objVer == null )
				throw new Exception( "The previous version was not found." );

			return objVer;
		}

		/// <summary>
		/// Returns the id of the state from the property values or null, if not found.
		/// </summary>
		public static int? TryGetState( PropertyValues propertyValues )
		{
			// Get the state property index.
			int propertyIndex = propertyValues.IndexOf( ( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefState );

			// If there is no state, we are not entering a smartcard state.
			if( propertyIndex == -1 )
				return null;

			// Get the state property value.
			MFilesAPI.PropertyValue propertyValue = propertyValues[ propertyIndex ];

			// If there is no state, we are not entering a smartcard state.
			if( propertyValue.Value.IsNULL() || propertyValue.Value.IsUninitialized() )
				return null;

			// Get state id.
			return propertyValue.Value.GetLookupID();
		}

		/// <summary>
		/// Returns the id of the state transition from the property values or null, if not found.
		/// </summary>
		public static int? TryGetStateTransition( PropertyValues propertyValues )
		{
			// Get the state property index.
			int propertyIndex = propertyValues.IndexOf( ( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefStateTransition );

			// If there is no state, we are not entering a smartcard state.
			if( propertyIndex == -1 )
				return null;

			// Get the state property value.
			MFilesAPI.PropertyValue propertyValue = propertyValues[ propertyIndex ];

			// If there is no state, we are not entering a smartcard state.
			if( propertyValue.Value.IsNULL() || propertyValue.Value.IsUninitialized() )
				return null;

			// Get state id.
			return propertyValue.Value.GetLookupID();
		}

		/// <summary>
		/// Returns the id of the worklow from the property values or null, if not found.
		/// </summary>
		public static int? TryGetWorkflow( PropertyValues propertyValues )
		{
			// Get the state property index.
			int propertyIndex = propertyValues.IndexOf( ( int )MFBuiltInPropertyDef.MFBuiltInPropertyDefWorkflow );

			// If there is no wf, we are not entering a smartcard state.
			if( propertyIndex == -1 )
				return null;

			// Get the wf property value.
			MFilesAPI.PropertyValue propertyValue = propertyValues[ propertyIndex ];

			// If there is no wf, we are not entering a smartcard state.
			if( propertyValue.Value.IsNULL() || propertyValue.Value.IsUninitialized() )
				return null;

			// Get wf id.
			return propertyValue.Value.GetLookupID();
		}
	}
}
