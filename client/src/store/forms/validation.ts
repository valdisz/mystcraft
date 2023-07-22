import * as ValidatorJS from 'validatorjs'
import cronstrue from 'cronstrue'

export interface Validator<T> {
    (value: T): string | null
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

export function pass() {
    return null
}

export function required<T>(error?: string): Validator<T> {
    return (value: T) => {
        if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
            return error || 'Required'
        }

        return null
    }
}

export function rule<T = unknown>(rule: string, error?: string): Validator<T> {
    return (value: T) => {
        const validation = new ValidatorJS({ value }, { value: rule });

        if (validation.fails()) {
            return error ?? validation.errors.first('value')
        }

        return null
    }
}

export function isCron(error?: string): Validator<string> {
    return (value: string) => {
        if (!value) {
            return null
        }

        try {
            cronstrue.toString(value)
        }
        catch (e) {
            if (typeof e === 'string') {
                return e
            }

            return error ?? (e as Error)?.message ?? 'Invalid cron expression'
        }

        return null
    }
}
