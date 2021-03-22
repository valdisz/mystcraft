import { ItemInfo } from "./item-info";
import { Unit } from './unit';


export interface Transfer {
    target: Unit;
    item: ItemInfo;
    amount: number;
}
