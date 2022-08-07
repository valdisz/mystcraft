import { action, makeObservable, observable, runInAction } from 'mobx'
import { CLIENT } from '../client'
import { GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery, GetGameEnginesQueryVariables } from '../schema'

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
}

export class NewGameEngineStore {
    constructor(private readonly store: GameEnginesStore) {
        makeObservable(this)
    }

    @observable isOpen = false
    @observable name = ''

    @action open = () => {
        this.isOpen = true
        this.name = ''
    }

    @action cancel = () => {
        this.isOpen = false
    }

    @action confirm = () => {
        this.isOpen = false
    }
}
