import { Box, BoxProps } from '@mui/material'
import React from 'react'

export interface FileInputProps extends Omit<BoxProps, 'children'> {
    trigger: React.ReactElement
    onChange?: React.ChangeEventHandler<HTMLInputElement>
}

export function FileInput({ trigger, onChange, ...props }: FileInputProps) {
    const inputRef = React.useRef<HTMLInputElement>(null)

    const onFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        onChange(event)
        event.target.value = null
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
