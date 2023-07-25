import { action, computed, makeObservable, observable, runInAction } from 'mobx'
import { FormControlProps, FormHelperTextProps, InputLabelProps, TextFieldProps } from '@mui/material'
import { FieldView } from './form-field'
import { Converter, ValueOrError } from './converters'
import { Validator, combine, pass } from './validation'
import { FileFieldProps, SelectFieldProps, SwitchFieldProps } from '../../components'

export type Option<T> = T | null | undefined

export interface FieldOptions<TRaw, T> {
    converter: Converter<TRaw, T>
    initialValue?: T
    validator?: Validator<T>
    isRequired?: boolean | (() => boolean)
    isReadonly?: boolean | (() => boolean)
    isDisabled?: boolean | (() => boolean)
}

interface InnerState<TRaw> {
    value: TRaw
    dirty: boolean
    touched: boolean
    active: boolean
    valid: boolean
    error: string
}

function _yes() {
    return true
}

function _no() {
    return false
}

/**
 * Converts a boolean or a function that returns a boolean into a function that returns a boolean.
 */
function _asCallable(value: boolean | (() => boolean)) {
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
function _or(a: boolean, b: boolean) {
    return a || b
}

function required<T = unknown>(value: T, error?: string): string | null {
    if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
        return error || 'Required'
    }

    return null
}


export function forFormControl(f: FieldView): FormControlProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        error: f.isErrorVisible
    }
}

export function forInputLabel(f: FieldView): InputLabelProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        error: f.isErrorVisible,
    }
}

export function forFormHelperText(f: FieldView): FormHelperTextProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        error: f.isErrorVisible,
        children: f.error,
    }
}

export function forTextField<T>(f: Field<string, T | null>): TextFieldProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        value: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: e => f.handleChange(e.target.value),
        onBlur: f.handleTouch,
    }
}

export function forFileField(f: Field<File | null, File | null>): FileFieldProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        value: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: f.handleChange,
        onBlur: f.handleTouch,
    }
}

export function forSelectField<T>(f: Field<T | null, T | null>): Partial<SelectFieldProps<T | null>> {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        value: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: f.handleChange,
        onBlur: f.handleTouch,
    }
}

export function forSwitchField(f: Field<boolean | null | undefined, boolean | null>): SwitchFieldProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        checked: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: (event) => f.handleChange(event.target.checked),
        onBlur: f.handleTouch,
    }
}

function _makeValidator<T>(validators?: Validator<T>[]): Validator<T> {
    if (validators?.length ?? 0 === 0) {
        return pass
    }

    if (validators.length === 1) {
        return validators[0]
    }

    return combine(...validators)
}

const _STRING: Converter<string, string> = {
    sanitzie: value => ({ value: value ? value : null }),
    format: value => value ?? ''
}

const _STRING_TRIM: Converter<string, string> = {
    sanitzie: value => ({ value: value ? value?.trim() : null || '' }),
    format: value => value ?? ''
}

