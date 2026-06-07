window.lensmaniaGoogleAuth = {
    initialize: function (clientId, buttonElementId, dotNetRef) {
        if (typeof google === 'undefined' || !google.accounts) {
            console.error('Google Identity Services not loaded.');
            return;
        }
        google.accounts.id.initialize({
            client_id: clientId,
            callback: function (response) {
                dotNetRef.invokeMethodAsync('OnGoogleCredential', response.credential);
            }
        });
        const element = document.getElementById(buttonElementId);
        if (element) {
            google.accounts.id.renderButton(element, {
                theme: 'outline',
                size: 'large',
                text: 'continue_with',
                width: 280
            });
        }
    }
};
