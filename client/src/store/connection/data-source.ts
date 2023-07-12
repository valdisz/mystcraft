import { createAtom, IAtom, IReactionDisposer, reaction, transaction, IReactionOptions } from 'mobx'
import { OperationResult, TypedDocumentNode } from 'urql'
import { DocumentNode } from 'graphql'
import { RequestPolicy, DataSourceConnection, Disposable } from './data-source-connection'

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
    variables?: TVariables | VariablesGetter<TVariables>
    name?: string
    defaultRequestPolicy?: RequestPolicy
    defaultReloadPolicy?: RequestPolicy
}

export interface ResponseHandler<TData, TVariables extends object, TError> {
    onSuccess: (data: TData, result?: OperationResult<TData, TVariables>) => void
    onFailure: (error: TError, result?: OperationResult<TData, TVariables>) => void
}

export interface ReloadOptions {
    /**
     * Request policy to use for the reload operation
     */
    requestPolicy?: RequestPolicy

    /**
     * Force reload data from connection even if no one is observing
     */
    force?: boolean
}

export abstract class DataSource<T, TData = { }, TVariables extends object = { }, TError = unknown> {
    constructor(private readonly connection: DataSourceConnection<TData, TVariables, TError>, options: DataSourceOptions<T, TData, TVariables>) {
        this._name = options.name ?? 'DataSource'
        this._document = options.document
        this._projection = options.projection
        this._variables = options.variables
        this._defaultRequestPolicy = options.defaultRequestPolicy || RequestPolicy.CacheAndNetwork
        this._defaultReloadPolicy = options.defaultReloadPolicy || RequestPolicy.NetworkOnly

        this._valueAtom = createAtom(this._name, this.resume.bind(this), this.suspend.bind(this))
        this._stateAtom = createAtom(`${this._name}::state`)
        this._errorAtom = createAtom(`${this._name}::error`)
    }

    /**
     * Name of the data source
     */
    private readonly _name: string
    private readonly _defaultRequestPolicy: RequestPolicy
    private readonly _defaultReloadPolicy: RequestPolicy

    private readonly _valueAtom: IAtom
    protected abstract getValue(): T;
    protected abstract setValue(value: T): void;

    private readonly _stateAtom: IAtom
    private _state: DataSourceState = 'unspecified'

    private readonly _errorAtom: IAtom
    private _error: TError | null = null

    private readonly _document: DocumentNode | TypedDocumentNode<TData, TVariables>
    private readonly _projection: (data: TData) => T
    private readonly _variables?: TVariables | VariablesGetter<TVariables>

    private _variablesWatch: IReactionDisposer
    private _requestHandle: Disposable

    get value(): T {
        this._valueAtom.reportObserved()
        return this.getValue()
    }

    get state(): DataSourceState {
        this._stateAtom.reportObserved()
        return this._state
    }

    protected set state(newState: DataSourceState) {
        this._state = newState
        this._stateAtom.reportChanged()
    }

    get isLoading() {
        return this.state === 'loading'
    }

    get isReady() {
        return this.state === 'ready'
    }

    get isFailed() {
        return this.state === 'failed'
    }

    get error(): TError | null {
        this._errorAtom.reportObserved()
        return this._error
    }

    protected set error(err: TError) {
        this._error = err
        this._errorAtom.reportChanged()
    }

    protected resume() {
        console.debug(`${this._name}::resume()`)

        if (this._variables) {
            const options: IReactionOptions<TVariables, any> = {
                name: this._name,
                fireImmediately: true
            }

            const expression = () => {
                if (this._variables instanceof Function) {
                    return this._variables()
                }
                else {
                    return {...this._variables}
                }
            }

            const effect = vars => this.load(vars, this._defaultRequestPolicy)

            this._variablesWatch = reaction(expression, effect, options)
        }
        else {
            this.load(null, this._defaultRequestPolicy)
        }
    }

    protected suspend() {
        console.debug(`${this._name}::suspend()`)

        this.close()
    }

    protected load(variables: TVariables, requestPolicy: RequestPolicy): Promise<T> {
        console.debug(`${this._name}::load()`)

        return new Promise((resolve, reject) => {
            this.state = 'loading'

            this._requestHandle?.()
            this._requestHandle = this.connection.query(this._document, {
                onSuccess: data => {
                    console.debug(`${this._name}::load()..onSuccess`)

                    const projection = this._projection(data)

                    transaction(() => {
                        this.state = 'ready'

                        this.setValue(projection)
                        this._valueAtom.reportChanged()
                    })

                    resolve(projection)
                },
                onFailure: error => {
                    console.debug(`${this._name}::load()..onFailure`)

                    transaction(() => {
                        this.state = 'failed'
                        this.error = error
                    })

                    reject(error)
                }
            }, variables, requestPolicy)
        })
    }

    /**
     * Reload data from connection if someone is observing
     */
    reload(options?: ReloadOptions): Promise<T> {
        console.debug(`${this._name}::reload()`)

        if (!options?.force && !this._valueAtom.isBeingObserved_) {
            // return current value if no one is observing and do not reload data from connection
            return Promise.resolve(this.getValue())
        }

        let variables: TVariables = null

        if (this._variables) {
            variables = this._variables instanceof Function
                ? this._variables()
                : {...this._variables}
        }

        return this.load(variables, options?.requestPolicy || this._defaultReloadPolicy)
    }

    close() {
        console.debug(`${this._name}::close()`)

        this._variablesWatch?.()
        this._variablesWatch = null

        this._requestHandle?.()
        this._requestHandle = null
    }
}
