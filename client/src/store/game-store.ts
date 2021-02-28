import { ApolloQueryResult } from 'apollo-client'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { RegionFragment, TurnSummaryFragment } from '../schema'
import { GetSingleGame, GetSingleGameQuery, GetSingleGameQueryVariables } from '../schema'
import { GetRegions, GetRegionsQuery, GetRegionsQueryVariables } from '../schema'

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

    @observable turn: TurnSummaryFragment
    regions: RegionFragment[] = []

    async load(gameId: string) {
        runInAction(() => this.loading = true)

        const response = await CLIENT.query<GetSingleGameQuery, GetSingleGameQueryVariables>({
            query: GetSingleGame,
            variables: {
                gameId
            }
        })

        const { turns, ...game } = response.data.node
        runInAction(() => {
            Object.assign(this, game);
            this.turns.replace(turns)
            this.turn = turns.find(x => x.number == game.lastTurnNumber)
        })

        // find latest turn
        let latestTurn: TurnSummaryFragment
        for (const turn of turns) {
            if (!latestTurn) {
                latestTurn = turn
                continue
            }

            if (turn.number > latestTurn.number) {
                latestTurn = turn
            }
        }

        let cursor: string = null
        let regions: ApolloQueryResult<GetRegionsQuery> = null
        this.regions.length = 0
        do {
            regions = await CLIENT.query<GetRegionsQuery, GetRegionsQueryVariables>({
                query: GetRegions,
                variables: {
                    cursor,
                    turnId: latestTurn.id
                }
            })

            for (const { node } of regions.data.node.regions.edges) {
                this.regions.push(node)
            }

            cursor = regions.data.node.regions.pageInfo.endCursor
        }
        while (regions.data.node.regions.pageInfo.hasNextPage)

        setTimeout(() => runInAction(() => this.loading = false))
    }

    @observable selctedRegion: RegionFragment = null

    @action selectRegion = (col: number, row: number) => {
        console.log('selectRegion', col, row)
        const reg = this.regions.find(x => x.x === col && x.y === row && x.z === 1)
        this.selctedRegion = reg ? observable(reg) : null
        console.log('selectRegion', reg, this.selctedRegion)
    }

    @computed get units() {
        const units = this.selctedRegion?.units ?? []
        return units
    }

    @computed get structures() {
        const structures = this.selctedRegion?.structures ?? []
        return structures
    }
}
