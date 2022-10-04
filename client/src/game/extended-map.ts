export interface ExtendedMapPredicate<K, V> {
    (value: V, key: K): boolean
}

export interface ExtendedMapTransform<K, V, U> {
    (value: V, key: K): U
}

export interface ExtendedMapReduce<K, V, U> {
    (previousValue: U, currentValue: V, currentKey: K, currentIndex: number, source: ExtendedMap<K, V>): U;
}

export class ExtendedMap<K, V> implements Iterable<V> {
    constructor(private readonly keyAccesor: (value: V) => K) {

    }

    private readonly items = new Map<K, V>();

    [Symbol.iterator](): Iterator<V, any, undefined> {
        return this.items.values()
    }

    get size() {
        return this.items.size
    }

    toArray() {
        return Array.from(this.items.values())
    }

    get(key: K): V {
        return this.items.get(key)
    }

    set(key: K, value: V): void {
        this.items.set(key, value)
    }

    add(value: V) {
        this.set(this.keyAccesor(value), value)
    }

    has(key: K) {
        return this.items.has(key)
    }

    delete(key: K) {
        return this.items.delete(key)
    }

    first() {
        const r = this.items.entries().next()
        return !r.done ? r.value[1] : null
    }

    some(predicate: ExtendedMapPredicate<K, V>): boolean {
        for (const [ k, v ] of this.items) {
            if (predicate(v, k)) {
                return true
            }
        }

        return false
    }

    all(predicate: ExtendedMapPredicate<K, V>): boolean {
        for (const [ k, v ] of this.items) {
            if (!predicate(v, k)) {
                return false
            }
        }

        return true
    }

    map<U>(predicate: ExtendedMapTransform<K, V, U>): U[] {
        const items: U[] = [ ]
        for (const [ k, v ] of this.items) {
            items.push(predicate(v, k))
        }

        return items
    }

    find(predicate: ExtendedMapPredicate<K, V>) {
        for (const [ k, v ] of this.items) {
            if (predicate(v, k)) {
                return v
            }
        }

        return null
    }

    filter(predicate: ExtendedMapPredicate<K, V>) {
        const items: V[] = [ ]
        for (const [ k, v ] of this.items) {
            if (predicate(v, k)) {
                items.push(v)
            }
        }

        return items
    }

    reduce<U>(predicate: ExtendedMapReduce<K, V, U>, initialValue: U): U {
        let value = initialValue
        let i = 0;
        for (const [ k, v ] of this.items) {
            value = predicate(value, v, k, i++, this)
        }

        return value
    }
}
