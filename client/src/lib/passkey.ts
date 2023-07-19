/**
 * Check if the browser supports the WebAuthn API.
 */
export function canUsePasskey(): Promise<boolean> {
    return new Promise<boolean>((resolve, reject) => {
        if (window.PublicKeyCredential &&
            PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable &&
            PublicKeyCredential.isConditionalMediationAvailable) {

            // Check if user verifying platform authenticator is available.
            Promise.all([
                PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable(),
                PublicKeyCredential.isConditionalMediationAvailable(),
            ])
                .then(
                    results => resolve(results.every(r => r === true)),
                    reject
                );
        }

        resolve(false)
    })
}
