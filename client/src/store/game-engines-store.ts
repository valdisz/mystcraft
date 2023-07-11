import { action, makeObservable, observable } from 'mobx'
import { GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables } from '../schema'
import { GameEngineDelete, GameEngineDeleteMutation, GameEngineDeleteMutationVariables } from '../schema'
import { seq, mutate } from './connection'
import { FileViewModel } from '../components'

export class GameEnginesStore {
    readonly engines = seq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [], null, 'engines')

    readonly newEngine = new NewGameEngineStore(this.add.bind(this))

    async add(name: string, content: File, ruleset: File) {
        this.newEngine.begin()

        try {
            var result = await mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables>(GameEngineCreate, { name, content, ruleset }, {
                refetch: [this.engines]
            })

            if (result.error) {
                this.newEngine.setError(result.error.message)
            }
            else if (!result.data.gameEngineCreate.isSuccess) {
                this.newEngine.setError(result.data.gameEngineCreate.error)
            }
            else {
                this.newEngine.close()
            }
        }
        catch (error) {
            this.newEngine.setError(error.toString())
        }
        finally {
            this.newEngine.end()
        }
    }

    delete(gameEngineId: string) {
        return mutate<GameEngineDeleteMutation, GameEngineDeleteMutationVariables>(GameEngineDelete, { gameEngineId }, { refetch: [ this.engines ] })
    }
}

export interface NewGameEngineConfirmationCallback {
    (name: string, content: File, ruleset: File): Promise<any>
}

export class NewGameEngineStore {
    constructor(private readonly onConfirm: NewGameEngineConfirmationCallback) {
        makeObservable(this)
    }

    @observable name = ''
    @observable isOpen = false
    @observable inProgress = false
    @observable error = ''

    readonly content = new FileViewModel()
    readonly ruleset = new FileViewModel()

    @action readonly setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
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
        await this.onConfirm(this.name, this.content.file, this.ruleset.file)
    }
}
