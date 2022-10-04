import { RegionFragment, Stance, StructureFragment, UnitFragment, BattleFragment, Direction } from '../schema'
import {
    Ruleset, Region, Level, WorldInfo, Provinces, Structure, MovementPathfinder, ICoords, oppositeDirection, Factions, Unit,
    WorldLevel, Trade, Item, TradeRoute, Battle, Faction
} from './internal'

export class World {
    constructor(public readonly info: WorldInfo, public readonly ruleset: Ruleset) {
        for (let i = 0; i < info.map.length; i++) {
            this.addLevel(i, info.map[i])
        }
    }

    readonly levels: Level[] = [ ]
    readonly provinces = new Provinces()
    readonly factions = new Factions()
    readonly pathfinder = new MovementPathfinder()
    readonly battles: Battle[] = [ ]
    readonly tradeRoutes: TradeRoute[] = [ ]

    defaultStance: Stance = Stance.Neutral
    month: number
    year: number
    turnNumber: number
    unclaimed: number

    private addLevel(z: number, { width, height, label }: WorldLevel) {
        const level = new Level(width, height, z, label)
        this.levels.push(level)

        return level
    }

    private linkRegions(regions: RegionFragment[]) {
        for (const reg of regions) {
            var source = this.getRegion(reg)

            for (const exit of reg.exits) {
                if (source.neighbors.has(exit.direction)) {
                    continue
                }

                let target = this.getRegion(exit)

                if (!target) {
                    target = Region.fromExit(exit, this.ruleset)

                    const province = this.provinces.getOrCreate(exit.province)
                    target.province = province
                    province.add(target)

                    let level: Level = this.getLevel(target.coords.z)
                    level.add(target);
                }

                source.neighbors.set(source, exit.direction, target)
                target.neighbors.set(target, oppositeDirection(exit.direction), source)
            }
        }
    }

    private addCoveredRegions() {
        for (const level of this.levels) {
            for (let x = 0; x < level.width; x++) {
                for (let y = 0; y < level.height; y++) {
                    if ((x + y) % 2) {
                        continue
                    }

                    if (level.get(x, y)) {
                        continue
                    }

                    level.add(Region.createCovered(x, y, level.index, level.label, this.ruleset))
                }
            }
        }
    }

    private linkCoveredRegions() {
        for (const level of this.levels) {
            for (const reg of level) {
                if (!reg || reg.explored) {
                    continue
                }

                let neighborsNeeded = [
                    {direction: Direction.North, x: 0, y: -2},
                    {direction: Direction.Northwest, x: -1, y: -1},
                    {direction: Direction.Northeast, x: 1, y: -1},

                    {direction: Direction.South, x: 0, y: 2},
                    {direction: Direction.Southwest, x: -0, y: 1},
                    {direction: Direction.Southeast, x: 1, y: 1},
                ]

                for (const neighbor of neighborsNeeded) {
                    if (reg.neighbors.has(neighbor.direction)) {
                        continue
                    }

                    let neighborX = reg.coords.x + neighbor.x;
                    let neighborY = reg.coords.y + neighbor.y;

                    if ((neighborX + neighborY) % 2) {
                        continue
                    }

                    if (neighborX > level.width - 1) {
                        neighborX = 0;
                    }

                    if (neighborX < 0) {
                        neighborX = level.width - 1;
                    }

                    if (neighborY < 0) {
                        continue;
                    }

                    if (neighborY > level.height - 1) {
                        continue;
                    }

                    const target = this.getRegion(neighborX, neighborY, level.index);

                    if (!target) {
                        console.error(`Can't find connecting region ${neighborX}, ${neighborY} for ${reg.coords}`);
                        continue;
                    }

                    reg.neighbors.set(reg, neighbor.direction, target)
                    target.neighbors.set(target, oppositeDirection(neighbor.direction), reg);
                }
            }
        }
    }

    private linkProvinces() {
        for (const province of this.provinces) {
            const neighbors = new Set<string>()
            for (const reg of province.regions) {
                for (const exit of reg.neighbors) {
                    neighbors.add(exit.target.province.name)
                }
            }

            for (const name of neighbors) {
                if (name === province.name) continue

                const other = this.provinces.get(name)
                province.addBorderWith(other)
            }
        }
    }

