export type Traits = 'silver' | 'man'

export abstract class Trait {
    abstract readonly type: Traits
}

export type AllTraits = SilverTrait | ManTrait

export type TraitsMap = {
    [ trait in Traits ]?: AllTraits[]
}


// this is silver
export class SilverTrait extends Trait {
    readonly type: Traits = 'silver'
}

// this is man
export class ManTrait extends Trait {
    readonly type: Traits = 'man'
}
