
"use strict";

// Prepare top-level namespace.
var mfilesapi = mfilesapi || new function() {}
 
// Define namespace for function which modify objects.
 mfilesapi.content = mfilesapi.content || new function mfilesapi_content() {

	/**
	* Gets object version object of the specified object.
	*/
	this.getObjectVersion = function( vault, objver ) {

		// Return the object version info.	
		var objectInfo = vault.ObjectOperations.GetObjectInfo( objver, false, false );
		return objectInfo;
	}

	/**
	* Gets the state of the previous version of the object.
	*/
	this.getStateIdFromServer = function( vault, objver ) {
		
		// Fetch the properties of the object and return the possible state id.
		var properties = vault.ObjectPropertyOperations.GetProperties( objver );
		var stateId = this.getStateId( properties );
		return stateId;
	}

	/**
	* Gets the id of the state from the given property value collection
	*/
	this.getStateId = function( properties ) {

		// Does the collection have the state property?
		var index = properties.IndexOf( 39 );  // State property def.
		if( index === -1 )
			return -1;	
			
		// Get the state id.
		var state = properties.SearchForProperty( 39 );
		var stateId = state.TypedValue.GetLookupID();
		return stateId;
	}

	/**
	* Gets the id of the state transition from the given property value collection
	*/
	this.getStateTransitionId = function( properties ) {

		// Does the collection have the state property?
		var index = properties.IndexOf( 99 );  // State transition property def.
		if( index === -1 )
			return -1;

		// Get the state id.
		var state = properties.SearchForProperty( 99 );
		var stateId = state.TypedValue.GetLookupID();
		return stateId;
	}

	/**
	* Creates a new object.
	*/
	this.createNewObject = function( vault, objectTypeId, classId, title, additionalPropertyValues ) {

		// Prepare the property values.
		var properties = new MFiles.PropertyValues;
		if( additionalPropertyValues ) {
		
			// Collect all the additional property values.
			for( var index = 0; index < additionalPropertyValues.Count; index++ ) {
				var additionalValue = additionalPropertyValues[ index ];
				properties.Add( -1, additionalValue );
			}
			
		}  // end if.
		
		// Specify class.
		properties.Add( -1, createPropertyValueFromLookupId( 100, 0 ) );
		
		// Title.
		properties.Add( -1, createPropertyValueFromText( 0, title ) );
		
		// Create the object.
		var sourceFiles = new MFiles.SourceObjectFiles;
		var defaultAcl = mfilesapi.structure.getDefaultAclForObjectsOfType( vault, objectTypeId );
		var createdObject = vault.ObjectOperations.CreateNewObjectEx( objectTypeId, properties, sourceFiles, false, true, defaultAcl );
		return createdObject;
	}
	
	/**
	* Checkouts an object.
	*/
	this.checkOut = function( vault, objid ) {
	
		// Checkout.
		var result = vault.ObjectOperations.CheckOut( objid );
		return result;
	}
	
	/**
	* Check in the object.
	*/
	this.checkIn = function( vault, objver ) {
	
		// Check in.
		var result = vault.ObjectOperations.CheckIn( objver );
		return result;
	}
	
	/**
	* Sets the specified property to an object.
	*/
	this.setProperty = function( vault, objver, propertyValue ) {
	
		// Set the property.
		var result = vault.ObjectPropertyOperations.SetProperty( objver, propertyValue );
		return result;
	}

	/**
	* Creates a new property value from the given text.
	*/
	this.createPropertyValueFromText = function( propertyDefId, text ) {

		// Create and return the property value.
		var pv = new MFiles.PropertyValue;
		pv.PropertyDef = propertyDefId;
		pv.TypedValue.SetValue( 1, text );  // MFDatatypeText.
		return pv;
	}

	/**
	* Creates a new property value from lookup id.
	*/
	this.createPropertyValueFromLookupId = function( propertyDefId, lookupId ) {

		// Create and return the property value.
		var pv = new MFiles.PropertyValue;
		pv.PropertyDef = propertyDefId;
		pv.TypedValue.SetValue( 9, lookupId );  // MFDatatypeLookup.
		return pv;
	}

	/**
	* Creates property value from the given lookup.
	*/
	this.createPropertyValueFromLookup = function( propertyDefId, lookup ) {

		// Create and return the property value.
		var pv = new MFiles.PropertyValue;
		pv.PropertyDef = propertyDefId;
		pv.TypedValue.SetValueToLookup( lookup );
		return pv;
	}

	/**
	* Creates a lookup object from the specified object version.
	*/
	this.createLookupFromObjVer = function( objver, versionSpecific ) {

		// Create the lookup.
		var lookup = new MFiles.Lookup;
		lookup.OBjectType = objver.Type;
		lookup.Item = objver.ID;
		lookup.Version = -1;
		if( versionSpecific )
			lookup.Version = objver.Version;
		return lookup;
	}

}