const INT: Converter<Option<string>, Option<number>> = {
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


export interface BaseFieldFactoryOptions<TRaw, T> extends Omit<
    FieldOptions<TRaw, T>,
    'converter' | 'validator'
> { }

export interface StringFieldOptions extends BaseFieldFactoryOptions<string, string> {
    trim?: boolean
}

export function text(validators?: Validator<string>[], options?: StringFieldOptions): Field<string | undefined, string | null> {
    const trim = options?.trim ?? false
    return new Field({
        converter: trim ? _STRING_TRIM : _STRING,
        validator: _makeValidator(validators),
        ...options
    })
}

export interface IntFieldOptions extends BaseFieldFactoryOptions<string, number> {
    trim?: boolean
}

export function int(validators?: Validator<number>[], options?: IntFieldOptions): Field<string, number> {
    return new Field({
        converter: INT,
        validator: _makeValidator(validators),
        ...options
    })
}

// TODO: Implemented validation as computed property

export class Field<TRaw, T> implements FieldView {
    constructor({ converter, initialValue, validator, isReadonly, isRequired, isDisabled }: FieldOptions<TRaw, T>) {
        this._converter = converter
        this._inner = observable.object({
            dirty: false,
            touched: false,
            active: false,
            valid: true,
            value: converter.format(initialValue),
            error: ''
        })

        this._isReadonly = _asCallable(isReadonly)
        this._isRequired = _asCallable(isRequired)
        this._isDisabled = _asCallable(isDisabled)
        this._validator = validator

        this.reset(initialValue)

        makeObservable(this, {
            value: computed,
            isDisabled: computed,
            isEnabled: computed,
            isReadonly: computed,
            isWritable: computed,
            isRequired: computed,
            isOptional: computed,
            isDirty: computed,
            isPristine: computed,
            isTouched: computed,
            isUntouched: computed,
            isActive: computed,
            isInactive: computed,
            isValid: computed,
            isInvalid: computed,
            isErrorVisible: computed,
            error: computed,
            reset: action,
            validate: action,
            setValue: action,
            touch: action,
            activate: action,
            deactivate: action,
            handleChange: action,
        })
    }

    private readonly _converter: Converter<TRaw, T>
    private readonly _validator: Validator<T>
    private readonly _isReadonly: () => boolean
    private readonly _isRequired: () => boolean
    private readonly _isDisabled: () => boolean
    private readonly _inner: InnerState<TRaw>

    get value(): ValueOrError<T> {
        const { value, error } = this._converter.sanitzie(this._inner.value)
        if (error) {
            return { error }
        }

        return {
            value,
            error: this._validator(value)
        }
    }

    get rawValue() {
        return this._inner.value
    }

    get isDisabled() {
        return this._isDisabled()
    }

    get isEnabled() {
        return !this.isDisabled
    }

    get isReadonly() {
        return this._isReadonly()
    }

    get isWritable() {
        return !this.isReadonly
    }

    get isRequired() {
        return this._isRequired()
    }

    get isOptional() {
        return !this.isRequired
    }

    get isDirty(): boolean {
        return this.isDisabled ? false : this._inner.dirty
    }

    get isPristine(): boolean {
        return !this.isDirty
    }

    get isTouched(): boolean {
        return this.isDisabled ? false : this._inner.touched
    }

    get isUntouched(): boolean {
        return !this.isTouched
    }

    get isActive(): boolean {
        return this.isDisabled ? false : this._inner.active
    }

    get isInactive(): boolean {
        return !this.isActive
    }

    get isValid(): boolean {
        return this.isDisabled ? true : this._inner.valid
    }

    get isInvalid(): boolean {
        return !this.isValid
    }

    get isErrorVisible() {
        const enabled = this.isEnabled
        const touched = this.isTouched
        const valid = this.isValid

        if (!enabled) {
            return false
        }

        if (valid) {
            return false
        }

        if (!touched) {
            return false
        }

        return true
    }

    get error() {
        return this.isErrorVisible ? this._inner.error : ''
    }

    /**
     * Handles a event when the user interacted (touched) with the field.
     */
    readonly handleTouch = () => this.touch()

    /**
     * Handles a change event from the input.
     * This will trigger validation, update the dirty state, and change the touched state.
     */
    readonly handleChange = (value: any) => {
        this._inner.touched = true
        this.setValue(value)
    }

    /**
     * Sets the value of the field programmatically.
     * This will trigger validation, and update the dirty state, but will not change touched state.
     */
    setValue(input: TRaw) {
        this._inner.value = input
        const { error, value } = this._converter.sanitzie(input)

        if (error) {
            this.value = null

            this._inner.error = error
            this._inner.dirty = true
            this._inner.valid = false

            return
        }

        this._inner.dirty = this._inner.dirty || this.value !== value
        this.value = value

        this.validate()
    }

    /**
     * Resets the field to its initial state.
     * Optionally, a new value can be provided.
     */
    reset(value?: T) {
        this.value = {
            value,
            error: null
        }

        this._inner.value = this._converter.format(this.value.value)
        this.validate()

        this._inner.dirty = false
        this._inner.touched = false
        this._inner.active = false
    }

    validate() {
        if (this.isDisabled){
            return true
        }

        this._inner.error = this._validator(this.value)
        this._inner.valid = !this.error

        return this._inner.valid
    }

    touch() {
        if (this.isDisabled) {
            return
        }

        this._inner.touched = true
    }

    activate() {
        if (this.isDisabled) {
            return
        }

        this._inner.active = true
    }

    deactivate() {
        if (this.isDisabled) {
            return
        }

        this._inner.active = false
    }
}
