import { action, makeObservable, observable } from 'mobx'
import { FileViewModel } from '../components'

export interface NewGameEngineConfirmationCallback {
    (name: string, description: string, content: File, ruleset: File): Promise<any>
}

export class NewGameEngineViewModel {
    constructor(private readonly onConfirm: NewGameEngineConfirmationCallback) {
        makeObservable(this)
    }

    @observable name = ''
    @observable description = ''
    @observable isOpen = false
    @observable inProgress = false
    @observable error = ''

    readonly content = new FileViewModel()
    readonly ruleset = new FileViewModel()

    @action readonly setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }

    @action readonly setDescription = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.description = event.target.value
    }

    @action readonly open = () => {
        this.isOpen = true

        this.name = ''
        this.inProgress = false
        this.error = ''
        this.content.clear()
        this.ruleset.clear()
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

    readonly confirm = async () => {
        await this.onConfirm(this.name, this.description, this.content.file, this.ruleset.file)
    }
}
