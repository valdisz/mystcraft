import React from 'react'
import { FormControl, InputLabel, Select, FormHelperText, SelectProps, MenuItem, SelectChangeEvent } from '@mui/material'

export interface SelectFieldProps<T> extends Omit<SelectProps, 'children' | 'value' | 'onChange' | 'renderValue'> {
    items: T[]
    children?: (item: T) => React.ReactNode
    mapKey?: (item: T) => string
    label?: React.ReactNode
    helperText?: React.ReactNode
    value?: T | null | ''
    onChange?: (value: T | null) => void
    renderValue?: (value: T | null) => React.ReactNode
}

function SelectField<T = unknown>({
    label,
    required,
    error,
    helperText,
    mapKey,
    items,
    children,
    value,
    onChange,
    renderValue,
    ...props
}: SelectFieldProps<T>) {
    const key = React.useCallback((item: T) => mapKey?.(item) ?? item, [mapKey])

    const itemMap = React.useMemo(() =>
        new Map(items.map(item => [key(item), item])),
        [items, key]
    )

    const handleOnChange = React.useCallback((event: SelectChangeEvent<any>) => {
        const newValue = typeof event.target.value === 'string'
            ? (
                event.target.value
                    ? itemMap.get(event.target.value)
                    : null
            )
            : event.target.value as T

        onChange?.(newValue)
    }, [onChange, itemMap])

    const renderSelectedValue = React.useCallback((value: any) => {
        if (!value) {
            return null
        }

        return renderValue ? renderValue(itemMap.get(value)) : value
    }, [itemMap, renderValue])

    const slectedValue = value ? key(value) : ''

    return <FormControl required={required} error={error}>
        <InputLabel required={required} error={error}>{label}</InputLabel>
        <Select label={label} required={required} error={error} value={slectedValue} renderValue={renderSelectedValue} onChange={handleOnChange} {...props}>
            { items.map((item) => {
                const keyValue = key(item) as any
                return <MenuItem key={keyValue} value={keyValue}>
                    {(children ? children(item) : item) as any}
                </MenuItem>
            })}
        </Select>
        <FormHelperText required={required} error={error}>{helperText}</FormHelperText>
    </FormControl>
}

export default SelectField
