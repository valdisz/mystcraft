import { BattleFragment, BattleUnitFragment } from '../schema'
import { Item, ItemMap, Region, World, Unit, Faction, Skill, SkillInfo, ExtendedMap } from './internal'

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

export class Battle {
    constructor(
        public readonly region: Region,
        public readonly attacker: BattleUnit,
        public readonly defender: BattleUnit,
    ) {

    }

    readonly attackers = new ExtendedMap<number, BattleUnit>(x => x.number)
    readonly defenders = new ExtendedMap<number, BattleUnit>(x => x.number)
    readonly rounds: BattleRound[] = []
    readonly attackerCasualties = new Casualties()
    readonly defenderCasualties = new Casualties()
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
            battle.attackers.add(unit)
        }

        for (const unit of src.defenders.map(x => units.get(x.number))) {
            battle.defenders.add(unit)
        }

        for (const { army, lost, damagedUnits } of src.casualties) {
            const casualties = army.number === battle.attacker.number
                ? battle.attackerCasualties
                : battle.defenderCasualties

            casualties.lost = lost
            for (const unit of damagedUnits.map(x => units.get(x))) {
                casualties.damagedUnits.add(unit)
            }
        }

        return battle
    }
}
