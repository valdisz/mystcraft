import { Box, BoxProps } from '@mui/material'
import React from 'react'

export interface FileInputProps extends Omit<BoxProps, 'children' | 'onChange'> {
    trigger: React.ReactElement
    onChange?: (files: FileList) => void
}

export function FileInput({ trigger, onChange, ...props }: FileInputProps) {
    const inputRef = React.useRef<HTMLInputElement>(null)

    const onFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const { files } = event.target
        if (files.length) {
            onChange(files)
            event.target.value = null
        }
    }

    return <Box {...props}>
        { trigger
            ? React.cloneElement(trigger, {
                onClick: () => {
                    inputRef.current.click()
                }
            } )
            : null
        }
        <input style={{ display: 'none' }}
            ref={inputRef}
            type='file'
            onChange={onFileChange}
        />
    </Box>
}
