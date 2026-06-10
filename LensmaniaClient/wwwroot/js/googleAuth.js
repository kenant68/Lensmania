window.lensmaniaGoogleAuth = {
    maxRetries: 50,
    retryDelayMs: 100,
    initialize: function (clientId, buttonElementId, dotNetRef, attempt) {
        attempt = attempt || 0;
        // The GIS script is loaded async, so it may not be ready when Blazor
        // first calls initialize(). Retry until it is, with a bounded count.
        if (typeof google === 'undefined' || !google.accounts) {
            if (attempt >= this.maxRetries) {
                console.error('Google Identity Services failed to load.');
                return;
            }
            const self = this;
            setTimeout(function () {
                self.initialize(clientId, buttonElementId, dotNetRef, attempt + 1);
            }, this.retryDelayMs);
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
