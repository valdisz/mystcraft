import { ItemInfo } from "./item-info"
import { Capacity } from "./move-capacity"
import { SkillInfo } from "./skill-info"

export type Traits = keyof TraitsMap

export abstract class Trait {
    abstract readonly type: Traits
}

export interface TraitsMap {
    consume?: ConsumeTrait
    food?: FoodTrait
    canLearn?: CanLearnTrait
    canTeach?: CanTeachTrait
    canMove?: CanMoveTrait
    canSail?: CanSailTrait
    defense?: DefenseTrait
    canProduce?: CanProduceTrait
    noGive?: NoGiveTrait
    noTransport?: NoTransportTrait
    freeMovingItem?: FreeMovingItemTrait
    advanced?: AdvancedTrait
}

export class ConsumeTrait extends Trait {
    readonly type: Traits = 'consume'

    constructor(public readonly amount: number) {
        super()
    }
}

export class FoodTrait extends Trait {
    readonly type: Traits = 'food'

    constructor(public readonly value: number) {
        super()
    }
}

export interface SkillKnowledge {
    readonly skill: SkillInfo
    readonly level: number
}

export class CanLearnTrait extends Trait {
    readonly type: Traits = 'canLearn'

    constructor(public readonly defaultLevel: number, public readonly skills: SkillKnowledge[]) {
        super()
    }
}

export class CanTeachTrait extends Trait {
    readonly type: Traits = 'canTeach'
}

export class CanMoveTrait extends Trait {
    readonly type: Traits = 'canMove'

    constructor(public readonly capacity: Capacity, public readonly speed: number, public readonly requires?: ItemInfo) {
        super()
    }
}

export class CanSailTrait extends Trait {
    readonly type: Traits = 'canSail'

    constructor(public readonly capacity: Capacity, public readonly speed: number, public readonly sailors: number) {
        super()
    }
}

export class DefenseTrait extends Trait {
    readonly type: Traits = 'defense'
}

export interface ProductionBonus {
    readonly item: ItemInfo
    readonly amount: number
}

export class CanProduceTrait extends Trait {
    readonly type: Traits = 'canProduce'

    constructor(
        public readonly skill: SkillInfo,
        public readonly level: number,
        public readonly effort: number,
        public readonly amount: number,
        public readonly productionBonus?: ProductionBonus
        ) {
        super()
    }
}

export class NoGiveTrait extends Trait {
    readonly type: Traits = 'noGive'
}

export class NoTransportTrait extends Trait {
    readonly type: Traits = 'noTransport'
}

export class FreeMovingItemTrait extends Trait {
    readonly type: Traits = 'freeMovingItem'
}

export class AdvancedTrait extends Trait {
    readonly type: Traits = 'advanced'
}
