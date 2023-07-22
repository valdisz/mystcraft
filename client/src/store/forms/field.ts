import { action, computed, makeObservable, observable } from 'mobx'
import { FormControlProps, FormHelperTextProps, InputLabelProps, SelectProps, TextFieldProps } from '@mui/material'
import { FormField } from './form-field'
import { Converter } from './converters'
import { Validator, combine, required, noValidate } from './validation'
import { FileFieldProps, SelectFieldProps } from '../../components'

export class Field<T> implements FormField {
    constructor(private converter: Converter<T>, value: T | null, private isRequired?: boolean, ...validators: Validator<T>[]) {
        this.value = value
        this.rawValue = converter.format(value)
        this.validator = isRequired
            ? combine(required(), ...validators, noValidate)
            : combine(...validators, noValidate)

        makeObservable(this)
    }

    private readonly validator: Validator<T>

    @observable value: T | null = null
    @observable rawValue: any = null

    @observable dirty: boolean = false
    @observable touched: boolean = false
    @observable valid: boolean = true
    @observable error: string = ''

    @computed get showError() {
        return this.shouldShowError()
    }

    @computed get errorText() {
        return this.shouldShowError() ? this.error : ''
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

    get forSelect(): SelectProps {
        return {
            value: this.rawValue,
            error: this.showError,
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

    private shouldShowError() {
        return (this.dirty || this.touched) && !this.valid
    }

    @action reset(value?: T | null) {
        this.value = value || null
        this.rawValue = this.converter.format(this.value)
        this.dirty = false
        this.touched = false
        this.valid = true
        this.error = ''
    }

    @action validate() {
        this.error = this.validator?.(this.value)
        this.valid = !this.error

        return this.valid
    }

    @action setValue(input: any) {
        this.rawValue = input

        const { error, value } = this.converter.sanitzie(input)
        if (error) {
            this.error = error
            this.value = null

            this.dirty = true
            this.valid = false

            return
        }

        this.dirty = this.dirty || this.value !== value
        this.value = value

        this.validate()
    }

    @action change(value: any) {
        this.touched = true
        this.setValue(value)
    }

    @action touch() {
        if (!this.touched) {
            this.validate()
        }

        this.touched = true
    }
}
