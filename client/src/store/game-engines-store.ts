import { action, makeObservable, observable } from 'mobx'
import { FILE, Field, FormField, STRING, makeGroup, rule } from './forms'

export interface NewGameEngineConfirmationCallback {
    (name: string, description: string, content: File, ruleset: File): Promise<any>
}

export interface NewGameEngineForm extends FormField {
    name: Field<string>
    description: Field<string>
    engine: Field<File>
    ruleset: Field<File>
}

export class NewGameEngineViewModel {
    constructor(private readonly onConfirm: NewGameEngineConfirmationCallback) {
        makeObservable(this)
    }

    readonly form: NewGameEngineForm = makeGroup({
        name: new Field<string>(STRING(true), '', true, rule('max:128')),
        description: new Field<string>(STRING(true), '', false, rule('max:1024')),
        engine: new Field<File>(FILE, null, true),
        ruleset: new Field<File>(FILE, null, true),
    })

    @observable isOpen = false
    @observable inProgress = false
    @observable error = ''

    @action readonly open = () => {
        this.isOpen = true

        this.inProgress = false
        this.error = ''
        this.form.reset()
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
