import { action, makeObservable, observable } from 'mobx'
import { group, text, bool, file, rule } from './forms'

export interface NewGameEngineConfirmationCallback {
    (name: string, description: string, content: File, ruleset: File): Promise<any>
}

function newForm() {
    const name = text(rule('max:128'), { isRequired: true })
    const description = text(rule('max:1024'))
    const remote = bool(null, { initialValue: false, isRequired: true })

    const disabledWhenRemote = () => remote.value

    const files = group(
        {
            engine: file(null, { isRequired: true }),
            ruleset: file(null, { isRequired: true })
        },
        {
            isDisabled: disabledWhenRemote
        }
    )

    return group({ name, description, remote, files })
}

type Form = ReturnType<typeof newForm>

export class NewGameEngineViewModel {
    constructor(private readonly onConfirm: NewGameEngineConfirmationCallback) {
        makeObservable(this)
    }

    readonly form: Form = newForm()

    @observable isOpen = false
    @observable inProgress = false
    @observable error = ''

    @action readonly open = () => {
        this.isOpen = true

        this.inProgress = false
        this.error = ''

        this.form.reset()
        this.form.remote.reset(false)
    }

    @action readonly close = () => {
        this.isOpen = false
    }

    readonly autoClose = () => {
        if (this.inProgress) {
            return
        }

        this.close()
    }

    @action readonly begin = () => {
        this.inProgress = true
    }

    @action readonly end = () => {
        this.inProgress = false
    }

    @action readonly setError = (message: string) => this.error = message

    @action readonly confirm = async () => {
        this.form.touch()
        // await this.onConfirm(this.name, this.description, this.content.file, this.ruleset.file)
    }
}
