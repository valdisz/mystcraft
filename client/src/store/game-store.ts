import { ApolloQueryResult } from 'apollo-client'
import { makeObservable, observable, IObservableArray, runInAction, action, computed, reaction, comparer } from 'mobx'
import { CLIENT } from '../client'
import { RegionFragment, UnitFragment, TurnFragment } from '../schema'
import { GetGame, GetGameQuery, GetGameQueryVariables } from '../schema'
import { GetTurn, GetTurnQuery, GetTurnQueryVariables } from '../schema'
import { GetRegions, GetRegionsQuery, GetRegionsQueryVariables } from '../schema'
import { GetUnits, GetUnitsQuery, GetUnitsQueryVariables } from '../schema'
import { SetOrder, SetOrderMutation, SetOrderMutationVariables } from '../schema'
import { Ruleset } from "../game/ruleset"
import { Region } from "../game/region"
import { World } from "../game/world"
import { WorldInfo, WorldLevel } from '../game/world-info'
import { Unit } from '../game/types'
import { saveAs } from 'file-saver'
import { InterfaceCommand, MoveCommand } from './commands/move'

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

export type OrdersState = 'SAVED' | 'UNSAVED' | 'SAVING' | 'ERROR'

export class GameStore {
    constructor() {
        makeObservable(this)

        reaction(
            () => ({ unit: this.unit, orders: this.unitOrders }),
            async ({ unit, orders }, { unit: prevUnit, orders: prevOrders }) => {
                if (!unit || !prevUnit || unit.num !== prevUnit.num) return

                this.startOrdersSaving()

                if (this.ordersSaveAbortController) {
                    this.ordersSaveAbortController.abort()
                }
                this.ordersSaveAbortController = new AbortController()

                try {
                    const response = await CLIENT.mutate<SetOrderMutation, SetOrderMutationVariables>({
                        mutation: SetOrder,
                        variables: {
                            unitId: unit.id,
                            orders
                        },
                        context: {
                            fetchOptions: {
                                signal: this.ordersSaveAbortController.signal
                            }
                        }
                    })

                    const result = response.data?.setOrders
                    if (!response.errors && result?.isSuccess) {
                        unit.setOrders(orders)
                    }
                    else {
                        this.setOrders(prevOrders)
                        this.errorOrdersSaving()
                    }

                    this.stopOrdersSaving()
                }
                catch {
                    this.errorOrdersSaving()
                }
            },
            {
                equals: comparer.shallow,
                delay: 1000
            }
        )

        this.commands.push(new MoveCommand(this))
    }

    readonly commands: InterfaceCommand[] = [ ]

    private ordersSaveAbortController: AbortController

    @observable loading = true
    @action startLoading = () => {
        this.loading = true
        this.loadingMessage = 'Loading...'
    }

    @action stopLoading = () => this.loading = false

    @observable loadingMessage = 'Loading...'
    @action updateLoadingMessage = (message: string) => this.loadingMessage = message

    @observable name: string = null
    @observable rulesetName: string = null
    @observable rulesetVersion: string = null

    @observable lastTurnNumber: number = null
    @observable factionName: string = null
    @observable factionNumber: number = null

    @observable turn: TurnFragment = null

    @observable world: World = null
    gameId: string = null

    async loadRegions(turnId: string, onProgress: ProgressCallback) {
        const items: RegionFragment[] = []

        let skip = 0
        let response: ApolloQueryResult<GetRegionsQuery> = null
        do {
            response = await CLIENT.query<GetRegionsQuery, GetRegionsQueryVariables>({
                query: GetRegions,
                variables: { skip, turnId, pageSize: 1000 },
            })

            const data = response.data.node
            data.regions.items.forEach(x => items.push(x))

            onProgress({ total: data.regions.totalCount, position: items.length })
            skip = items.length
        }
        while (response.data.node.regions.pageInfo.hasNextPage)

        return items
    }

    async loadUnits(turnId: string, onProgress: ProgressCallback) {
        const items: UnitFragment[] = []

        let skip = 0
        let response: ApolloQueryResult<GetUnitsQuery> = null
        do {
            response = await CLIENT.query<GetUnitsQuery, GetUnitsQueryVariables>({
                query: GetUnits,
                variables: { skip, turnId, pageSize: 1000 }
            })

            const data = response.data.node
            data.units.items.forEach(x => items.push(x))

            onProgress({ total: data.units.totalCount, position: items.length })
            skip = items.length
        }
        while (response.data.node.units.pageInfo.hasNextPage)

        return items
    }

    async load(gameId: string) {
        if (this.gameId === gameId) {
            return
        }
        this.gameId = gameId

        this.startLoading()

        const response = await CLIENT.query<GetGameQuery, GetGameQueryVariables>({
            query: GetGame,
            variables: {
                gameId
            }
        })

        const { me, ...game } = response.data.node

        runInAction(() => {
            this.name = game.name
            this.rulesetName = game.rulesetName
            this.rulesetVersion = game.rulesetVersion

            this.factionName = me.name
            this.factionNumber = me.number
            this.lastTurnNumber = me.lastTurnNumber
        })

        const turnDetails = await CLIENT.query<GetTurnQuery, GetTurnQueryVariables>({
            query: GetTurn,
            variables: {
                turnId: me.lastTurnId
            }
        })

        this.turn = turnDetails.data.node

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

        this.updateLoadingMessage('Loading Units...')
        const units = await this.loadUnits(this.turn.id, ({ position, total }) => this.updateLoadingMessage(`Loading Units: ${position} of ${total}`))

        world.addRegions(regions)
        world.addUnits(units)

        for (const level of world.levels) {
            for (const region of level) {
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
            this.region = reg && !reg.covered ? observable(reg) : null
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
    @observable unitOrders: string = ''
    @observable ordersState: OrdersState = 'SAVED'
    ordersChanged = false

    @computed get isOrdersVisible() {
        return this.unit?.isPlayer ?? false
    }

    @action selectUnit = (unit: Unit) => {
        this.unit = unit
        this.unitOrders = unit?.ordersSrc
        this.ordersState = 'SAVED'
    }

    @action setOrders = (orders: string) => {
        const changed = this.unitOrders !== orders
        this.unitOrders = orders

        if (changed) {
            this.ordersState = 'UNSAVED'
            this.ordersChanged = true
        }
    }

    @action startOrdersSaving = () => this.ordersState = 'SAVING'
    @action stopOrdersSaving = () => this.ordersState = 'SAVED'
    @action errorOrdersSaving = () => this.ordersState = 'ERROR'

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

    getOrders = () => {
        const lines = [
            '#atlantis ""'
        ]

        var faction = this.world.factions.get(this.factionNumber)
        for (const unit of faction.troops) {
            lines.push(`unit ${unit.num}`)
            lines.push(unit.ordersSrc)
            lines.push('')
        }

        lines.push('#end')
        lines.push('')

        const orders = lines.join('\n')
        const blob = new Blob([ orders ], { type: 'text/plain' })
        saveAs(blob, `orders-${this.turn.number}.ord`)
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
