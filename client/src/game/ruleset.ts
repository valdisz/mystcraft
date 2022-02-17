import YAML from 'yaml'
import { ItemMap } from './item-map'
import { ItemInfo } from './item-info'
import { ItemCategory } from './item-category'
import { TerrainInfo } from './terrain-info'
import {
    AdvancedTrait, CanLearnTrait, CanMoveTrait, CanProduceTrait, CanSailTrait, CanTeachTrait, ConsumeTrait, FoodTrait,
    FreeMovingItemTrait, NoGiveTrait, NoTransportTrait, ProductionBonus
} from './traits'
import { SkillInfo } from './skill-info'
import { Capacity, MoveType } from './move-capacity'

export class Ruleset {
    readonly skills: ItemMap<Readonly<SkillInfo>> = new ItemMap();
    readonly items: ItemMap<Readonly<ItemInfo>> = new ItemMap();
    readonly terrain: ItemMap<Readonly<TerrainInfo>> = new ItemMap();
    readonly movePoints: Map<MoveType, number> = new Map();
    readonly orders: string[] = [];

    money: Readonly<ItemInfo>

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
            for (const i of this.items) {
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

    getMovePoints(moveType: MoveType) {
        return this.movePoints.get(moveType)
    }

    parse(source: string) {
        const data: RulesetData = YAML.parse(source)
        this.load(data)
    }

    load(data: RulesetData) {
        this.loadSkills(data.skills)
        this.loadItems(data.items)
        this.loadTerrain(data.terrain)
        this.loadOrders(data.orders)
        this.loadMovePoints(data.movePoints)

        for (const item of this.items) {
            if (item.category === 'money') {
                this.money = item
                break
            }
        }
    }

    private loadMovePoints(movePoints: MovePointsMap) {
        Object.keys(movePoints).forEach(x => this.movePoints.set(x as MoveType, movePoints[x]))
    }

    private loadOrders(orders: OrdersDataMap) {
        Object.keys(orders).forEach(x => this.orders.push(x))
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
                item: ItemInfo,
                data: ItemData
            }
        } = { };

        function populateDict() {
            for (const code in items) {
                const data: ItemData = items[code]
                const item = new ItemInfo(code, data.category, data.name[0], data.name[1])
                item.description = data.description
                item.weight = data.weight

                itemMap[code] = ({ item, data })
            }
        }

        populateDict()

        for (const code in itemMap) {
            const { item, data } = itemMap[code]

            if (data.traits) {
                if (data.traits.advanced !== undefined) item.traits.advanced = new AdvancedTrait()
                if (data.traits.noGive !== undefined) item.traits.noGive = new NoGiveTrait()
                if (data.traits.noTransport !== undefined) item.traits.noTransport = new NoTransportTrait()
                if (data.traits.freeMovingItem !== undefined) item.traits.freeMovingItem = new FreeMovingItemTrait()
                if (data.traits.canTeach !== undefined) item.traits.canTeach = new CanTeachTrait()

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
                    const skills = Object.keys(_skills ?? {}).map(skill => ({
                        skill: this.getSkill(skill),
                        level: _skills[skill]
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

                    item.traits.canProduce = new CanProduceTrait(skill, level, effort, amount, [], productionBonus)
                }
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
                isWater,
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
            info.isWater = isWater

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

        const info = new TerrainInfo(TerrainInfo.UNKNOWN)
        info.movement = {
            fly: 1,
            ride: 1,
            swim: 1,
            walk: 1
        }
        info.allowRiding = true
        info.allowFlying = true
        info.isBarren = true
        info.isWater = false

        this.terrain.set(Object.freeze(info))
    }
}

///// Ruleset Model

interface RulesetData {
    terrain: TerrainDataMap
    items: ItemDataMap
    skills: SkillDataMap
    orders: OrdersDataMap
    movePoints: MovePointsMap
}

interface MovePointsMap {
    [ moveType: string ]: number
}

interface OrdersDataMap {
    [ order: string ]: any
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
    isWater: boolean
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
