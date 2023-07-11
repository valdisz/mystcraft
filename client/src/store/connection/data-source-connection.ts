import { TypedDocumentNode } from 'urql'
import { DocumentNode } from 'graphql'
import { ResponseHandler } from './data-source'

export enum RequestPolicy {
    CacheFirst,
    CacheAndNetwork,
    NetworkOnly,
    CacheOnly
}

export interface Disposable {
    (): void
}

export interface DataSourceConnection<TData, TVariables extends object, TError = unknown> {
    query(query: DocumentNode | TypedDocumentNode<TData, TVariables>, onResponse: ResponseHandler<TData, TVariables, TError>, variables: TVariables, requestPolicy: RequestPolicy): Disposable
}
