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

    get isSilver() {
        return this.info.isSilver
    }

    get isMan() {
        return this.info.isMan
    }
}
