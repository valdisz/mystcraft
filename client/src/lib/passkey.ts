/**
 * Check if the browser supports the WebAuthn API.
 */
export function canUsePasskey(): Promise<boolean> {
    // FIXME: This breaks production build, fix it later
    // return window.PublicKeyCredential && PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable
    //     ? PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable()
    //     : Promise.resolve(false)
    return Promise.resolve(false)
}


export function canUseConditionalMediation(): Promise<boolean> {
    // FIXME: This breaks production build, fix it later
    // return window.PublicKeyCredential && PublicKeyCredential.isConditionalMediationAvailable
    //     ? PublicKeyCredential.isConditionalMediationAvailable()
    //     : Promise.resolve(false)
    return Promise.resolve(false)
}
