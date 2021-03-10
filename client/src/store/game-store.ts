import { ApolloQueryResult } from 'apollo-client'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { TurnSummaryFragment } from '../schema'
import { GetSingleGame, GetSingleGameQuery, GetSingleGameQueryVariables } from '../schema'
import { GetRegions, GetRegionsQuery, GetRegionsQueryVariables } from '../schema'
import { Region, Ruleset, World } from './game/types'

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

    world: World

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

        this.world = new World({ width: 56, height: 56 }, new Ruleset())

        let cursor: string = null
        let regions: ApolloQueryResult<GetRegionsQuery> = null
        do {
            regions = await CLIENT.query<GetRegionsQuery, GetRegionsQueryVariables>({
                query: GetRegions,
                variables: {
                    cursor,
                    turnId: latestTurn.id
                }
            })

            for (const { node } of regions.data.node.regions.edges) {
                this.world.addRegion(node)
            }

            cursor = regions.data.node.regions.pageInfo.endCursor
        }
        while (regions.data.node.regions.pageInfo.hasNextPage)

        setTimeout(() => runInAction(() => this.loading = false))
    }

    @observable selctedRegion: Region = null

    @action selectRegion = (col: number, row: number) => {
        const reg = this.world.getRegion(col, row, 1)
        this.selctedRegion = reg ? observable(reg) : null
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
