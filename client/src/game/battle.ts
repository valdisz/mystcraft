import { arrayEquals } from '../lib'
import { BattleFragment, BattleUnitFragment } from '../schema'
import { Item, ItemMap, Region, World, Unit, Faction, Skill, SkillInfo, ExtendedMap, ItemInfo, ItemCategory } from './internal'

export class BattleUnit {
    number: number
    name: string
    description?: string
    faction: Faction
    readonly flags: string[] = []
    readonly items = new ItemMap<Item>()
    readonly skills = new ItemMap<Skill>()
    unit?: Unit

    static from(src: BattleUnitFragment, world: World) {
        const { ruleset } = world

        const unit = new BattleUnit()
        unit.unit = world.getUnit(src.number)

        unit.number = src.number
        unit.name = src.name
        unit.description = src.description
        unit.flags.push(...src.flags)
        unit.faction = src.faction
            ? (world.getFaction(src.faction.number) ?? world.addFaction(src.faction.number, src.faction.name, false))
            : world.factions.unknown

        for (const { name, level } of src.skills ?? []) {
            const skill = ruleset.getSkill(name).create(SkillInfo.days(level), level)
            unit.skills.set(skill)
        }

        for (const { code, amount } of src.items ?? []) {
            const item = ruleset.getItem(code).create(amount)
            unit.items.set(item)
        }

        return unit
    }
}

export interface BattleRound {
    log: string
    statistics?: string
}

export class Casualties {
    lost: number
    readonly damagedUnits = new ExtendedMap<number, BattleUnit>(x => x.number)
}

export interface Squad {
    size: number

    men: Item[]

    behind: boolean

    comb: number
    ridi: number
    tact: number
    lbow: number
    xbow: number

    mounts?: ItemInfo
    weapon?: ItemInfo
    armour?: ItemInfo
    shield?: ItemInfo

    attackLevel: number
    defenseLevel: number
    rangedAttack: boolean
}

export class Army {
    constructor(readonly leader: BattleUnit) {

    }

    readonly units = new ExtendedMap<number, BattleUnit>(x => x.number)
    readonly casualties = new Casualties()
    readonly squads: Squad[] = []

