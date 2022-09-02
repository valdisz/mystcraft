export interface Disposable {
    dispose()
}

export class CallbackDisposable implements Disposable {
    constructor(private disposeCallback: () => void) {
    }

    private _disposed = false

    dispose() {
        if (this._disposed) {
            return
        }

        this.disposeCallback()
        this._disposed = true
    }
}
