import { ItemInfo } from './item-info';
import { Capacity } from './move-capacity';
import { UniqueItem } from './unique-item';

export interface Resource {
    item: ItemInfo
    min: number
    max: number
    chance: number
}

export class TerrainInfo implements UniqueItem {
    constructor(public readonly code: string) {
    }

    movement: Capacity
    allowFlying: boolean
    allowRiding: boolean
    isBarren: boolean
    races: ItemInfo[]
    coastalRaces: ItemInfo[]
    resources: Resource[]
}
