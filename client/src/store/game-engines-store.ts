import { action, makeObservable, observable } from 'mobx'
import { GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables } from '../schema'
import { seq, mutate } from './connection'
import { FileViewModel } from '../components'

export class GameEnginesStore {
    readonly engines = seq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [], null, 'engines')

    readonly newEngine = new NewGameEngineStore(this.addEngine.bind(this))

    addEngine(name: string, content: File, ruleset: File) {
        return mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables>(GameEngineCreate, { name, content, ruleset }, {
            refetch: [ this.engines ]
        })
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

    readonly content = new FileViewModel()
    readonly ruleset = new FileViewModel()

    @action readonly setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }

    @action readonly open = () => {
        this.isOpen = true

        this.name = ''
        this.content.clear()
        this.ruleset.clear()
    }

    @action readonly close = () => {
        this.isOpen = false
    }

    readonly confirm = async () => {
        await this.onConfirm(this.name, this.content.file, this.ruleset.file)
        this.close()
    }
}
