import { DocumentNode } from 'graphql'
import { CombinedError, TypedDocumentNode } from 'urql'
import { SeqDataSource } from './array-data-source'
import { VariablesGetter } from './variables-getter'
import { ObjectDataSource } from './object-data-source'
import { UrqlConnection } from './urql-connection'
import { Mutation, MutationOptions } from './mutation'
import client from './client'
import { MutationResult } from '../../schema'
import { Option, OperationError, Projection, ContextProjection } from './types'
import { Operation } from './operation'
import { OperationState } from './operation-state'
import { computed, makeObservable } from 'mobx'

export function query<TData, T extends object, TVariables extends object = { }>(
    document: DocumentNode | TypedDocumentNode<TData, TVariables>,
    projection: Projection<TData, T>,
    variables?: TVariables | VariablesGetter<TVariables>,
    name?: string
) {
    const conn = new UrqlConnection<TData, TVariables>(client)
    const ds = new ObjectDataSource<T, TData, TVariables, CombinedError>(conn, {
        document,
        projection,
        variables,
        name,
        initialValue: null,
    })

    return ds
}

export function seq<TData, T, TVariables extends object = { }>(
    document: DocumentNode | TypedDocumentNode<TData, TVariables>,
    projection: Projection<TData, T[]>,
    variables?: TVariables | VariablesGetter<TVariables>,
    name?: string
) {
    const conn = new UrqlConnection<TData, TVariables>(client)
    const ds = new SeqDataSource<T, TData, TVariables, CombinedError>(conn, {
        document,
        projection,
        variables,
        name,
        initialValue: [ ],
    })

    return ds
}

export function mapMutationError<T>(selector: (data: T) => MutationResult): ((data: T) => Option<OperationError> ) {
    return (data: T) => {
        const result = selector(data)
        if (result.isSuccess) {
            return null
        }

        return { message: result.error }
    }
}

export function mapProtocolError(error: CombinedError): Option<OperationError> {
    if (error.networkError) {
        return { message: error.networkError.message }
    }

    if (error.graphQLErrors.length > 0) {
        return { message: error.graphQLErrors[0].message }
    }

    return null
}

export function noop(...args: any): any {
}

export type MutationFactoryOptions<TData, TVariables extends object, TResult extends MutationResult, T> =
    Omit<
        MutationOptions<TData, TVariables, T, OperationError, CombinedError>,
        'mapProtocolError' | 'mapError' | 'map'
    > & {
        /**
         * Select the restult of the mutation from the mutation data
         */
        pick: Projection<TData, TResult>

        /**
         * Map the mutation result to the desired type
         */
        map?: ContextProjection<TResult, TVariables, T>
    }

export function mutate<TData, TVariables extends object, TResult extends MutationResult, T>(
    { map, pick, ...options }: MutationFactoryOptions<TData, TVariables, TResult, T>
): Mutation<TData, TVariables, T, OperationError, CombinedError> {
    const conn = new UrqlConnection<TData, TVariables>(client)
    return new Mutation<TData, TVariables, T, OperationError, CombinedError>(conn, {
        ...options,
        map: map ? (d, c) => map(pick(d), c) : noop,
        mapError: mapMutationError<TData>(pick),
        mapProtocolError
    })
}

class CombinedOperation<TError> implements Operation<TError> {
    constructor(private readonly operations: Operation<TError>[]) {
        makeObservable(this, {
            state: computed,
            isLoading: computed,
            isReady: computed,
            isFailed: computed,
            error: computed,
        })
    }

    get state() {
        const isLoading = this.operations.some(o => o.isLoading)
        const isReady = this.operations.every(o => o.isReady)
        const isFailed = this.operations.some(o => o.isFailed)

        if (isLoading) {
            return 'loading' as OperationState
        }

        if (isFailed) {
            return 'failed' as OperationState
        }

        if (isReady) {
            return 'failed' as OperationState
        }

        return 'unspecified'
    }

    get isLoading() {
        return this.operations.some(o => o.isLoading)
    }

    get isReady() {
        return this.operations.every(o => o.isReady)
    }

    get isFailed() {
        return this.operations.some(o => o.isFailed)
    }

    get error() {
        return this.operations.find(o => o.isFailed)?.error ?? null
    }

    reset() {
        this.operations.forEach(o => o.reset())
    }
}

export function combine<TError = OperationError>(...operations: Operation<TError>[]): Operation<TError> {
    return new CombinedOperation(operations || [])
}
