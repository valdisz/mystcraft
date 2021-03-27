import { UniqueItem } from './unique-item'

export class List<T extends UniqueItem> {
    constructor(items?: T[]) {
        if (!items)
            return;

        for (const item of items) {
            this.set(item);
        }
    }

    private readonly items: {
        [code: string]: T;
    } = {};

    get length() {
        return Object.keys(this.items).length;
    }

    get all() {
        return Object.values(this.items);
    }

    get(code: string | UniqueItem): T {
        if (!code) return null
        
        return this.items[typeof code === 'string' ? code : code.code];
    }

    set(item: T): void {
        this.items[item.code] = item;
    }

    contains(item: T | string) {
        return !!this.items[typeof item === 'string' ? item : item.code];
    }

    remove(item: T | string) {
        const code = typeof item === 'string'
            ? item
            : item.code;

        if (this.items[code]) {
            delete this.items[code];
        }
    }
}
