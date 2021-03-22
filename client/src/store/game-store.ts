import { ApolloQueryResult } from 'apollo-client'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { TurnSummaryFragment } from '../schema'
import { GetSingleGame, GetSingleGameQuery, GetSingleGameQueryVariables } from '../schema'
import { GetRegions, GetRegionsQuery, GetRegionsQueryVariables } from '../schema'
import { Ruleset } from "./game/ruleset"
import { Region } from "./game/region"
import { World } from "./game/world"

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

    @observable name: string
    @observable rulesetName: string
    @observable rulesetVersion: string

    @observable lastTurnNumber: number
    @observable factionName: string
    @observable factionNumber: number

    turns: IObservableArray<TurnSummaryFragment> = observable([])
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

        const { myPlayer, ...game } = response.data.node
        const { turns, lastTurnNumber } = myPlayer

        runInAction(() => {
            this.name = game.name
            this.rulesetName = game.rulesetName
            this.rulesetVersion = game.rulesetVersion

            this.factionName = myPlayer.factionName
            this.factionNumber = myPlayer.factionNumber
            this.lastTurnNumber = lastTurnNumber

            this.turns.replace(turns)
            this.turn = turns.find(x => x.number == lastTurnNumber)
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

        this.world = new World({ levels: [
            {
                width: 96, height: 72, label: 'surface'
            }
        ] }, new Ruleset())

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

            this.world.addRegions(regions.data.node.regions.edges.map(x => x.node))

            cursor = regions.data.node.regions.pageInfo.endCursor
        }
        while (regions.data.node.regions.pageInfo.hasNextPage)

        setTimeout(() => runInAction(() => this.loading = false))
    }

    @observable region: Region = null

    @action selectRegion = (col: number, row: number) => {
        const reg = this.world.getRegion(col, row, 1)
        this.region = reg ? observable(reg) : null
        console.log(reg)
    }

    @computed get units() {
        const units = this.region?.units ?? []
        return units
    }

    @computed get structures() {
        const structures = this.region?.structures ?? []
        return structures
    }
}
