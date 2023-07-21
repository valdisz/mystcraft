export interface Validator<T> {
    (value: T): string | null
}

export function minLen(len: number, error?: string): Validator<string> {
    return (value: string) => (value?.length ?? 0) < len ? error || `Must be at least ${len} symbols` : null
}

export function maxLen(len: number, error?: string): Validator<string> {
    return (value: string) => (value?.length ?? 0) > len ? error || `Must be no more tha ${len} symbols` : null
}

export function min(minValue: number, error?: string): Validator<number> {
    return (value: number) => value < minValue ? error || `Must be greater or equal to ${minValue}` : null
}

export function max(maxValue: number, error?: string): Validator<number> {
    return (value: number) => value > maxValue ? error || `Must be less or equal to ${maxValue}` : null
}

export function minCount<T>(count: number, error?: string): Validator<T[]> {
    return (value: T[]) => (value?.length ?? 0) < count ? error || `Must be at least ${count} items` : null
}

export function maxCount<T>(count: number, error?: string): Validator<T[]> {
    return (value: T[]) => (value?.length ?? 0) > count ? error || `Must be no more tha ${count} items` : null
}

export function required<T>(error?: string): Validator<T> {
    return (value: T) => {
        if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
            return error || 'Required'
        }

        return null
    }
}

export function noValidate() {
    return null
}

export function combine<T>(...validators: Validator<T>[]): Validator<T> {
    return (value: T) => {
        for (var v of validators) {
            const error = v(value)
            if (error) {
                return error
            }
        }

        return null
    }
}
