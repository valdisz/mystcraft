export type Option<T> = T | null | undefined

export function _yes() {
    return true
}

export function _no() {
    return false
}

/**
 * Converts a boolean or a function that returns a boolean into a function that returns a boolean.
 */
export function _asCallable(value: boolean | (() => boolean)) {
    return typeof value === 'function'
        ? value
        : (
            value ? _yes : _no
        )
}

/**
 * Checks if one of the sides of the OR operation is true, greedily.
 * Will help MobX change detection to capture both sides of the OR operation.
 */
export function _or(a: boolean, b: boolean, ...rest: boolean[]) {
    return a || b || rest.some(x => x)
}

/**
 * Checks if both sides of the AND operation are true, greedily.
 * Will help MobX change detection to capture both sides of the AND operation.
 */
export function _and(a: boolean, b: boolean, ...rest: boolean[]) {
    return a && b && rest.every(x => x)
}

/**
 * Checks if any of the items in the array matches the predicate, greedily.
 */
export function _some<T>(items: T[], predicate: (item: T) => boolean) {
    return items.reduce((acc, item) => acc || predicate(item), false)
}

/**
 * Checks if all of the items in the array matches the predicate, greedily.
 */
export function _every<T>(items: T[], predicate: (item: T) => boolean) {
    return items.reduce((acc, item) => acc && predicate(item), true)
}

export function isEmpty<T>(value: T): boolean {
    if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
        return true
    }

    return false
}
