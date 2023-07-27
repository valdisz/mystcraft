import { action, computed, makeObservable, observable } from 'mobx'

export interface DialogEvent {
    (): boolean | null | undefined | void
}

export interface DialogOptions {
    onOpen?: DialogEvent
    onClose?: DialogEvent
    onAutoClose?: DialogEvent
}


export class DialogViewModel {
    constructor(options?: DialogOptions) {
        this._onOpen = options?.onOpen ?? (() => true)
        this._onClose = options?.onClose ?? (() => true)
        this._onAutoClose = options?.onAutoClose ?? (() => true)

        makeObservable(this, {
            isOpen: observable,
            isClosed: computed,
            open: action,
            close: action
        })
    }

    private readonly _onOpen: DialogEvent
    private readonly _onClose: DialogEvent
    private readonly _onAutoClose: DialogEvent

    isOpen = false

    get isClosed() {
        return !this.isOpen
    }

    readonly open = () => {
        if (this._onOpen() === false) {
            return
        }

        this.isOpen = true
    }

    readonly close = () => {
        if (this._onClose() === false) {
            return
        }
        this.isOpen = false
    }

    readonly autoClose = () => {
        if (this._onAutoClose() === false) {
            return
        }

        this.close()
    }
}

export function newDialog(options?: DialogOptions) {
    return new DialogViewModel(options)
}
