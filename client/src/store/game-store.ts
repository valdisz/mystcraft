import { ApolloQueryResult } from 'apollo-client'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { TurnSummaryFragment } from '../schema'
import { GetSingleGame, GetSingleGameQuery, GetSingleGameQueryVariables } from '../schema'
import { GetRegions, GetRegionsQuery, GetRegionsQueryVariables } from '../schema'
import { GetTurnDetails, GetTurnDetailsQuery, GetTurnDetailsQueryVariables } from '../schema'
import { Ruleset } from "./game/ruleset"
import { Region } from "./game/region"
import { World } from "./game/world"
import { WorldInfo, WorldLevel } from './game/world-info'
import { Unit } from './game/types'
import { saveAs } from 'file-saver'

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

    @observable world: World
    gameId: string = null

    async load(gameId: string) {
        if (this.gameId === gameId) {
            return
        }
        this.gameId = gameId

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

        game.options.map.sort((a, b) => a.level - b.level)
        const map: WorldLevel[] = game.options.map.map(level => ({
            label: level.label,
            width: level.width,
            height: level.height
        }))
        const worldInfo: WorldInfo = { map }

        const ruleset = new Ruleset()
        ruleset.load(game.ruleset)

        const world = new World(worldInfo, ruleset)

        const turnDetails = await CLIENT.query<GetTurnDetailsQuery, GetTurnDetailsQueryVariables>({
            query: GetTurnDetails,
            variables: {
                turnId: latestTurn.id
            }
        })

        for (const faction of turnDetails.data.node.factions) {
            world.addFaction(faction.number, faction.name, faction.number === this.factionNumber)
        }

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

            world.addRegions(regions.data.node.regions.edges.map(x => x.node))

            cursor = regions.data.node.regions.pageInfo.endCursor
        }
        while (regions.data.node.regions.pageInfo.hasNextPage)

        runInAction(() => this.world = world)
        setTimeout(() => runInAction(() => this.loading = false))
    }

    @observable region: Region = null

    @action selectRegion = (col: number, row: number) => {
        const reg = this.world.getRegion(col, row, 1)

        if (reg.id !== this.region?.id) {
            this.region = reg ? observable(reg) : null
            this.unit = null
        }

        if (reg) {
            window.localStorage.setItem('coords', JSON.stringify(reg.coords))
        }
    }

    @computed get units() {
        const units = this.region?.units ?? []
        return units
    }

    @computed get structures() {
        const structures = this.region?.structures ?? []
        return structures
    }

    @observable unit: Unit = null
    @action selectUnit = (unit: Unit) => this.unit = unit


    @observable battleSimOpen = false
    readonly attackers: IObservableArray<Unit> = observable([])
    readonly defenders: IObservableArray<Unit> = observable([])

    @action addAttacker = (unit: Unit) => this.attackers.push(unit)
    @action addDefender = (unit: Unit) => this.defenders.push(unit)
    @action resetBattleSim = () => {
        this.attackers.clear()
        this.defenders.clear()
    }

    @action openBattleSim = () => {
        this.resetBattleSim()
        this.battleSimOpen = true
    }

    @action closeBattleSim = () => {
        this.resetBattleSim()
        this.battleSimOpen = false
    }

    isAttacker = (unit: Unit) => this.attackers.some(x => x.id === unit.id)
    isDefender = (unit: Unit) => this.defenders.some(x => x.id === unit.id)

    toBattleSim = () => {
        function toBattleSimUnit(unit: Unit) {
            const u: BattleSimUnit = {
                name: `${unit.name} - ${unit.num}`,
                skills: unit.skills.all.map(s => ({
                    abbr: s.code,
                    level: s.level
                })),
                items: unit.inventory.items.all
                    .map(i => ({
                        abbr: i.code,
                        amount: i.amount
                    })),
                combatSpell: unit.combatSpell?.code,
                flags: unit.flags.filter(x => x === 'behind') as any[]
            }

            if (!u.skills.length) delete u.skills
            if (!u.items.length) delete u.items
            if (!u.flags.length) delete u.flags
            if (!u.combatSpell) delete u.combatSpell

            return u
        }

        const sim: BattleSim = {
            attackers: {
                units: this.attackers.map(toBattleSimUnit)
            },
            defenders: {
                units: this.defenders.map(toBattleSimUnit)
            }
        }

        const json = JSON.stringify(sim)

        const blob = new Blob([json], { type: 'application/json' })
        saveAs(blob, 'battlesim.json')
    }
}

export interface BattleSimItem {
    abbr: string;
    amount: number;
}

export interface BattleSimUnit {
    name: string;
    items: BattleSimItem[];
    skills: BattleSimSkill[];
    flags: ('behind')[];
    combatSpell: string;
}

export interface BattleSimSkill {
    abbr: string;
    level: number;
}

export interface BattleSimUnitList {
    units: BattleSimUnit[]
}

export interface BattleSim {
    attackers: BattleSimUnitList
    defenders: BattleSimUnitList
}
