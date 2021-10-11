import { ApolloQueryResult } from 'apollo-client'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { RegionFragment, StructureFragment, TurnDetailsFragment, TurnSummaryFragment, UnitFragment } from '../schema'
import { GetSingleGame, GetSingleGameQuery, GetSingleGameQueryVariables } from '../schema'
import { GetTurnDetails, GetTurnDetailsQuery, GetTurnDetailsQueryVariables } from '../schema'
import { GetRegions, GetRegionsQuery, GetRegionsQueryVariables } from '../schema'
import { GetStructures, GetStructuresQuery, GetStructuresQueryVariables } from '../schema'
import { GetUnits, GetUnitsQuery, GetUnitsQueryVariables } from '../schema'
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

interface Progress {
    total: number
    position: number
}

interface ProgressCallback {
    (progress: Progress): void
}

export class GameStore {
    constructor() {
        makeObservable(this)
    }

    @observable loading = true
    @action startLoading = () => {
        this.loading = true
        this.loadingMessage = 'Loading...'
    }

    @action stopLoading = () => this.loading = false

    @observable loadingMessage = 'Loading...'
    @action updateLoadingMessage = (message: string) => this.loadingMessage = message

    @observable name: string
    @observable rulesetName: string
    @observable rulesetVersion: string

    @observable lastTurnNumber: number
    @observable factionName: string
    @observable factionNumber: number

    turns: IObservableArray<TurnDetailsFragment> = observable([])
    @observable turn: TurnDetailsFragment

    @observable world: World
    gameId: string = null

    async loadRegions(turnId: string, onProgress: ProgressCallback) {
        const items: RegionFragment[] = []

        let cursor: string = null
        let regions: ApolloQueryResult<GetRegionsQuery> = null
        do {
            regions = await CLIENT.query<GetRegionsQuery, GetRegionsQueryVariables>({
                query: GetRegions,
                variables: { cursor, turnId, pageSize: 1000 },
            })


            regions.data.node.regions.edges.forEach(x => items.push(x.node))

            onProgress({ total: regions.data.node.regions.totalCount, position: items.length })
            cursor = regions.data.node.regions.pageInfo.endCursor
        }
        while (regions.data.node.regions.pageInfo.hasNextPage)

        return items
    }

    async loadStructures(turnId: string, onProgress: ProgressCallback) {
        const items: StructureFragment[] = []

        let cursor: string = null
        let structures: ApolloQueryResult<GetStructuresQuery> = null
        do {
            structures = await CLIENT.query<GetStructuresQuery, GetStructuresQueryVariables>({
                query: GetStructures,
                variables: { cursor, turnId, pageSize: 1000 }
            })


            structures.data.node.structures.edges.forEach(x => items.push(x.node))

            onProgress({ total: structures.data.node.structures.totalCount, position: items.length })
            cursor = structures.data.node.structures.pageInfo.endCursor
        }
        while (structures.data.node.structures.pageInfo.hasNextPage)

        return items
    }

    async loadUnits(turnId: string, onProgress: ProgressCallback) {
        const items: UnitFragment[] = []

        let cursor: string = null
        let units: ApolloQueryResult<GetUnitsQuery> = null
        do {
            units = await CLIENT.query<GetUnitsQuery, GetUnitsQueryVariables>({
                query: GetUnits,
                variables: { cursor, turnId, pageSize: 1000 }
            })

            units.data.node.units.edges.forEach(x => items.push(x.node))

            onProgress({ total: units.data.node.units.totalCount, position: items.length })
            cursor = units.data.node.units.pageInfo.endCursor
        }
        while (units.data.node.units.pageInfo.hasNextPage)

        return items
    }

    async load(gameId: string) {
        if (this.gameId === gameId) {
            return
        }
        this.gameId = gameId

        this.startLoading()

        const response = await CLIENT.query<GetSingleGameQuery, GetSingleGameQueryVariables>({
            query: GetSingleGame,
            variables: {
                gameId
            }
        })

        const { myPlayer, ...game } = response.data.node
        const { lastTurnNumber } = myPlayer

        runInAction(() => {
            this.name = game.name
            this.rulesetName = game.rulesetName
            this.rulesetVersion = game.rulesetVersion

            this.factionName = myPlayer.factionName
            this.factionNumber = myPlayer.factionNumber
            this.lastTurnNumber = lastTurnNumber
        })

        const turnDetails = await CLIENT.query<GetTurnDetailsQuery, GetTurnDetailsQueryVariables>({
            query: GetTurnDetails,
            variables: {
                playerId: myPlayer.id,
                turnNumber: lastTurnNumber
            }
        })

        this.turns.push(turnDetails.data.node.turnByNumber)
        this.turn = turnDetails.data.node.turnByNumber

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
        for (const faction of this.turn.factions) {
            world.addFaction(faction.number, faction.name, faction.number === this.factionNumber)
        }

        this.updateLoadingMessage('Loading Regions...')
        const regions = await this.loadRegions(this.turn.id, ({ position, total }) => this.updateLoadingMessage(`Loading Regions: ${position} of ${total}`))

        this.updateLoadingMessage('Loading Structures...')
        const structures = await this.loadStructures(this.turn.id, ({ position, total }) => this.updateLoadingMessage(`Loading Structures: ${position} of ${total}`))

        this.updateLoadingMessage('Loading Units...')
        const units = await this.loadUnits(this.turn.id, ({ position, total }) => this.updateLoadingMessage(`Loading Units: ${position} of ${total}`))


        world.addRegions(regions)
        world.addStructures(structures)
        world.addUnits(units)

        for (const level of world.levels) {
            for (const region of level.regions) {
                region.sort()
            }
        }

        runInAction(() =>{
            console.log(`Turn ${this.turn.number} loaded`)

            this.world = world
            this.stopLoading()
        })
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
