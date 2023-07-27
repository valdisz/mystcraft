import { OperationError } from './types'

export type OperationState = 'loading' | 'ready' | 'failed' | 'unspecified'

/**
 * Represents a data source that can be observed
 */
export interface Operation<TError = OperationError> {
    readonly state: OperationState
    readonly isLoading: boolean
    readonly isIdle: boolean
    readonly isReady: boolean
    readonly isFailed: boolean
    readonly error: TError | null

    reset(): void
}
