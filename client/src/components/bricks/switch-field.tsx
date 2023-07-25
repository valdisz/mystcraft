import React from 'react'
import { FormControl, FormHelperText, FormControlLabel, Switch } from '@mui/material'

export interface SwitchFieldProps {
    label?: React.ReactNode
    required?: boolean
    disabled?: boolean
    checked?: boolean
    error?: boolean
    helperText?: React.ReactNode
    onChange?: (event: React.ChangeEvent<HTMLInputElement>, checked: boolean) => void
    onBlur?: () => void
}

function SwitchField({ label, required, checked, error, helperText, onBlur, onChange }: SwitchFieldProps) {
    return (
        <FormControl required={required} error={error}>
            <FormControlLabel required={required} control={<Switch checked={checked} required={required} onBlur={onBlur} onChange={onChange} />} label={label} />
            <FormHelperText error={error} required={required}>{helperText}</FormHelperText>
        </FormControl>
    )
}

export default SwitchField
