import { computed, makeObservable, override } from 'mobx'
import { DataSource, DataSourceOptions } from './data-source'
import { DataSourceConnection } from './data-source-connection'

export class ArrayDataSource<T, TData = {}, TVariables extends object = {}, TError = unknown> extends DataSource<T[], TData, TVariables, TError> {
    constructor(connection: DataSourceConnection<TData, TVariables, TError>, options: DataSourceOptions<T[], TData, TVariables>) {
        super(connection, options)

        // makeObservable(this, {
        //     isLoading: override,
        //     isReady: override,
        //     state: override,
        //     error: override,
        //     isEmpty: computed
        // })

        this._value = Array.from(options.initialValue)
    }

    get isEmpty() {
        return this.value.length === 0
    }

    private _value: T[]

    protected getValue(): T[] {
        return this._value
    }

    protected setValue(value: T[]): void {
        this._value = Array.from(value)
    }

    map<U>(callbackfn: (value: T, index: number, array: T[]) => U, thisArg?: any): U[] {
        return this.value.map(callbackfn)
    }

    filter<S extends T>(predicate: (value: T, index: number, array: T[]) => value is S, thisArg?: any): S[] {
        return this.value.filter(predicate)
    }
}
