import { Option } from './utils'

export interface ValueOrError<T> {
    error?: string
    value?: T
}

export interface Converter<TRaw, T> {
    sanitize(value: TRaw): ValueOrError<T>
    format(value: T): TRaw
}

export const STRING: Converter<string, string> = {
    sanitize: value => ({ value: value ? value : null }),
    format: value => value ?? ''
}

export const STRING_TRIM: Converter<string, string> = {
    sanitize: value => ({ value: value ? value?.trim() : null || '' }),
    format: value => value ?? ''
}

export const INT: Converter<Option<string>, Option<number>> = {
    sanitize: s => {
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

export const REAL: Converter<Option<string>, Option<number>> = {
    sanitize: s => {
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

export const FILE: Converter<Option<File>, Option<File>> = {
    sanitize: (value: File) => ({ value: value === undefined ? null : value }),
    format: value => value
}

export const BOOLEAN: Converter<Option<boolean | string>, Option<boolean>> = {
    sanitize: value => {
        if (value === null || value === undefined) {
            return { value: null }
        }

        if (typeof value === 'boolean') {
            return { value }
        }

        switch (value.toLowerCase().trim()) {
            case 'true':
            case 'yes':
            case 'on':
            case '1':
                return { value: true }

            case 'false':
            case 'off':
            case 'no':
            case '0':
              return { value: false }

            default: {
                try {
                    const parsed = JSON.parse(value)
                    return { value: !!parsed }
                }
                catch (e) {
                    return { error: 'Value is not a boolean' }
                }

            }
        }
    },
    format: value => value
}
