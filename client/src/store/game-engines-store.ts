import { action, makeObservable, observable, runInAction } from 'mobx'
import { CLIENT } from '../client'
import { GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery, GetGameEnginesQueryVariables } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables } from '../schema'

export class GameEnginesStore {
    constructor() {
        // makeObservable(this)
    }

    readonly engines = observable<GameEngineFragment>([])

    readonly newEngine = new NewGameEngineStore(this)

    load = () => {
        CLIENT.query<GetGameEnginesQuery, GetGameEnginesQueryVariables>({
            query: GetGameEngines
        }).then(response => {
            runInAction(() => {
                this.engines.replace(response.data.gameEngines.items)
            });
        });
    }

    beginNewEngine = () => this.newEngine.open()

    addEngine(name: string, file: File) {
        CLIENT.mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables>({
            mutation: GameEngineCreate,
            variables: { name, file }
        }).then(result => {
            const { engine } = result.data.gameEngineCreate
            runInAction(() => this.engines.push(engine))
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

    @action setFile = (event: React.ChangeEvent<HTMLInputElement>) => {
        console.log('set file')

        const { files } = event.target
        if (!files.length) {
            this.file = null
            this.fileName = ''

            return
        }

        this.file = files[0]
        this.fileName = this.file.name
    }

    @action setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }
}
