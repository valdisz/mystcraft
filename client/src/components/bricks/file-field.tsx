import React from 'react'
import { Button, FormControl, InputLabel, IconButton, Stack, Typography, FormHelperText } from '@mui/material'
import { FileInput } from './file-input'

import AttachFileIcon from '@mui/icons-material/AttachFile'
import ClearIcon from '@mui/icons-material/Clear'

const FILE_SIZE_TABLE = [
    { unit: 'TiB', limit: 1024 * 1024 * 1024 * 1024 },
    { unit: 'GiB', limit: 1024 * 1024 * 1024 },
    { unit: 'MiB', limit: 1024 * 1024 },
    { unit: 'KiB', limit: 1024 },
    { unit:   'B', limit: 0 },
]

function getFileSizeUnit(size: number) {
    for (const unit of FILE_SIZE_TABLE) {
        if (size >= unit.limit) {
            return unit
        }
    }

    return FILE_SIZE_TABLE[FILE_SIZE_TABLE.length - 1]
}

function formatFileSize(size: number) {
    const { unit, limit } = getFileSizeUnit(size)

    const normalizedSize = limit > 0
        ? (size / limit).toFixed(2)
        : size

    return `${normalizedSize} ${unit}`
}

export interface FileFieldProps {
    label?: React.ReactNode
    actionLabel?: React.ReactNode
    required?: boolean
    value?: File
    accept?: string
    error?: boolean
    helperText?: React.ReactNode
    onChange?: (file: File) => void
    onBlur?: () => void

    // model: FileViewModel
    // action?: React.ReactNode
}

export function FileField({
    label,
    actionLabel,
    required,
    value,
    accept,
    error,
    helperText,
    onChange,
    onBlur
}: FileFieldProps) {
    const handleClear = React.useCallback(() => {
        onChange(null)
    }, [ onChange ])

    const handleFileChange = React.useCallback((files: FileList) => {
        onChange(files[0])
    }, [ onChange ])

    const trigger = (
        <Button variant='outlined' startIcon={<AttachFileIcon />}>
            { actionLabel || 'Choose file' }
        </Button>
    )

    return <FormControl required={required} error={error}>
        <InputLabel shrink required={required} error={error}>{label}</InputLabel>
        <Stack direction='row' alignItems='center' gap={2} mt={2}>
            {(!!value) && <IconButton color='error' onClick={handleClear}>
                <ClearIcon />
            </IconButton> }

            { !!value
                ? <Typography variant='body2'>{value.name} ({formatFileSize(value.size)})</Typography>
                : <FileInput accept={accept} trigger={trigger} onChange={handleFileChange} onBlur={onBlur} />
            }
        </Stack>
        <FormHelperText error={error} required={required}>{helperText}</FormHelperText>
    </FormControl>
}
