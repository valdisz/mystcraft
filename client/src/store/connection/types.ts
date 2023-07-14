export type Option<T> = T | null | undefined

export type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>

export type OptionPromise<T> = Option<Promise<T>> | void

export interface Projection<S, D> {
    (source: S): D;
}

export interface ContextProjection<S, C, D> {
    (source: S, context: C): D;
}

export interface Callback<R, O> {
    (result: R): OptionPromise<O>
}

export interface ContextCallback<R, C, O> {
    (result: R, context: C): OptionPromise<O>
}

export interface OperationError {
    message: string
}
