import { OperationState } from './operation-state'
import { OperationError } from './types'

/**
 * Represents a data source that can be observed
 */
export interface Operation<TError = OperationError> {
    readonly state: OperationState
    readonly isLoading: boolean
    readonly isReady: boolean
    readonly isFailed: boolean
    readonly error: TError | null

    reset(): void
}
