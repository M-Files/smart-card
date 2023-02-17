
// Prepare top-level namespace.
var mfilesapi = mfilesapi || new function() {}

// Define namespace for function which modify objects.
 mfilesapi.structure = mfilesapi.structure || new function mfilesapi_structure() {

	/**
	* Gets the default property of the specified object type.
	*/
	this.getDefaultProperty = function( vault, objectTypeId ) {

		// Fetch the object type and get the default property.
		var ot = vault.ObjectTypeOperations.GetObjectType( objectTypeId );
		return ot.DefaultPropertyDef;
	}

	/**
	* Gets the default access control list for objects of the specified type.
	*/
	this.getDefaultAclForObjectsOfType = function( vault, objectTypeId ) {

	// Fetch the object type and get the default property.
		var ot = vault.ObjectTypeOperations.GetObjectType( objectTypeId );
		return ot.DefaultAccessControlList;
	}
}
