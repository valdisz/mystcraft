import { DataSource, DataSourceOptions } from './data-source'
import { DataSourceConnection } from './data-source-connection'

export interface SeqDataSource<T> {
    [Symbol.iterator](): IterableIterator<T>
    readonly isEmpty: boolean
    readonly length: number
    map<U>(callbackfn: (value: T, index: number, array: T[]) => U, thisArg?: any): U[]
    filter<S extends T>(predicate: (value: T, index: number, array: T[]) => value is S, thisArg?: any): S[]
}

export class ArrayDataSource<T, TData = {}, TVariables extends object = {}, TError = unknown>
    extends DataSource<T[], TData, TVariables, TError>
    implements SeqDataSource<T> {

    constructor(connection: DataSourceConnection<TData, TVariables, TError>, options: DataSourceOptions<T[], TData, TVariables>) {
        super(connection, options)

        this._value = Array.from(options.initialValue)
    }

    private _value: T[]

    *[Symbol.iterator](): IterableIterator<T> {
        for (let i = 0; i < this._value.length; ++i) {
            yield this._value[i];
        }
    }

    get isEmpty() {
        return this.value.length === 0
    }

    get length() {
        return this.value.length
    }

    protected getValue(): T[] {
        return this._value
    }

    protected setValue(value: T[]): void {
        this._value.length = 0
        this._value.push(...value)
    }

    /////////////////////////
    ///// reading sequence

    map<U>(callbackfn: (value: T, index: number, array: T[]) => U, thisArg?: any): U[] {
        return this.value.map(callbackfn)
    }

    filter<S extends T>(predicate: (value: T, index: number, array: T[]) => value is S, thisArg?: any): S[] {
        return this.value.filter(predicate)
    }

    find(predicate: (value: T, index: number, obj: T[]) => boolean, thisArg?: any): T | undefined {
        return this.value.find(predicate)
    }

    /////////////////////////
    ///// writing sequence

    push(...items: T[]): number {
        const ret = this._value.push(...items)

        if (items?.length) {
            this.valueChanged()
        }

        return ret
    }

    pop(): T | undefined {
        const willChange = this._value.length > 0

        const ret = this._value.pop()

        if (willChange) {
            this.valueChanged()
        }

        return ret
    }

    splice(start: number, deleteCount: number, ...items: T[]): T[] {
        const ret = this._value.splice(start, deleteCount, ...items)

        if (ret.length || items?.length) {
            this.valueChanged()
        }

        return ret
    }

    insert(index: number, ...items: T[]): T[] {
        return this.splice(index, 0, ...items)
    }

    remove(predicate: (value: T, index: number, array: T[]) => boolean): T[] {
        const ret = []
        for (let i = this._value.length - 1; i >= 0; --i) {
            if (predicate(this._value[i], i, this._value)) {
                ret.push(...this._value.splice(i, 1))
            }
        }

        if (ret.length) {
            this.valueChanged()
        }

        return ret
    }
}
