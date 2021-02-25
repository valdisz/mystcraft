import { makeObservable, observable, IObservableArray, runInAction } from 'mobx'
import { CLIENT } from '../client'
import { GetSingleGame, GetSingleGameQuery, GetSingleGameQueryVariables, TurnSummaryFragment } from '../schema'

export class TurnsStore {
    constructor() {
        makeObservable(this)
    }
}

export class GameStore {
    constructor() {
        makeObservable(this)
    }

    @observable loading = true

    @observable lastTurnNumber: number
    @observable name: string
    @observable password: string
    @observable playerFactionName: string
    @observable playerFactionNumber: number
    turns: IObservableArray<TurnSummaryFragment> = observable([])
    @observable rulesetName: string
    @observable rulesetVersion: string

    async load(gameId: string) {
        runInAction(() => this.loading = true)

        const response = await CLIENT.query<GetSingleGameQuery, GetSingleGameQueryVariables>({
            query: GetSingleGame,
            variables: {
                gameId
            }
        })

        runInAction(() => {
            const { turns, ...game } = response.data.node
            Object.assign(this, game);
            this.turns.replace(turns)
        })

        setTimeout(() => runInAction(() => this.loading = false))
    }
}
