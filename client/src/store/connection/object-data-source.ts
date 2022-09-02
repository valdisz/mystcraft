import { DataSource, DataSourceOptions } from './data-source'
import { DataSourceConnection } from './data-source-connection'

export class ObjectDataSource<T extends object, TData = {}, TVariables extends object = {}, TError = unknown> extends DataSource<T, TData, TVariables, TError> {
    constructor(connection: DataSourceConnection<TData, TVariables, TError>, options: DataSourceOptions<T, TData, TVariables>) {
        super(connection, options)

        this._value = Object.assign({}, options.initialValue)
    }

    private _value: T

    protected getValue(): T {
        return this._value
    }

    protected setValue(value: T): void {
        this._value = Object.assign({}, value)
    }
}
