import { ItemInfo, Capacity, MoveType, UniqueItem } from './internal';

export interface Resource {
    item: ItemInfo
    min: number
    max: number
    chance: number
}

export interface MoveCost {
    cost: number
    type: MoveType
}

export class TerrainInfo implements UniqueItem {
    constructor(public readonly code: string) {
    }

    movement: Capacity
    allowFlying: boolean
    allowRiding: boolean
    isBarren: boolean
    isWater: boolean
    races: ItemInfo[]
    coastalRaces: ItemInfo[]
    resources: Resource[]

    get name() {
        return this.code
    }

    getMoveCost(capacity: Capacity): MoveCost {
        let currentMt = null
        let curretCost = 0

        for (const mt of Object.keys(capacity)) {
            if (!capacity[mt]) {
                continue
            }

            const cost = this.movement[mt]
            if (curretCost > cost) {
                curretCost = cost
                currentMt = mt
            }
        }

        if (!currentMt) {
            return null
        }

        return {
            cost: curretCost,
            type: currentMt
        }
    }

    static readonly UNKNOWN = 'unknown'
}
