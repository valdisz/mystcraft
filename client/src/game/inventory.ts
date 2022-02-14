import { Item } from './item';
import { ItemInfo } from './item-info';
import { ItemMap } from './item-map';
import { Unit } from './unit';
import { Income } from './income';


export enum TransferOutcome {
    Ok,
    NotEnaugh,
    NoItem
}

export interface Transfer {
    target: Inventory;
    item: ItemInfo;
    amount: number;
}

export class Tx {
    order: number
    // stage: 'initial' | 'give' | 'exchange' | 'tax' | 'cast' | 'sell' | 'buy' | 'move' | 'produce' | 'build' | 'entertain' | 'work' | 'transport'
    credit: Account
    debit: Account
    item: Item
}

export class Inventory2 {
    readonly items = new ItemMap<Item>()
}

export class Account {

}

export class Inventory {
    constructor() {
    }

    readonly items = new ItemMap<Item>();

    readonly credit: Transfer[] = [];
    readonly debit: Transfer[] = [];

    readonly balance = new ItemMap<Item>();

    readonly income: Income = {
        tax: 0,
        sell: 0,
        entertain: 0,
        work: 0
    }

    get men() {
        return this.items.toArray().filter(x => x.isManLike)
    }

    get menCount() {
        let count = 0
        for (const item of this.items) {
            if (!item.isManLike) {
                continue
            }

            count += item.amount
        }

        return count
    }

    transfer(target: Inventory, itemOrCode: ItemInfo | string, amount?: number) {
        const code = typeof itemOrCode === 'string' ? itemOrCode : itemOrCode.code;
        const item = this.balance.get(code);
        const src = this.items.get(code);

        if (!item) return TransferOutcome.NoItem;
        if (amount && item.amount < amount) return TransferOutcome.NotEnaugh;

        if (!amount) amount = item.amount;
        if (!src || src.amount < amount) {
            // todo: rewrite incoming transfers
        }

        this.debit.push({ amount, target, item: item.info });
        target.credit.push({ amount, target: this, item: item.info });

        // todo: update balance
    }

    receive(source: Unit, item: ItemInfo, amount: number) {
    }
}

/*

Turn processing is divided into several phases, which are producing different inputs and outputs.
Invetntory movement must take into account those phases.
Each inventory movement is dependeant on some command.
As well as subject of movement can be either unit or region itself.

Region contains limited or unlimited set of resources, that can be used.
Unit contains always limited amount of items.

*/

// class Entry {

// }

// class

// class Book {


//     balance() {

//     }

//     entry() {

//     }
// }
