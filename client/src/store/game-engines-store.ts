import { action, computed, makeObservable } from 'mobx'
import { group, text, bool, file, rule } from './forms'
import { Operation } from './connection'
import { newDialog } from './dialog'

export interface NewGameEngineConfirmationCallback {
    (name: string, description: string, content: File, ruleset: File): Promise<any>
}

function newForm() {
    const name = text(rule('max:128'), { isRequired: true })
    const description = text(rule('max:1024'))
    const remote = bool(null, { initialValue: false, isRequired: true })

    const disabledWhenRemote = () => remote.value
    const enabledWhenRemote = () => !remote.value

    const files = group(
        {
            engine: file(null, { isRequired: true }),
            ruleset: file(null, { isRequired: true })
        },
        {
            isDisabled: disabledWhenRemote
        }
    )

    const remoteOptions = group({
        url: text(rule('max:1024'), { isRequired: true }),
    }, { isDisabled: enabledWhenRemote })

    return group({ name, description, remote, files, remoteOptions })
}

export class NewGameEngineViewModel {
    constructor(public readonly operation: Operation, private readonly onConfirm: NewGameEngineConfirmationCallback) {
        makeObservable(this, {
            mode: computed,
            setMode: action,
            confirm: action
        })
    }

    private canClose() {
        return !this.operation.isLoading
    }

    readonly form = newForm()
    readonly dialog = newDialog({
        onOpen: () => {
            this.form.reset()
            this.form.remote.reset(false)
        },
        onClose: this.canClose.bind(this),
        onAutoClose: this.canClose.bind(this)
    })

    get mode(): 'local' | 'remote' {
        return this.form.remote.value ? 'remote' : 'local'
    }

    setMode = (e, value) => {
        this.form.remote.handleChange(value === 'remote')
    }

    readonly confirm = async () => {
        this.form.touch()
        if (this.form.isValid) {
            const { name, description, files } = this.form.value
            await this.onConfirm(name, description, files.engine, files.ruleset)
        }
    }
}
