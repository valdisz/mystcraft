import { createAtom, IAtom, IReactionDisposer, makeObservable, observable, IObservableValue, reaction, computed } from 'mobx'
import { OperationResult, TypedDocumentNode } from 'urql'
import { DocumentNode } from 'graphql'
import { Disposable } from './disposable'
import { RequestPolicy, DataSourceConnection } from './data-source-connection'

export type DataSourceState = 'loading' | 'ready' | 'failed' | 'unspecified'

export interface Projection<S, D> {
    (source: S): D
}

export interface VariablesGetter<T> {
    (): T
}

export interface DataSourceOptions<T, TData, TVariables extends object> {
    initialValue: T,
    document: DocumentNode | TypedDocumentNode<TData, TVariables>
    projection: Projection<TData, T>
    variables?: VariablesGetter<TVariables>
    name?: string
}

export interface ResponseHandler<TData, TVariables extends object, TError> {
    onSuccess: (data: TData, result?: OperationResult<TData, TVariables>) => void
    onFailure: (error: TError, result?: OperationResult<TData, TVariables>) => void
}

export abstract class DataSource<T, TData = { }, TVariables extends object = { }, TError = unknown> {
    constructor(private readonly connection: DataSourceConnection<TData, TVariables, TError>, options: DataSourceOptions<T, TData, TVariables>) {
        makeObservable(this, {
            isLoading: computed,
            isReady: computed,
            state: computed,
            error: computed
        })

        this._document = options.document
        this._projection = options.projection
        this._variables = options.variables

        this._name = options.name ?? 'DataSource'
        this._valueAtom = createAtom(this._name, this.resume.bind(this), this.suspend.bind(this))
    }

    private readonly _valueAtom: IAtom
    private readonly _name: string
    private _state: IObservableValue<DataSourceState> = observable.box<DataSourceState>('unspecified')
    private _error: IObservableValue<TError | null> = observable.box<TError | null>(null)
    private _document: DocumentNode | TypedDocumentNode<TData, TVariables>
    private _projection: (data: TData) => T
    private _variables?: () => TVariables

    private _variablesWatch: IReactionDisposer
    private _requestHandle: Disposable

    protected abstract getValue(): T;
    protected abstract setValue(value: T): void;

    get value(): T {
        this._valueAtom.reportObserved()

        return this.getValue()
    }

    get state() {
        return this._state.get()
    }

    protected set state(newState: DataSourceState) {
        this._state.set(newState)
    }

    get isLoading() {
        return this.state === 'loading'
    }

    get isReady() {
        return this.state === 'ready'
    }

    get error(): TError | null {
        return this._error.get()
    }

    protected set error(err: TError) {
        this._error.set(err)
    }

    defaultRequestPolicy: RequestPolicy = RequestPolicy.NetworkOnly

    protected resume() {
        console.debug(`${this._name}::resume`)

        if (this._variables) {
            this._variablesWatch = reaction(this._variables, vars => this.load(vars, this.defaultRequestPolicy), {
                name: `${this._valueAtom.name_}::variables`,
                fireImmediately: true
            })
        }
        else {
            this.load(null, this.defaultRequestPolicy)
        }
    }

    protected suspend() {
        console.debug(`${this._name}::suspend`)

        if (this._variablesWatch) {
            this._variablesWatch()
            this._variablesWatch = null
        }

        if (this._requestHandle) {
            this._requestHandle.dispose()
            this._requestHandle = null
        }
    }

    protected load(variables: TVariables, requestPolicy: RequestPolicy) {
        console.debug(`${this._name}::load`)

        return new Promise((resolve, reject) => {
            this.state = 'loading'

            if (this._requestHandle) {
                this._requestHandle.dispose()
            }

            this._requestHandle = this.connection.query(this._document, {
                onSuccess: data => {
                    const projection = this._projection(data)

                    this.setValue(projection)

                    this.state = 'ready'
                    this._requestHandle = null

                    this._valueAtom.reportChanged()

                    resolve(projection)
                },
                onFailure: error => {
                    this.state = 'failed'
                    this.error = error

                    reject(error)
                }
            }, variables, requestPolicy)
        })
    }

    reload(requestPolicy?: RequestPolicy) {
        const variables = this._variables ? this._variables() : null

        return this.load(variables, requestPolicy || this.defaultRequestPolicy)
    }
}
