import { UniqueItem, ItemInfo, getCategoryOrder } from './internal'

export class Item implements UniqueItem {
    constructor(public readonly info: ItemInfo) {

    }

    get name() {
        return this.info.getName(this.amount)
    }

    get code() {
        return this.info.code
    }

    amount: number
    price: number

    get weight() {
        return this.amount * this.info.weight
    }

    get isMoney() {
        return this.info.category === 'money'
    }

    get isMan() {
        return this.info.category === 'man'
    }

    get isManLike() {
        return this.info.isManLike
    }

    get isMount() {
        return this.info.category === 'mount'
    }
}

export function defaultItemOrder(a: Item, b: Item) {
    const aV = getCategoryOrder(a.info.category)
    const bV = getCategoryOrder(b.info.category)

    if (aV === bV) {
        if (a.price > 0) {
            return a.price - b.price
        }

        return a.amount - b.amount
    }

    return aV - bV
}
