import { UniqueItem } from "./unique-item"
import { Traits, TraitsMap } from './traits'
import { Item } from "./item"
import { ItemCategory } from "./item-category"

export class ItemInfo implements UniqueItem {
    constructor(public readonly code: string, public readonly category: ItemCategory, singular: string, plural: string) {
        this.name = [singular, plural]
    }

    readonly traits: TraitsMap = { }

    weight: number
    name: [string, string]
    description: string

    hasTrait(trait: Traits) {
        return !!this.traits[trait]
    }

    /**
     * Man-like items can act on its own and form a unit.
     */
    get isManLike() {
        return this.category === 'man' || this.hasTrait('freeMovingItem')
    }

    create(amount: number = 1): Item {
        const item = new Item(this)
        item.amount = amount

        return item
    }

    getName(count: number) {
        return this.name[count == 1 ? 0 : 1];
    }
}