    addFaction(num: number, name: string, isPlayer: boolean) {
        return this.factions.create(num, name, isPlayer)
    }

    setAttitudes(defaultAttitude: Stance, attitudes: Map<number, Stance>) {
        this.factions.unknown.attitude = defaultAttitude
        for (const faction of this.factions) {
            faction.attitude = attitudes.has(faction.num)
                ? attitudes.get(faction.num)
                : defaultAttitude
        }
    }

    addRegions(regions: RegionFragment[]) {
        for (const reg of regions) {
            this.addRegion(reg);

            for (const str of reg.structures) {
                this.addStructure(str);
            }
        }

        this.linkRegions(regions)
        this.linkProvinces()
        this.addCoveredRegions()
        this.linkCoveredRegions()
    }

    addUnits(units: UnitFragment[]) {
        for (const unit of units) {
            this.addUnit(unit);
        }
    }

    addRegion(region: RegionFragment) {
        const reg = Region.from(region, this.ruleset);

        const province = this.provinces.getOrCreate(region.province);
        province.add(reg);

        let level: Level = this.getLevel(reg.coords.z);
        level.add(reg);

        return reg;
    }

    addStructure(structure: StructureFragment) {
        const str = Structure.from(structure, this.ruleset)
        const region = this.getRegion(structure)

        region.addStructure(str)
    }

    addUnit(unit: UnitFragment) {
        const region = this.getRegion(unit)
        const u = Unit.from(unit, region, this.factions, this.ruleset)
        const structure = unit.structureNumber ? region.structures.find(x => x.num === unit.structureNumber) : null

        region.addUnit(u, structure)
    }

    addBattle(battle: BattleFragment) {
        const b = Battle.from(battle, this)

        const region = b.region
        region.battles.push(b)

        this.battles.push(b)
    }


    getRegion(id: string): Region
    getRegion(coords: ICoords): Region
    getRegion(x: number, y: number, z: number): Region
    getRegion(multi: ICoords | number | string, y?: number, z?: number): Region {
        if (typeof(multi) === 'string') {
            for (const level of this.levels) {
                const region = level.getById(multi)
                if (region) return region
            }

            return null
        }

        const level = this.getLevel(typeof(multi) === 'number' ? z : multi.z)
        if (!level) return null

        return typeof(multi) === 'number'
            ? level.get(multi, y)
            : level.get(multi)
    }

    getLevel(z: number): Level {
        return this.levels[z]
    }

    getFaction(num: number): Faction | null {
        return this.factions.get(num)
    }

    // todo: have global index of all units
    getUnit(num: number): Unit | null {
        for (const f of this.factions) {
            const unit = f.troops.get(num)
            if (unit) {
                return unit
            }
        }

        return null
    }

    findAllTradeRoutes() {
        for (const level of this.levels) {
            for (const route of this.findTradeRoutes(level)) {
                this.tradeRoutes.push(route)

                route.buy.region.addTradeRoute(route)
                route.sell.region.addTradeRoute(route)
            }
        }
    }

    findTradeRoutes(level: Level) {
        const pf = new MovementPathfinder()
        const markets = new Map<string, Trade>()

        const getTrade = (item: Item) => {
            if (!markets.has(item.code)) {
                markets.set(item.code, new Trade(item.info, level, pf))
            }

            return markets.get(item.code)
        }

        for (const region of level) {
            if (!region.settlement) {
                continue
            }

            for (const sale of region.forSale) {
                if (sale.info.category !== 'trade') {
                    continue
                }

                const trade = getTrade(sale)
                trade.supply(region, sale.price, sale.amount)
            }

            for (const buy of region.wanted) {
                if (buy.info.category !== 'trade') {
                    continue
                }

                const trade = getTrade(buy)
                trade.demand(region, buy.price, buy.amount)
            }
        }

        const routes: TradeRoute[] = []
        for (const trade of markets.values()) {
            for (const route of trade.getRoutes()) {
                routes.push(route)
            }
        }

        return routes
    }
}
