
"use strict";

function OnNewShellUI( shellUI ) {

	/// <summary>The entry point of ShellUI module.</summary>
	/// <param name="shellUI" type="MFiles.ShellUI">The new shell UI object.</param> 
    
    // Listen to set properties of object version event, that triggers when state transition is made from task pane.
	shellUI.Events.Register( Event_SetPropertiesOfObjectVersion, SetPropertiesOfObjectVersionHandler );
	
	// Convert the document to pdf with state transitions marked with a certain alias.
	function SetPropertiesOfObjectVersionHandler( setPropertiesParams, singlePropertyUpdate, singlePropertyRemove, window )
    {
        
        // Nothing to do if the module is not enabled.
        if( ! smartCard.isEnabled( shellUI.Vault ) ) 
            return;

        // Shortucts 
        var content = mfilesapi.content;
        var structure = mfilesapi.structure;
    
        // Do nothing on property removal.
        if( singlePropertyRemove )
            return;
            
        // Are we interested of the new state transition?
        var stateTransitionId = content.getStateTransitionId( setPropertiesParams.PropertyValuestoSet );
        var authenticationRequiredForTransition = false;
        if( smartCard.isAuthenticationRequiredForStateTransition( shellUI.Vault, stateTransitionId ) )
            authenticationRequiredForTransition = true;
        
        // Are we interested of the new state?
        var stateId = content.getStateId( setPropertiesParams.PropertyValuestoSet );
        var authenticationRequiredForState = false;
        if( smartCard.isAuthenticationRequiredForState( shellUI.Vault, stateId ) )
            authenticationRequiredForState = true;
                
        // Are we signing PDF in new state transition?
        var signPDFForTransition = false;
        if( smartCard.isSignPDFRequiredForStateTransition( shellUI.Vault, stateTransitionId ) )
            signPDFForTransition = true;

        // Authentication required?
        if( ! authenticationRequiredForTransition && ! authenticationRequiredForState && ! signPDFForTransition )
            return;
        
        // If state triggered the transition, verify that the state has actually changed.
        if( ! authenticationRequiredForTransition && ! signPDFForTransition ) {
            
            // The state is of interest.
            // Check if the state was actually changed.
            var oldStateId = content.getStateIdFromServer( shellUI.Vault, setPropertiesParams.ObjVer );
            if( stateId === oldStateId )
                return;
        }
        
        // Can we authenticate this user?
        if( ! smartCard.canAuthenticateUser( shellUI.Vault ) )
            MFiles.ThrowError( "SmartCard authentication required but not allowed." );

        if( signPDFForTransition && ( authenticationRequiredForTransition || authenticationRequiredForState ) )
            MFiles.ThrowError( "Cannot configure evidence PDF and sign PDF for same state or state transition." );
        
        // Create evidence PDF and sign that.
        if( authenticationRequiredForTransition || authenticationRequiredForState )
            
            // Check existance of MFCK extension method. Still could be possible that PDF processor is not configured.
            if( shellUI.Vault.ExtensionMethodOperations.DoesActiveVaultExtensionMethodExist( "MFiles.ComplianceKit.MFEventHandlerVaultExtensionMethod" ) )
                smartCard.signObject( window, shellUI, setPropertiesParams, true );
             else
                MFiles.ThrowError( "Cannot create evidence PDF. M-Files Compliance Kit is not found." );
                
        else if( signPDFForTransition )
        {
            // Sign PDF.
            smartCard.signObject( window, shellUI, setPropertiesParams, false );
            
            // Close the state transition window.
            window.Close();
                
            // Prevent the state transition call from going to the server.
            MFiles.ThrowError( "State transition successful. The PDF was signed." );
        }
         
    }  // end SetPropertiesOfObjectVersionHandler
    
}  // end OnNewShellUI

