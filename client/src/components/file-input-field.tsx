import React from 'react'
import { observer } from 'mobx-react-lite'
import { action, computed, makeObservable, observable } from 'mobx'
import { Button, IconButton, Stack, Typography } from '@mui/material'
import { FileInput } from './file-input'

import AttachFileIcon from '@mui/icons-material/AttachFile'
import ClearIcon from '@mui/icons-material/Clear'

export class FileViewModel {
    constructor() {
        makeObservable(this)
    }

    file: File

    @observable name: string = ''
    @observable size: number = 0

    @computed get isSet() {
        return this.size > 0
    }

    @action readonly set = (files: FileList) => {
        this.file = files[0]
        this.name = this.file.name
        this.size = this.file.size
    }

    @action readonly clear = () => {
        this.file = null
        this.name = ''
        this.size = 0
    }
}

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

export interface FileInputFieldProps {
    model: FileViewModel
    title: React.ReactNode
    action?: React.ReactNode
}

function _FileInputField({ model, title, action }: FileInputFieldProps) {
    return <Stack gap={1}>
        <Typography variant='caption' color='textSecondary'>{title}</Typography>
        <Stack direction='row' gap={2} alignItems='center'>
            {model.isSet && <IconButton color='error' onClick={model.clear}>
                <ClearIcon />
            </IconButton> }

            { !model.isSet
                ? <FileInput
                    trigger={
                        <Button variant='outlined' startIcon={<AttachFileIcon />}>
                            { action ? action : 'Choose file' }
                        </Button>
                    }
                    onChange={model.set}
                />
                : <Typography variant='body2'>{model.name} ({formatFileSize(model.size)})</Typography>
            }
        </Stack>
    </Stack>
}

export const FileInputField = observer(_FileInputField)
