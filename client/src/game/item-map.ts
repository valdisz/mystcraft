import { UniqueItem } from './unique-item'

export class ItemMap<T extends UniqueItem> {
    constructor(items?: T[]) {
        if (!items)
            return;

        for (const item of items) {
            this.set(item);
        }
    }

    private readonly items = new Map<string, T>()

    get size() {
        return this.items.size
    }

    get all() {
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
}
