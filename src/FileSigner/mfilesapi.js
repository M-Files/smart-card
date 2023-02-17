
"use strict";

// Prepare top-level namespace.
var mfilesapi = mfilesapi || new function() {}

/**
* Gets the vault object from the given shell frame.
*/
mfilesapi.getVault = function( uiElement ) {

	// Verify shellFrame.
	if( !uiElement )
		MFiles.ThrowError( "Argument is null." );
		
	// Does the UI element have the vault object?
	if( uiElement.Vault )
		return uiElement.Vault;
		
	// Does the UI element defined ShellUI. ShellFrame exposes the vault object via the ShellUI property.		
	if( uiElement.ShellUI )
		return uiElement.ShellUI.Vault;
	
	// Unable to resolve the vault object.
	MFiles.ThrowError( "Unable to access Vault." );	
}

/**
* Gets the handle of the parent window.
*/
mfilesapi.getWindowHandle = function( uiElement ) {

	// Verify shellFrame.
	if( !uiElement )
		MFiles.ThrowError( "Argument is null." );
		
	// Does the UI element have the vault object?
	if( uiElement.Vault )
		return uiElement.Vault;
		
	// Does the UI element defined ShellUI. ShellFrame exposes the vault object via the ShellUI property.		
	if( uiElement.ShellUI )
		return uiElement.ShellUI.Vault;
	
	// Unable to resolve the vault object.
	MFiles.ThrowError( "Unable to access Vault." );	
}

/**
* Gets M-Files version.
*/
mfilesapi.getMFilesVersion = function() {

	var client = new MFiles.MFilesClientApplication;
	var version = client.GetClientVersion();
	var versionForDisplay = version.Display;
	return versionForDisplay;
}