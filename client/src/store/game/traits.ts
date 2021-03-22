export type Traits = keyof TraitsMap

export abstract class Trait {
    abstract readonly type: Traits
}

export type AllTraits = SilverTrait | ManTrait | MonsterTrait

export interface TraitsMap {
    silver?: SilverTrait
    man?: ManTrait
    monster?: MonsterTrait
    illusion?: IllusionTrait
    food?: FoodTrait
}


export class SilverTrait extends Trait {
    readonly type: Traits = 'silver'
}

export class ManTrait extends Trait {
    readonly type: Traits = 'man'
}

export class MonsterTrait extends Trait {
    readonly type: Traits = 'monster'
}

export class IllusionTrait extends Trait {
    readonly type: Traits = 'illusion'
}

export class FoodTrait extends Trait {
    readonly type: Traits = 'food'

    constructor(public readonly value: number) {
        super()
    }
}
