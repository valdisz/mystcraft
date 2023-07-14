import { observable, makeObservable, computed, runInAction } from 'mobx'
import { TypedDocumentNode } from 'urql'
import { DocumentNode } from 'graphql'
import { RequestPolicy, DataSourceConnection } from './data-source-connection'
import { Operation } from './operation'
import { OperationState } from './operation-state'
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
     * Called when the mutation is successful.
     */
    onSuccess?: ContextCallback<T, TVariables, void>

    /**
     * Called when the mutation fails.
     */
    onFailure?: ContextCallback<TError, TVariables, void>
}

export interface MutationRunOptions {
    requestPolicy?: Option<RequestPolicy>
}

/**
 * Represents a GraphQL mutation operation that can be run multiple times with different variables.
 */
export class Mutation<TData, TVariables extends object, T, TError, TProtocolError> implements Operation<TError> {
    constructor(
        private readonly connection: DataSourceConnection<TData, TVariables, TProtocolError>,
        options: MutationOptions<TData, TVariables, T, TError, TProtocolError>
    ) {
        makeObservable(this, {
            state: observable,
            error: observable,
            value: observable,
            isLoading: computed,
            isReady: computed,
            isFailed: computed
        })

        this._name = options.name ?? 'Mutation'
        this._document = options.document
        this._map = options.map
        this._mapError = options.mapError
        this._defaultRequestPolicy = options.defaultRequestPolicy || RequestPolicy.CacheAndNetwork
        this._onLoad = options.onLoad
        this._onSuccess = options.onSuccess
        this._onFailure = options.onFailure
    }

    private readonly _name: string
    private readonly _defaultRequestPolicy: RequestPolicy
    private readonly _document: DocumentNode | TypedDocumentNode<TData, TVariables>
    private readonly _map: ContextProjection<TData, Option<TVariables>, T>
    private readonly _mapError: ContextProjection<TData, Option<TVariables>, Option<TError>>
    private readonly _mapProtocolError: ContextProjection<TProtocolError, Option<TVariables>, Option<TError>>
    private readonly _onLoad: Callback<TVariables, void>
    private readonly _onSuccess?: ContextCallback<T, TVariables, void>
    private readonly _onFailure?: ContextCallback<TError, TVariables, void>

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
                    this.error = error;

                    (this?._onFailure(error, variables) || Promise.resolve())
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
                        this.value = projection;

                        (this._onSuccess?.(projection, variables) || Promise.resolve())
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
}
