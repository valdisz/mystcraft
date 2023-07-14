import { OperationState } from './operation-state';

/**
 * Represents a data source that can be observed
 */
export interface Operation<TError> {
    readonly state: OperationState;
    readonly isLoading: boolean;
    readonly isReady: boolean;
    readonly isFailed: boolean;
    readonly error: TError | null;
}
