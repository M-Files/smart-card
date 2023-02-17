
"use strict";

// Prepare configuration namespace.
var smartCard = smartCard || new function() {	
		
	// Gets the configuration.
	var getConfiguration = function( vault ) {
	
		// Do we need to fetch the configuration from the vault?
		if( ! _configuration && ! _configurationSet ) 
			loadConfiguration( vault );
		else if( ! _configuration && _configurationSet )
			this.prepareConfiguration( vault );

		return _configuration;
	}
	
	/**
	* Is this module enabled?
	*/
	this.isEnabled = function( vault ) {
	
		// Load resources.
		loadResources();
	
		// Get the configuration.
		var configuration = getConfiguration( vault );
		if( configuration )
			return configuration.Enabled;
		else
			return false;
	}
	
	/**
	* Gets this module's resources.
	*/
	this.getResources = function() {
	
		// Load resources.
		loadResources();
		
		// Return reference to the resources.
		return _resources;
	}
	
	/**
	* Checks if an authentication is required for the specified state.
	*/
	this.isAuthenticationRequiredForState = function( vault, stateId ) {
	
		// Must be enabled.
		if( ! this.isEnabled( vault ) )
			MFiles.ThrowError( _resources.NotEnabled );
	
		// Is the authentication configured at all?
		var configuration = getConfiguration( vault );
		var authentication = configuration.Authentication;
		if( ! authentication || ! authentication.RequiredForStates )
			return false
			
		// Iterate over the states and try to find the one we are looking for.
		var requiredForStates = authentication.RequiredForStates;
		for( var rs = 0; rs < requiredForStates.length; rs++ ) {
		
			// Is the current required state the one we are looking for?
			var requiredForState = requiredForStates[ rs ];
			if( requiredForState === stateId ) {
				return true;
			}
		}
		return false;
	};
	
	/**
	* Checks if an authentication is required for the specified state transition.
	*/
	this.isAuthenticationRequiredForStateTransition = function( vault, stateTransitionId ) {

		// Must be enabled.
		if( !this.isEnabled( vault ) )
			MFiles.ThrowError( _resources.NotEnabled );

		// Is the authentication configured at all?
		var configuration = getConfiguration( vault );
 		var authentication = configuration.Authentication;
		if( !authentication || !authentication.RequiredForStateTransitions )
			return false
 
        // Iterate over the state transitions and try to find the one we are looking for.
		var requiredForTransitions = authentication.RequiredForStateTransitions;
		for( var rs = 0; rs < requiredForTransitions.length; rs++ ) {

			// Is the current required state transition the one we are looking for?
			var requiredForTransition = requiredForTransitions[ rs ];
			if( requiredForTransition === stateTransitionId ) {
				return true;
			}
		}
		return false;
	};
    
    /**
	* Checks if signing PDF is required for the specified state transition.
	*/
	this.isSignPDFRequiredForStateTransition = function( vault, stateTransitionId ) {

		// Must be enabled.
		if( !this.isEnabled( vault ) )
			MFiles.ThrowError( _resources.NotEnabled );

		// Is the authentication configured at all?
		var configuration = getConfiguration( vault );
		var authentication = configuration.Authentication;
		if( !authentication || !authentication.SignPDFForStateTransitions )
			return false

		// Iterate over the state transitions and try to find the one we are looking for.
		var requiredForTransitions = authentication.SignPDFForStateTransitions;
		for( var rs = 0; rs < requiredForTransitions.length; rs++ ) {

			// Is the current required state transition the one we are looking for?
			var requiredForTransition = requiredForTransitions[ rs ];
			if( requiredForTransition === stateTransitionId ) {
				return true;
			}
		}
		return false;
	};

	/**
	* Checks if we can authenticate the specified user.
	*/
	this.canAuthenticateUser = function( vault ) {
	
		// Must be enabled.
		if( ! this.isEnabled( vault ) )
			MFiles.ThrowError( _resources.NotEnabled );
	
		// Get the authentication configuration section.
		var configuration = getConfiguration( vault );
		var authentication = configuration.Authentication;
		if( ! authentication )
			return false;

		// Evaluate the user's authentication type against the allowed authentication types.
		var userAuthenticateAs = vault.SessionInfo.AuthenticationType;
		if( authentication.AllowedAuthenticationTypes && authentication.AllowedAuthenticationTypes.length > 0 ) {

			// Iterate over tha allowed authentication types.
			for( var i = 0; i < authentication.AllowedAuthenticationTypes.length; i++ ) {

				// Is the current authentication type user's authentication type.
				var at = authentication.AllowedAuthenticationTypes[i];
				if( at === userAuthenticateAs ) {
					return true;
				}

			}  // end for

			// The authentication type condition was not satisfied.
			return false;

		}  // end if

		// No allowed authentication method was specified.
		// Authentication is not allowed.
		return false;
	}
		
	/**
	* Prepares the configuration asynchronously if the vault is online.
	*/
	this.prepareConfiguration = function( vault ) {
	
		// Prepare only if online.
		if( vault.ClientOperations.IsOnline() ) {

			// Fetch configuration.
			vault.Async.ExtensionMethodOperations.ExecuteVaultExtensionMethod( "M-Files.SmartCard.GetConfigurationForClient", "",
				function( configuration ) {	setConfiguration( configuration ); },
				function ( shorterror, longerror, errorobj ) {

					// Failed. Report the error.
					MFiles.ReportException( errorobj );
				} );
		}
	};
	
	/** 
	 * Signs the specified object.
	 */
	this.signObject = function( window, uiElement, setPropertiesParams, createEvidencePDF, funcPrepare, funcSuccess, funcError ) {
	
		// Must be enabled.
		var vault = mfilesapi.getVault( uiElement );
		if( ! this.isEnabled( vault ) )
			MFiles.ThrowError( _resources.NotEnabled );

		// Sign the object.
		
		// Start the signing operation.
		if( funcPrepare )
			funcPrepare( uiElement, setPropertiesParams.ObjVer );
		var configuration = getConfiguration( vault );
		var objectInfo = vault.ObjectOperations.GetObjectInfo( setPropertiesParams.ObjVer, true, false );
		
		// Verify that the user saw the correct object version before signing.
		// When we fetch the latest object version here we might get a newer version from the server.
		// The user has not actually seen this object version so we must not perform the signing with it.
		// => Show an error.
		if( setPropertiesParams.ObjVer.Version !== objectInfo.ObjVer.Version ) {
		
			// Inform the user that there is a newer version on the server.
			MFiles.ThrowError( _resources.ErrorNewVersionAvailable );
		}
		
		// Sign.
		var errorMessage = signObjectPrivate( window, uiElement, vault, objectInfo, setPropertiesParams, createEvidencePDF );

		// Process possible error.
		if( errorMessage.description ) {

			// Error.
			if( funcError )
				funcError( uiElement, setPropertiesParams.ObjVer, errorMessage );

			// Throw exception.
			if( errorMessage.location ) {
				
				// Display the error augmented with the location if available.
				var fullMessage = errorMessage.description.concat( " (", String( errorMessage.location ), ")" );
				MFiles.ThrowError( fullMessage );
			}
			else {
				MFiles.ThrowError( errorMessage.description );
			}
		}
		else {

			// Success.
			if( funcSuccess )
				funcSuccess( uiElement, setPropertiesParams.ObjVer );				

		}  // end if
	}
	
	/** 
	 * Signs the specified object.
	 */
	var signObjectPrivate = function( window, uiElement, vault, objectInfo, setPropertiesParams, createEvidencePDF ) {

		// Prepare for signing the document.
		var latestObjectInfo;
		var mustCheckIn = false;
		var versionShownToTheUser = objectInfo.ObjVer.Version;
		var signed = false;
		var errorMessage = { 
				location: "",
				description: "",
				stack: ""
			};
		var configuration = getConfiguration();
		try {

			// Checkout the object if not checked out already.
			if( objectInfo.ThisVersionCheckedOut && objectInfo.ThisVersionLatestToThisUser ) {

				// The object must not be checked out
				MFiles.ThrowError( _resources.ErrorObjectCheckedOut );
			}
			else {
                // Check out the object.
                latestObjectInfo = vault.ObjectOperations.CheckOut( objectInfo.ObjVer.ObjID );
                mustCheckIn = true;
            }
		}
		catch( error ) {

			// Process error.
			errorMessage.location = "1";
			errorMessage.description = error.description;
		}

		// Sign the document.
		if( latestObjectInfo ) {

			try {
				
				// Verify that the user saw the correct object version before signing.
				// When we do checkout we always get the latest version of the object but the
				// version the user saw in the client could have been an old version if some other user 
				// had made changes to the object just prior to the checkout.
				// => Show an error.
				if( versionShownToTheUser !== latestObjectInfo.LatestCheckedInVersion ) {
				
					// Inform the user that there is a newer version on the server.
					MFiles.ThrowError( _resources.ErrorNewVersionAvailable );
				}

				// Sign the selected file.
				var launcher = MFiles.CreateObjectCLR( "managed/MFiles.PdfTools.UIExt.exe", "MFiles.PdfTools.UIExt.Launcher" );
				launcher.MFilesVersion = mfilesapi.getMFilesVersion();
				launcher.Locale = configuration.Locale;
				launcher.PdfTools = JSON.stringify( configuration.PdfTools );
				var accountName = vault.SessionInfo.AccountName;
				var errorCode = launcher.SignPDF( window.Handle, vault, latestObjectInfo, JSON.stringify( configuration.Authentication ), accountName, createEvidencePDF );
				if( errorCode == 0 )
					signed = true;
				else {
					errorMessage.description = launcher.ErrorMessage;
					errorMessage.stack = launcher.ErrorStack;
				}
			}
			catch( error ) {

				// Process error.
				errorMessage.location = "3";
				errorMessage.description = error.description;
			}
		}		

		// Check in the signed PDF.	
		try {

			// Check-in the object?
			if( mustCheckIn ) {

				// Check-in if the operation succeeded. Otherwise try undo checkout.
				if( signed ) {
                
                    // Do state transition when signing PDF
                    if( createEvidencePDF == false )
                    {
                        var sppomo = new MFiles.SetPropertiesParamsOfMultipleObjects();
                        var spp = setPropertiesParams.Clone();
                        spp.ObjVer = latestObjectInfo.ObjVer;
                        sppomo.Add( -1, spp );
                        vault.ObjectPropertyOperations.SetPropertiesOfMultipleObjects( sppomo );
                    }
                    
                    // Finalize object checkin.
                    latestObjectInfo = vault.ObjectOperations.CheckIn( latestObjectInfo.ObjVer );
				}
				else {
					latestObjectInfo = vault.ObjectOperations.UndoCheckout( latestObjectInfo.ObjVer );
				}

			}  // end if
		}
		catch( error ) {

			// Process error.
			errorMessage.location = "5";
			errorMessage.description = error.description;
			
		} // end try		

		// Return the possible error message.
		return errorMessage;
	}
	
	// Sets the configuration
	var setConfiguration = function( configuration ) {
	
		// Load resources.
		loadResources();
		
		// Is the configuration valid?
		if( configuration ) {
		
			// The configuration is valid.
			// The module is enabled if enabled in the configuration.
			_configuration = JSON.parse( configuration );
		}
		else {
		
			// Configuration is not valid. The module will be disabled.
			_configuration = null;
		}
		_configurationSet = true;
	}
	
	// Loads configuration from the server.
	var loadConfiguration = function( vault ) {
		// Fetch the configuration as we do not have it yet.
		var configuration = vault.ExtensionMethodOperations.ExecuteVaultExtensionMethod( "M-Files.SmartCard.GetConfigurationForClient", "" );
		setConfiguration( configuration );
	}
	
	// Loads resources.
	var loadResources = function() {
	
		// Load if not already loaded.
		if( ! _resources )
			_resources = MFiles.CreateObjectCLR( "managed/MFiles.PdfTools.UIExt.exe", "MFiles.PdfTools.UIExt.PublicResources" );	
	}
	
	// The configuration.
	var _configuration;
	var _configurationSet;
	var _resources;
};