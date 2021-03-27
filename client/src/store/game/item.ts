import { UniqueItem } from "./unique-item";
import { ItemInfo } from "./item-info";

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
