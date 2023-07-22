import { action, computed, makeObservable, observable } from 'mobx'
import { FormControlProps, FormHelperTextProps, InputLabelProps, TextFieldProps } from '@mui/material'
import { FormField } from './form-field'
import { Converter } from './converters'
import { Validator, combine, required, pass } from './validation'
import { FileFieldProps, SelectFieldProps } from '../../components'

export class Field<T> implements FormField {
    constructor(private converter: Converter<T>, value: T | null, private isRequired?: boolean, ...validators: Validator<T>[]) {
        this.value = value
        this.rawValue = converter.format(value)
        this._validator = isRequired
            ? combine(required(), ...validators)
            : combine(...validators, pass)

        makeObservable(this)
    }

    private readonly _validator: Validator<T>
    private readonly _inner = observable.object({
        dirty: false,
        touched: false,
        valid: true,
    })

    @observable value: T | null = null
    @observable rawValue: any = null
    @observable disabled: boolean = false

    @computed get dirty(): boolean {
        return this.disabled ? false :this._inner.dirty
    }

    @computed get touched(): boolean {
        return this.disabled ? false : this._inner.touched
    }

    @computed get valid(): boolean {
        return this.disabled ? true : this._inner.valid
    }

    @observable error: string = ''

    @computed get showError() {
        return !this.disabled && (this.dirty || this.touched) && !this.valid
    }

    @computed get errorText() {
        return this.showError ? this.error : ''
    }

    get forFormControl(): FormControlProps {
        return {
            required: this.isRequired,
            error: this.showError
        }
    }

    get forInputLabel(): InputLabelProps {
        return {
            required: this.isRequired,
        }
    }

    get forFormHelperText(): FormHelperTextProps {
        return {
            error: this.showError,
            children: this.errorText,
        }
    }

    get forTextField(): TextFieldProps {
        return {
            required: this.isRequired,
            value: this.rawValue,
            error: this.showError,
            helperText: this.errorText,
            onChange: e => this.change(e.target.value),
            onBlur: this.handleTouch,
        }
    }

    get forFileField(): FileFieldProps {
        return {
            required: this.isRequired,
            value: this.rawValue,
            error: this.showError,
            helperText: this.errorText,
            onChange: file => this.change(file),
            onBlur: this.handleTouch,
        }
    }

    get forSelectField(): Partial<SelectFieldProps<T>> {
        return {
            required: this.isRequired,
            value: this.rawValue,
            error: this.showError,
            helperText: this.errorText,
            onChange: value => this.change(value),
            onBlur: this.handleTouch,
        }
    }

    private readonly handleTouch = () => this.touch()

    @action reset(value?: T | null) {
        this.value = value || null
        this.rawValue = this.converter.format(this.value)
        this._inner.dirty = false
        this._inner.touched = false
        this._inner.valid = true
        this.error = ''
    }

    @action validate() {
        this.error = this._validator?.(this.value)
        this._inner.valid = !this.error

        return this._inner.valid
    }

    @action setValue(input: any) {
        this.rawValue = input

        const { error, value } = this.converter.sanitzie(input)
        if (error) {
            this.error = error
            this.value = null

            this._inner.dirty = true
            this._inner.valid = false

            return
        }

        this._inner.dirty = this.dirty || this.value !== value
        this.value = value

        this.validate()
    }

    @action change(value: any) {
        this._inner.touched = true
        this.setValue(value)
    }

    @action touch() {
        if (this.disabled) {
            return
        }

        if (!this._inner.touched) {
            this.validate()
        }

        this._inner.touched = true
    }

    @action disable() {
        this.disabled = true
        this._inner.dirty = false
        this._inner.touched = false
        this._inner.valid = true
    }

    @action enable() {
        this.disabled = false
        this._inner.dirty = false
        this._inner.touched = false
        this._inner.valid = true
    }
}