    addUnit(unit: BattleUnit) {
        this.units.add(unit)

        // get list of the items by category
        const items = new Map<ItemCategory, Item[]>()

        function getBestItem(category: ItemCategory) {
            return items.has(category) && items.get(category)[0]
        }

        function decBestItem(category: ItemCategory, count: number) {
            if (!items.has(category)) {
                return
            }

            const cat = items.get(category)
            const item = cat[0]
            item.amount -= count

            if (item.amount === 0) {
                cat.shift()
            }

            if (cat.length === 0) {
                items.delete(category)
            }
        }

        for (const item of unit.items) {
            const { category } = item.info

            if (!items.has(category)) {
                items.set(category, [])
            }

            // copy items so that we can manipulate their amounts later
            items.get(category).push(item.info.create(item.amount))
        }

        // sort them by value, more valuable items on top
        for (const [ _, arr ] of items) {
            // sorting in reverse order so that array starts with the best item
            arr.sort((a, b) => b.info.value - a.info.value)
        }

        // form squads
        while (items.has('man')) {
            const men = getBestItem('man')
            const mounts = getBestItem('mount')
            const weapon = getBestItem('weapon')
            const armour = getBestItem('armor')
            const shield = getBestItem('shield')

            const ma = men.amount
            const count = Math.min(
                ma,
                mounts?.amount ?? ma,
                weapon?.amount ?? ma,
                armour?.amount ?? ma,
                shield?.amount ?? ma
            )

            // todo: hardcoded skills should be removed later
            // todo: take into account skill requirements and riding bonus levels

            const behind = unit.flags.includes('behind')
            const lbow = unit.skills.get('LBOW')?.level ?? 0
            const xbow = unit.skills.get('XBOW')?.level ?? 0
            const comb = unit.skills.get('COMB')?.level ?? 0
            const ridi = unit.skills.get('RIDI')?.level ?? 0
            const tact = unit.skills.get('TACT')?.level ?? 0

            let squad: Squad
            for (const s of this.squads) {
                const a = [ s.behind, s.comb, s.ridi, s.lbow, s.xbow, s.tact,     s.mounts,     s.weapon,     s.armour,     s.shield ]
                const b = [   behind,   comb,   ridi,   lbow,   xbow,   tact, mounts?.info, weapon?.info, armour?.info, shield?.info ]

                if (arrayEquals(a, b)) {
                    squad = s
                    break
                }
            }

            if (squad) {
                squad.size += count
                const m = squad.men.find(x => x.info === men.info)
                if (m) {
                    m.amount += count
                }
                else {
                    squad.men.push(men.info.create(count))
                }
            }
            else {
                const rangedAttack = weapon && weapon.info.hasTrait('weapon')
                    ? weapon.info.traits.weapon.attackType === 'ranged' && (lbow > 0 || xbow > 0)
                    : false

                let attackLevel = 0
                if (rangedAttack) {
                    attackLevel = Math.min(lbow, xbow)
                }
                else {
                    attackLevel = comb

                    if (mounts) {
                        attackLevel += ridi
                    }
                }

                squad = {
                    size: count,
                    men: [ men.info.create(count) ],
                    behind,
                    comb,
                    ridi,
                    lbow,
                    xbow,
                    tact,
                    mounts: mounts?.info,
                    weapon: weapon?.info,
                    armour: armour?.info,
                    shield: shield?.info,
                    rangedAttack,
                    attackLevel,
                    defenseLevel: attackLevel
                }
                this.squads.push(squad)
            }

            // reduce items in the current unit
            decBestItem('man', count)
            decBestItem('mount', count)
            decBestItem('weapon', count)
            decBestItem('armor', count)
            decBestItem('shield', count)
        }

        while (items.has('monster')) {
            const monsters = getBestItem('monster')

            let squad: Squad
            for (const s of this.squads) {
                if (s.men[0].info === monsters.info) {
                    squad = s
                    break
                }
            }

            if (squad) {
                squad.size += monsters.amount
                squad.men[0].amount += monsters.amount
            }
            else {
                squad = {
                    men: [ monsters.info.create(monsters.amount) ],
                    size: monsters.amount,
                    behind: false,
                    comb: 0,
                    ridi: 0,
                    lbow: 0,
                    xbow: 0,
                    tact: 0,
                    attackLevel: 0,
                    defenseLevel: 0,
                    rangedAttack: false
                }
                this.squads.push(squad)
            }

            decBestItem('monster', monsters.amount)
        }
    }
}

export class Battle {
    constructor(
        public readonly region: Region,
        attacker: BattleUnit,
        defender: BattleUnit,
    ) {
        this.attacker = new Army(attacker)
        this.defender = new Army(defender)
    }

    readonly attacker: Army
    readonly defender: Army
    readonly rounds: BattleRound[] = []
    readonly spoils = new ItemMap<Item>()
    readonly rose = new ItemMap<Item>()
    statistics: string

    static from(src: BattleFragment, world: World) {
        const units = new Map<number, BattleUnit>()
        for (const unit of src.attackers.map(x => BattleUnit.from(x, world))) {
            units.set(unit.number, unit)
        }

        for (const unit of src.defenders.map(x => BattleUnit.from(x, world))) {
            units.set(unit.number, unit)
        }

        const { x, y, z } = src.location.coords
        const region = world.getRegion(x, y, z)

        const battle = new Battle(region, units.get(src.attacker.number), units.get(src.defender.number))

        for (const unit of src.attackers.map(x => units.get(x.number))) {
            battle.attacker.addUnit(unit)
        }

        for (const unit of src.defenders.map(x => units.get(x.number))) {
            battle.defender.addUnit(unit)
        }

        for (const { army, lost, damagedUnits } of src.casualties) {
            const casualties = army.number === battle.attacker.leader.number
                ? battle.attacker.casualties
                : battle.defender.casualties

            casualties.lost = lost
            for (const unit of damagedUnits.map(x => units.get(x))) {
                casualties.damagedUnits.add(unit)
            }
        }

        for (const { log, statistics } of src.rounds) {
            battle.rounds.push({ log, statistics })
        }

        console.log(battle)

        return battle
    }
}
