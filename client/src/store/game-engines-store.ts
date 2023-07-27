import { action, computed, makeObservable } from 'mobx'
import { group, text, bool, file, rule } from './forms'
import { Operation, Runnable } from './connection'
import { DialogViewModel, newDialog } from './dialog'
import { GameEngineCreateLocalMutationVariables, GameEngineCreateRemoteMutationVariables } from '../schema'

function newForm() {
    const name = text(rule('max:128'), { isRequired: true })
    const description = text(rule('max:1024'))
    const isRemote = bool(null, { initialValue: false, isRequired: true })

    const disabledWhenRemote = () => isRemote.value
    const enabledWhenRemote = () => !isRemote.value

    const files = group(
        {
            engine: file(null, { isRequired: true }),
            ruleset: file(null, { isRequired: true })
        },
        {
            isDisabled: disabledWhenRemote
        }
    )

    const remote = group({
        url: text(rule('max:1024|url'), { isRequired: true }),
    }, { isDisabled: enabledWhenRemote })

    return group({ name, description, isRemote, files, remote })
}

export class NewGameEngineViewModel {
    constructor(
        public readonly operation: Operation,
        private readonly addLocal: Runnable<GameEngineCreateLocalMutationVariables>,
        private readonly addRemote: Runnable<GameEngineCreateRemoteMutationVariables>,
    ) {
        this.dialog = newDialog({
            onOpen: () => {
                this.operation.reset()
                this.form.reset()
                this.form.isRemote.reset(false)
            },
            operation: this.operation
        })

        makeObservable(this, {
            mode: computed,
            setMode: action,
            confirm: action
        })
    }

    readonly dialog: DialogViewModel
    readonly form = newForm()

    get mode(): 'local' | 'remote' {
        return this.form.isRemote.value ? 'remote' : 'local'
    }

    setMode = (e, value) => {
        this.form.isRemote.handleChange(value === 'remote')
    }

    readonly confirm = async () => {
        this.form.touch()
        if (this.form.isInvalid) {
            return
        }

        const { files, remote, isRemote, ...variables } = this.form.value
        if (isRemote) {
            await this.addRemote.run({ ...variables, ...remote, api: 'no', options: '' })
        }
        else {
            await this.addLocal.run({ ...variables, ...files })
        }
    }
}
