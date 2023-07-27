import * as ValidatorJS from 'validatorjs'
import cronstrue from 'cronstrue'
import { Option } from './utils'

export interface Validator<T> {
    (value: T): string | null
}

export type OneOrManyValidators<T> = Validator<T> | Validator<T>[]

export function makeValidator<T>(validators?: OneOrManyValidators<T>): Validator<T> {
    if (!validators) {
        return pass
    }

    if (Array.isArray(validators)) {
        if (validators.length === 0) {
            return pass
        }

        if (validators.length === 1) {
            return validators[0]
        }

        return combine(...validators)
    }

    return validators
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

export function rule(rule: string, error?: string): Validator<Option<string>> {
    return (value: Option<string>) => {
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
