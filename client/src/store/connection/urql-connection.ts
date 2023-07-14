import { Client, TypedDocumentNode, createRequest, OperationResult, CombinedError, RequestPolicy as UrlRequestPolicy } from 'urql'
import { DocumentNode } from 'graphql'
import { pipe, Source, subscribe } from 'wonka'
import { ResponseHandler } from './data-source'
import { DataSourceConnection, Disposable, RequestPolicy } from './data-source-connection'

function mapRequestPolicy(requestPolicy?: RequestPolicy): UrlRequestPolicy {
    switch (requestPolicy) {
        case RequestPolicy.CacheFirst: return 'cache-first'
        case RequestPolicy.CacheAndNetwork: return 'cache-and-network'
        case RequestPolicy.CacheOnly: return 'cache-only'
        case RequestPolicy.NetworkOnly: return 'network-only'
        default: return 'cache-first'
    }
}

export class UrqlConnection<TData, TVariables extends object> implements DataSourceConnection<TData, TVariables, CombinedError> {
    constructor(private readonly client: Client) {
    }

    query(
        query: DocumentNode | TypedDocumentNode<TData, TVariables>,
        { onSuccess, onFailure }: ResponseHandler<TData, TVariables, CombinedError>,
        variables: TVariables,
        requestPolicy: RequestPolicy
    ): Disposable {
        const request = createRequest(query, variables)

        const source: Source<OperationResult<TData, TVariables>> = this.client.executeQuery(request, {
            requestPolicy: mapRequestPolicy(requestPolicy)
        })

        const { unsubscribe } = pipe(
            source,
            subscribe(result => {
                try {
                    if (result.error) {
                        onFailure(result.error, result)
                    }
                    else {
                        onSuccess(result.data, result);
                    }
                }
                catch (err) {
                    onFailure(new CombinedError({ networkError: err as Error }))
                }
            })
        )

        return () => unsubscribe()
    }

    mutate(
        mutation: DocumentNode | TypedDocumentNode<TData, TVariables>,
        { onSuccess, onFailure }: ResponseHandler<TData, TVariables, CombinedError>,
        variables: TVariables,
        requestPolicy: RequestPolicy): Disposable {

        const request = createRequest(mutation, variables)

        const source: Source<OperationResult<TData, TVariables>> = this.client.executeMutation(request, {
            requestPolicy: mapRequestPolicy(requestPolicy)
        })

        // for capturing unsubscribe function
        const memo = {
            unsubscribe: null as (() => void)
        }

        const { unsubscribe } = pipe(
            source,
            subscribe(result => {
                try {
                    if (result.error) {
                        onFailure(result.error, result)
                    }
                    else {
                        onSuccess(result.data, result);
                    }
                }
                catch (err) {
                    onFailure(new CombinedError({ networkError: err as Error }))
                }
                finally {
                    memo.unsubscribe?.()
                }
            })
        )
        memo.unsubscribe = unsubscribe

        return () => unsubscribe()
    }
}
