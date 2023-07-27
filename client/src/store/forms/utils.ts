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
 * So that MobX change detection captures boths sides of the OR operation, we need to wrap it into a function.
 * This will evaluate both sides of the OR operation, and then return the result.
 */
export function _or(a: boolean, b: boolean) {
    return a || b
}

export function _and(a: boolean, b: boolean) {
    return a && b
}

export function _some<T>(items: T[], predicate: (item: T) => boolean) {
    return items.reduce((acc, item) => acc || predicate(item), false)
}

export function _every<T>(items: T[], predicate: (item: T) => boolean) {
    return items.reduce((acc, item) => acc && predicate(item), true)
}

export function _isEmpty<T>(value: T): boolean {
    if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
        return true
    }

    return false
}
