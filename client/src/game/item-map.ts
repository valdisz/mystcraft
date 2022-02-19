import { UniqueItem } from './unique-item'

export interface ItemMapPredicate<T extends UniqueItem> {
    (item: T): boolean
}

export interface ItemMapTransform<T extends UniqueItem, U> {
    (item: T): U
}

export class ItemMap<T extends UniqueItem> implements Iterable<T> {
    constructor(items?: T[]) {
        if (!items)
            return;

        for (const item of items) {
            this.set(item);
        }
    }

    private readonly items = new Map<string, T>();

    [Symbol.iterator](): Iterator<T, any, undefined> {
        return this.items.values()
    }

    get size() {
        return this.items.size
    }

    toArray() {
        return Array.from(this.items.values())
    }

    get(code: string | UniqueItem): T {
        if (!code) return null

        return this.items.get(typeof code === 'string' ? code : code.code)
    }

    set(item: T): void {
        this.items.set(item.code, item)
    }

    has(item: T | string) {
        return this.items.has(typeof item === 'string' ? item : item.code)
    }

    delete(item: T | string) {
        const code = typeof item === 'string'
            ? item
            : item.code;

        return this.items.delete(code)
    }

    some(p: ItemMapPredicate<T>): boolean {
        for (const kv of this.items) {
            if (p(kv[1])) {
                return true
            }
        }

        return false
    }

    all(p: ItemMapPredicate<T>): boolean {
        for (const kv of this.items) {
            if (!p(kv[1])) {
                return false
            }
        }

        return true
    }

    map<U>(p: ItemMapTransform<T, U>): U[] {
        const items: U[] = [ ]
        for (const kv of this.items) {
            items.push(p(kv[1]))
        }

        return items
    }

    find(predicate: ItemMapPredicate<T>) {
        for (const kv of this.items) {
            if (predicate(kv[1])) {
                return kv[1]
            }
        }

        return null
    }

    filter(predicate: ItemMapPredicate<T>) {
        const items: T[] = [ ]
        for (const kv of this.items) {
            if (predicate(kv[1])) {
                items.push(kv[1])
            }
        }

        return items
    }
}
