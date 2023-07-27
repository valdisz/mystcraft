import { FormControlProps, FormHelperTextProps, InputLabelProps, TextFieldProps } from '@mui/material'
import { Field, FieldView } from './field'
import { _or } from './utils'
import { FileFieldProps, SelectFieldProps, SwitchFieldProps } from '../../components'

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
