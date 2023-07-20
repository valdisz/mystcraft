import { DocumentNode } from 'graphql'
import { CombinedError, TypedDocumentNode } from 'urql'
import { ArrayDataSource } from './array-data-source'
import { VariablesGetter } from './variables-getter'
import { ObjectDataSource } from './object-data-source'
import { UrqlConnection } from './urql-connection'
import { Mutation, MutationOptions } from './mutation'
import client from './client'
import { MutationResult } from '../../schema'
import { Option, OperationError, Projection, ContextProjection } from './types'

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
    const ds = new ArrayDataSource<T, TData, TVariables, CombinedError>(conn, {
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
