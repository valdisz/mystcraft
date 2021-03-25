import YAML from 'yaml'
import { List } from './list'
import { ItemInfo } from './item-info'
import { ItemCategory } from './item-category'
import { TerrainInfo } from './terrain-info'
import { AdvancedTrait, CanLearnTrait, CanMoveTrait, CanProduceTrait, CanSailTrait, CanTeachTrait, ConsumeTrait, FoodTrait, FreeMovingItemTrait,
    NoGiveTrait, NoTransportTrait, ProductionBonus } from './traits'
import { SkillInfo } from './skill-info'
import { Capacity } from './move-capacity'

export class Ruleset {
    readonly skills: List<Readonly<SkillInfo>> = new List();
    readonly items: List<Readonly<ItemInfo>> = new List();
    readonly terrain: List<Readonly<TerrainInfo>> = new List();

    getSkill(nameOrCode: string) {
        let skill = this.skills.get(nameOrCode)

        if (!skill) {
            skill = Object.freeze(new SkillInfo(nameOrCode))
            this.skills.set(skill)
        }

        return skill
    }

    getItem(nameOrCode: string) {
        let item = this.items.get(nameOrCode);

        if (!item) {
            for (const i of this.items.all) {
                for (const name of i.name) {
                    if (nameOrCode === name) {
                        item = i;
                        break;
                    }
                }
            }
        }

        // we must ensure that even missing items from the ruleset does not crash the client
        if (!item) {
            item = Object.freeze(new ItemInfo(nameOrCode, 'other', nameOrCode, nameOrCode))
            this.items.set(item)
        }

        return item;
    }

    getTerrain(name: string) {
        let terrain = this.terrain.get(name);

        // we must ensure that even missing terrain from the ruleset does not crash the client
        if (!terrain) {
            terrain = Object.freeze(new TerrainInfo(name))
            this.terrain.set(terrain)
        }

        return terrain;
    }

    load(source: string) {
        const data: RulesetData = YAML.parse(source)

        this.loadSkills(data.skills)
        this.loadItems(data.items)
        this.loadTerrain(data.terrain)
    }

    private loadSkills(skills: SkillDataMap) {
        for (const code in skills) {
            const { name, magic, description } = skills[code]

            const skill = new SkillInfo(code)
            skill.name = name
            skill.magic = magic
            skill.description = description

            this.skills.set(Object.freeze(skill))
        }
    }

    private loadItems(items: ItemDataMap) {
        const itemMap: {
            [ code: string ]: {
                item: ItemInfo
                data: ItemData
            }
        } = { }

        for (const code in items) {
            const data: ItemData = items[code]
            const item = new ItemInfo(code, data.category, data.name[0], data.name[1])
            item.description = data.description
            item.weight = data.weight

            itemMap[code] = { item, data }
        }

        for (const code in itemMap) {
            const { item, data } = itemMap[code]

            if (data.traits.advanced) item.traits.advanced = new AdvancedTrait()
            if (data.traits.noGive) item.traits.noGive = new NoGiveTrait()
            if (data.traits.noTransport) item.traits.noTransport = new NoTransportTrait()
            if (data.traits.freeMovingItem) item.traits.freeMovingItem = new FreeMovingItemTrait()
            if (data.traits.canTeach) item.traits.canTeach = new CanTeachTrait()

            if (data.traits.consume) {
                const { amount } = data.traits.consume
                item.traits.consume = new ConsumeTrait(amount)
            }

            if (data.traits.food) {
                const { value } = data.traits.food
                item.traits.food = new FoodTrait(value)
            }

            if (data.traits.canLearn) {
                const { defaultLevel, skills: _skills } = data.traits.canLearn
                const skills = (_skills ?? []).map(({ skill, level }) => ({
                    level,
                    skill: this.getSkill(skill)
                }))

                item.traits.canLearn = new CanLearnTrait(defaultLevel, skills)
            }

            if (data.traits.canMove) {
                const { capacity, speed, requires: _requires } = data.traits.canMove
                const requires = _requires ? this.getItem(_requires) : null

                item.traits.canMove = new CanMoveTrait(capacity, speed, requires)
            }

            if (data.traits.canSail) {
                const { capacity, speed, sailors } = data.traits.canSail

                item.traits.canSail = new CanSailTrait(capacity, speed, sailors)
            }

            if (data.traits.canProduce) {
                const { skill: _skill, level, effort, amount, productionBonus: _productionBonus } = data.traits.canProduce

                const skill = this.getSkill(_skill as any)
                const productionBonus: ProductionBonus = _productionBonus
                    ? {
                        item: this.getItem(_productionBonus.item),
                        amount: _productionBonus.amount
                    }
                    : null

                item.traits.canProduce = new CanProduceTrait(skill, level, effort, amount, productionBonus)
            }

            this.items.set(Object.freeze(item))
        }
    }

    private loadTerrain(terrain: TerrainDataMap) {
        for (const code in terrain) {
            const {
                allowFlying,
                allowRiding,
                isBarren,
                movement,
                coastalRaces: _coastalRaces,
                races: _races,
                resources: _resources
            } = terrain[code]

            const info = new TerrainInfo(code)
            info.movement = movement
            info.allowRiding = allowRiding
            info.allowFlying = allowFlying
            info.isBarren = isBarren

            info.coastalRaces = (_coastalRaces ?? []).map(race => this.getItem(race))
            info.races = (_races ?? []).map(race => this.getItem(race))
            info.resources = Object.keys(_resources ?? {}).map(res => {
                const {
                    amount: [ min, max ],
                    chance
                } = _resources[res]

                return {
                    item: this.getItem(res),
                    min,
                    max,
                    chance
                }
            })

            this.terrain.set(Object.freeze(info))
        }
    }
}

///// Ruleset Model

interface RulesetData {
    terrain: TerrainDataMap
    items: ItemDataMap
    skills: SkillDataMap
}

interface ObjectData {
    
}

interface ResourceData {
    amount: [ number, number ]
    chance: number
}

interface TerrainData {
    movement: Capacity
    allowFlying: boolean
    allowRiding: boolean
    isBarren: boolean
    races?: string[]
    coastalRaces?: string[]
    resources?: {
        [ code: string ]: ResourceData
    }
}

interface TerrainDataMap {
    [ name: string ]: TerrainData
}

interface SkillData {
    name: string
    magic: boolean
    description: string[]
}

interface SkillDataMap {
    [ code: string ]: SkillData
}

interface ItemDataMap {
    [ code: string ]: ItemData
}

interface TraitsData {
    consume?: {
        amount: number
    }
    food?: {
        value: number
    }
    canLearn?: {
        defaultLevel: number
        skills: {
            skill: string
            level: number
        }[]
    }
    canTeach?: {}
    canMove?: {
        capacity: Capacity
        speed: number
        requires?: string
    }
    canSail?: {
        capacity: Capacity
        speed: number
        sailors: number
    }
    canProduce?: {
        skill: string
        level: number
        effort: number
        amount: number
        productionBonus?: {
            item: string
            amount: number
        }
    }
    noGive?: {}
    noTransport?: {}
    freeMovingItem?: {}
    advanced?: {}
}

interface ItemData {
    name: [ string, string ]
    weight: number
    category: ItemCategory
    traits: TraitsData
    description: string

}
