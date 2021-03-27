import { Item } from './item';
import { ItemInfo } from "./item-info";
import { List } from './list';
import { Unit } from './unit';
import { Transfer } from "./transfer";
import { Income } from "./income";
import { TransferOutcome } from "./transfer-outcome";


export class Inventory {
    constructor(public readonly owner: Unit) {
    }

    readonly items = new List<Item>();

    readonly credit: Transfer[] = [];
    readonly debit: Transfer[] = [];

    readonly balance = new List<Item>();

    readonly income: Income = {
        tax: 0,
        sell: 0,
        entertain: 0,
        work: 0
    };

    transfer(target: Unit, itemOrCode: ItemInfo | string, amount?: number) {
        const code = typeof itemOrCode === 'string' ? itemOrCode : itemOrCode.code;
        const item = this.balance.get(code);
        const src = this.items.get(code);

        if (!item)
            return TransferOutcome.NoItem;
        if (amount && item.amount < amount)
            return TransferOutcome.NotEnaugh;

        if (!amount)
            amount = item.amount;
        if (!src || src.amount < amount) {
            // todo: rewrite incoming transfers
        }

        this.debit.push({ amount, target, item: item.info });
        target.inventory.credit.push({ amount, target: this.owner, item: item.info });

        // todo: update balance
    }

    receive(source: Unit, item: ItemInfo, amount: number) {
    }
}
