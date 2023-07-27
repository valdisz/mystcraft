import { IAtom, IObservableValue, action, computed, createAtom, makeObservable, observable, runInAction } from 'mobx'
import { Converter, ValueOrError } from './converters'
import { Validator } from './validation'
import { _and, _asCallable, _or, _isEmpty } from './utils'

export interface FieldView {
    readonly isDisabled: boolean
    readonly isEnabled: boolean
    readonly isReadonly: boolean
    readonly isWritable: boolean
    readonly isRequired: boolean
    readonly isOptional: boolean
    readonly isDirty: boolean
    readonly isPristine: boolean
    readonly isTouched: boolean
    readonly isUntouched: boolean
    readonly isActive: boolean
    readonly isInactive: boolean
    readonly isEmpty: boolean
    readonly isNotEmpty: boolean
    readonly isValid: boolean
    readonly isInvalid: boolean
    readonly isErrorVisible: boolean
    readonly error: string

    readonly value: any

    reset(): void
    touch(): void
    activate(): void
    deactivate(): void
    setParent(parent: FieldView): void
}

export interface FieldOptions<TRaw, T> {
    converter: Converter<TRaw, T>
    initialValue?: T
    validator?: Validator<T>
    isRequired?: boolean | (() => boolean)
    isReadonly?: boolean | (() => boolean)
    isDisabled?: boolean | (() => boolean)
}



interface FieldInnerState<TRaw> {
    value: TRaw
    dirty: boolean
    touched: boolean
    active: boolean
}

export class Field<TRaw, T> implements FieldView {
    constructor({ converter, initialValue, validator, isReadonly, isRequired, isDisabled }: FieldOptions<TRaw, T>) {
        this._converter = converter
        this._inner = observable.object({
            dirty: false,
            touched: false,
            active: false,
            value: converter.format(initialValue),
        })

        this._parent = observable.box(null, { deep: false })

        this._isReadonly = _asCallable(isReadonly)
        this._isRequired = _asCallable(isRequired)
        this._isDisabled = _asCallable(isDisabled)
        this._validator = validator

        makeObservable(this, {
            setParent: action,
            state: computed,
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
            isEmpty: computed,
            isNotEmpty: computed,
            isValid: computed,
            isInvalid: computed,
            isErrorVisible: computed,
            error: computed,
            reset: action,
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
    private readonly _inner: FieldInnerState<TRaw>

    private _parent: IObservableValue<FieldView>

    setParent(parent: FieldView) {
        this._parent.set(parent)
    }

    get state(): ValueOrError<T> {
        const { value, error } = this._converter.sanitize(this._inner.value)
        const required = this._isRequired()

        if (error) {
            return { error }
        }

        if (required && _isEmpty(value)) {
            return {
                value,
                error: 'Required'
            }
        }

        return {
            value,
            error: this._validator(value)
        }
    }

    get value() {
        const { value, error} = this.state
        return error ? null : value
    }

    get rawValue() {
        return this._inner.value
    }

    get isDisabled() {
        return _or(this._parent.get()?.isDisabled ?? false, this._isDisabled())
    }

    get isEnabled() {
        return !this.isDisabled
    }

    get isReadonly() {
        return _or(this._parent.get()?.isReadonly ?? false, _or(this.isDisabled, this._isReadonly()))
    }

    get isWritable() {
        return !this.isReadonly
    }

    get isRequired() {
        return _or(this._parent.get()?.isRequired ?? false, _and(!this.isDisabled, this._isRequired()))
    }

    get isOptional() {
        return !this.isRequired
    }

    get isDirty(): boolean {
        return _and(!this.isDisabled, this._inner.dirty)
    }

    get isPristine(): boolean {
        return !this.isDirty
    }

    get isTouched(): boolean {
        return _and(!this.isDisabled, this._inner.touched)
    }

    get isUntouched(): boolean {
        return !this.isTouched
    }

    get isActive(): boolean {
        return _and(!this.isDisabled, this._inner.active)
    }

    get isInactive(): boolean {
        return !this.isActive
    }

    get isEmpty(): boolean {
        return _isEmpty(this.value)
    }

    get isNotEmpty(): boolean {
        return !this.isEmpty
    }

    get isValid(): boolean {
        return _or(this.isDisabled, this.state.error === null)
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
        const err = this.state.error
        return this.isErrorVisible ? err : ''
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
        const changed = this._inner.value !== input
        if (!changed) {
            return
        }

        this._inner.value = input
        this._inner.dirty = true
    }

    /**
     * Resets the field to its initial state.
     * Optionally, a new value can be provided.
     */
    reset(value?: T) {
        this._inner.value = this._converter.format(value)
        this._inner.dirty = false
        this._inner.touched = false
        this._inner.active = false
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
