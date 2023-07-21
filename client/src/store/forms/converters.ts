export interface ValueOrError<T> {
    error?: string
    value?: T
}

export interface Converter<T> {
    sanitzie(value: string): ValueOrError<T>
    format(value: T | null): string
}


const _STRING: Converter<string> = {
    sanitzie: value => ({ value }),
    format: value => value || ''
}

const _STRING_TRIM: Converter<string> = {
    sanitzie: value => ({ value: value?.trim() || '' }),
    format: value => value || ''
}

export function STRING(trim?: boolean) {
    return trim ? _STRING_TRIM : _STRING
}

export const INT: Converter<number> = {
    sanitzie: s => {
        if (!s) {
            return { value: null }
        }

        const value = parseInt(s)
        if (isNaN(value)) {
            return { error: 'Value is not a whole number' }
        }

        if (!isFinite(value)) {
            return { error: 'Value is out of bounds' }
        }

        return { value }
    },
    format: value => value?.toString() || ''
}

export const REAL: Converter<number> = {
    sanitzie: s => {
        if (!s) {
            return { value: null }
        }

        const value = parseFloat(s)
        if (isNaN(value)) {
            return { error: 'Value is not a fraction or whole number' }
        }

        if (!isFinite(value)) {
            return { error: 'Value is out of bounds' }
        }

        return { value }
    },
    format: value => value?.toString() || ''
}
