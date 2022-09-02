import { action, makeObservable, observable } from 'mobx'
import { GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables } from '../schema'
import { querySeq, mutate } from './connection'

export class GameEnginesStore {
    constructor() {
        // makeObservable(this)
    }

    readonly engines = querySeq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [])

    readonly newEngine = new NewGameEngineStore(this)

    beginNewEngine = () => this.newEngine.open()

    addEngine(name: string, file: File) {
        mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables>(GameEngineCreate, { name, file }, {
            refetch: [ this.engines ]
        })
    }
}

export class NewGameEngineStore {
    constructor(private readonly store: GameEnginesStore) {
        makeObservable(this)
    }

    @observable isOpen = false
    @observable name = ''

    file: File
    @observable fileName = ''

    @action open = () => {
        this.isOpen = true

        this.name = ''
        this.file = null
        this.fileName = ''
    }

    @action cancel = () => {
        this.isOpen = false
    }

    @action confirm = () => {
        this.isOpen = false

        this.store.addEngine(this.name, this.file)
    }

    @action setFile = (files: FileList) => {
        this.file = files[0]
        this.fileName = this.file.name
    }

    @action setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }
}
