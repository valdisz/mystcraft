/**
 * Check if the browser supports the WebAuthn API.
 */
export function canUsePasskey(): Promise<boolean> {
    return window.PublicKeyCredential && PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable
        ? PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable()
        : Promise.resolve(false)
}


export function canUseConditionalMediation(): Promise<boolean> {
    return window.PublicKeyCredential && PublicKeyCredential.isConditionalMediationAvailable
        ? PublicKeyCredential.isConditionalMediationAvailable()
        : Promise.resolve(false)
}
