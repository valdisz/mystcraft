import { observable, makeObservable, computed, runInAction, action } from 'mobx'
import { TypedDocumentNode } from 'urql'
import { DocumentNode } from 'graphql'
import { RequestPolicy, DataSourceConnection } from './data-source-connection'
import { Operation, OperationState } from './operation'
import { Option, Callback, ContextCallback, ContextProjection } from './types'


export interface MutationOptions<TData, TVariables extends object, T, TError, TProtocolError> {
    /**
     * GraphQL document to use for the operation.
     */
    document: DocumentNode | TypedDocumentNode<TData, Option<TVariables>>

    /**
     * Projection that will transform the GraphQL response into the desired value.
     */
    map: ContextProjection<TData, Option<TVariables>, T>

    /**
     * Projection that will examine the GraphQL response and return an error if any.
     */
    mapError: ContextProjection<TData, Option<TVariables>, Option<TError>>

    /**
     * Projection that will transform protocol errors and exceptions into the desired error value.
     */
    mapProtocolError: ContextProjection<TProtocolError, Option<TVariables>, Option<TError>>

    /**
     * Name of the data source (used for debugging).
     */
    name?: string

    /**
     * Default request policy to use for the operation.
     */
    defaultRequestPolicy?: RequestPolicy

    /**
     * Called when the operation is started.
     */
    onLoad?: Callback<TVariables, void>

    /**
     * List of callbacks that will be called when the operation succeeds.
     */
    onSuccess?: ContextCallback<T, TVariables, any>[]

    /**
     * List of callbacks that will be called when the operation fails.
     */
    onFailure?: ContextCallback<TError, TVariables, any>[]
}

export interface MutationRunOptions {
    requestPolicy?: Option<RequestPolicy>
}

function execList<A, B>(arg1: A, arg2: B, list?: ContextCallback<A, B, any>[]): Promise<any> {
    if (!list) {
        return Promise.resolve()
    }

    const effects = []
    for (const effect of list) {
        effects.push(effect(arg1, arg2) || Promise.resolve())
    }

    return Promise.all(effects)
}

/**
 * Represents a runnable operation that can be run multiple times with different variables.
 */
export interface Runnable<TVariables extends object, T = unknown> {
    /**
     * Runs the operation with the specified variables and returns a promise that resolves to the result.
     */
    run(variables: Option<TVariables>, options?: MutationRunOptions): Promise<T>
}

/**
 * Represents a GraphQL mutation operation that can be run multiple times with different variables.
 */
export class Mutation<TData, TVariables extends object, T, TError, TProtocolError> implements Operation<TError>, Runnable<TVariables, T> {
    constructor(
        private readonly connection: DataSourceConnection<TData, TVariables, TProtocolError>,
        options: MutationOptions<TData, TVariables, T, TError, TProtocolError>
    ) {
        makeObservable(this, {
            state: observable,
            error: observable,
            value: observable,
            isLoading: computed,
            isIdle: computed,
            isReady: computed,
            isFailed: computed,
            reset: action
        })

        this._name = options.name ?? 'Mutation'
        this._document = options.document
        this._map = options.map
        this._mapError = options.mapError
        this._defaultRequestPolicy = options.defaultRequestPolicy || RequestPolicy.CacheAndNetwork
        this._onLoad = options.onLoad
        this._onSuccess = options.onSuccess
        this._onFailure = options.onFailure
        this._mapProtocolError = options.mapProtocolError
    }

    private readonly _name: string
    private readonly _defaultRequestPolicy: RequestPolicy
    private readonly _document: DocumentNode | TypedDocumentNode<TData, TVariables>
    private readonly _map: ContextProjection<TData, Option<TVariables>, T>
    private readonly _mapError: ContextProjection<TData, Option<TVariables>, Option<TError>>
    private readonly _mapProtocolError: ContextProjection<TProtocolError, Option<TVariables>, Option<TError>>
    private readonly _onLoad: Callback<TVariables, void>
    private readonly _onSuccess?: ContextCallback<T, TVariables, any>[]
    private readonly _onFailure?: ContextCallback<TError, TVariables, any>[]

    /**
     * Current state of the operation
     */
    state: OperationState = 'unspecified'

    /**
     * Last error that occurred while running the operation. Error is cleared when the operation is run again.
     */
    error: Option<TError> = null

    /**
     * Last value returned by the operation. Value is cleared when the operation is run again.
     */
    value: Option<T> = null

    get isLoading() {
        return this.state === 'loading'
    }

    get isReady() {
        return this.state === 'ready'
    }

    get isFailed() {
        return this.state === 'failed'
    }

    get isIdle() {
        return !this.isLoading
    }

    /**
     * Runs the operation with the specified variables and returns a promise that resolves to the result.
     */
    run(variables: Option<TVariables>, options?: MutationRunOptions): Promise<T> {
        runInAction(() => {
            this.state = 'loading'
            this.error = null
            this.value = null

            this._onLoad?.(variables)
        })

        return new Promise<T>((resolve, reject) => {
            const handleFailure = (error: TError) => {
                runInAction(() => {
                    this.state = 'failed'
                    this.error = error

                    execList(error, variables, this?._onFailure)
                        .finally(() => reject(error))
                })
            }

            this.connection.mutate(this._document, {
                onSuccess: data => {
                    console.debug(`${this._name}::run()..onSuccess`)

                    const err = this._mapError(data, variables)
                    if (err) {
                        handleFailure(err)
                        return
                    }

                    const projection = this._map(data, variables)

                    runInAction(() => {
                        this.state = 'ready'
                        this.value = projection

                        execList(projection, variables, this?._onSuccess)
                            .finally(() => resolve(projection))
                    })
                },
                onFailure: error => {
                    console.debug(`${this._name}::run()..onFailure`)
                    handleFailure(this._mapProtocolError(error, variables))
                }
            }, variables, options?.requestPolicy || this._defaultRequestPolicy)
        })
    }

    reset(): void {
        this.state = 'unspecified'
        this.error = null
        this.value = null
    }
}
