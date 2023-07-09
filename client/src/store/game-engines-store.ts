import { action, computed, makeObservable, observable } from 'mobx'
import { GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables } from '../schema'
import { seq, mutate } from './connection'

export class GameEnginesStore {
    constructor() {
        // makeObservable(this)
    }

    readonly engines = seq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [], null, 'engines')

    readonly newEngine = new NewGameEngineStore(this)

    beginNewEngine = () => this.newEngine.open()

    addEngine(name: string, content: File, ruleset: File) {
        mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables>(GameEngineCreate, { name, content, ruleset }, {
            refetch: [ this.engines ]
        })
    }
}

export class FileViewModel {
    constructor() {
        makeObservable(this)
    }

    file: File

    @observable name: string = ''
    @observable size: number = 0

    @computed get isEmpty() {
        return this.size === 0
    }

    @action set = (files: FileList) => {
        this.file = files[0]
        this.name = this.file.name
        this.size = this.file.size
    }

    @action clear = () => {
        this.file = null
        this.name = ''
        this.size = 0
    }
}

export class NewGameEngineStore {
    constructor(private readonly store: GameEnginesStore) {
        makeObservable(this)
    }

    @observable isOpen = false
    @observable name = ''

    readonly content = new FileViewModel()
    readonly ruleset = new FileViewModel()

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

        // FIXME
        // this.store.addEngine(this.name, this.file)
    }

    @action setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }
}
