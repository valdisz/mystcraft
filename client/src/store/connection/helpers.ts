import { DocumentNode } from 'graphql'
import { CombinedError, TypedDocumentNode, OperationResult, OperationContext } from 'urql'
import { ArrayDataSource } from './array-data-source'
import { DataSource, Projection, VariablesGetter } from './data-source'
import { ObjectDataSource } from './object-data-source'
import { UrqlConnection } from './urql-connection'
import client from './client'
import { RequestPolicy } from './data-source-connection'

export function query<TData, T extends object, TVariables extends object = { }>(
    document: DocumentNode | TypedDocumentNode<TData, TVariables>,
    projection: Projection<TData, T>,
    variables?: VariablesGetter<TVariables>,
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

export function querySeq<TData, T, TVariables extends object = { }>(
    document: DocumentNode | TypedDocumentNode<TData, TVariables>,
    projection: Projection<TData, T[]>,
    variables?: VariablesGetter<TVariables>,
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

export interface MutationOptions extends Partial<OperationContext> {
    refetch?: DataSource<any, any, any, any>[],
    refetchRequestPolicy?: RequestPolicy
}

export async function mutate<TData, TVariables extends object = { }>(
    document: DocumentNode | TypedDocumentNode<TData, TVariables>,
    variables?: TVariables,
    options?: MutationOptions
): Promise<OperationResult<TData, TVariables>> {
    const { refetch, refetchRequestPolicy, ...context } = options || { }

    const response = await client.mutation(document, variables, context).toPromise()

    if (refetch?.length > 0) {
        const tasks = []

        for (const source of refetch) {
            tasks.push(source.reload(refetchRequestPolicy))
        }

        await Promise.all(tasks)
    }

    return response
}
