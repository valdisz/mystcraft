import * as React from 'react'
import styled from '@emotion/styled'
import { Button, ButtonProps } from '@mui/material'
import { useCopy } from '../lib'

const XsButtonStyles = styled(Button)`
    padding: 2px !important;
    min-width: 20px !important;
    font-size: 11px;
    font-family: Fira Code, Roboto Mono, monospace;
`

export const XsButton = React.forwardRef<any, ButtonProps>(({ size, ...props}, ref) => {
    return <XsButtonStyles {...props} ref={ref} size='small' />
})

export interface CopyButtonProps extends ButtonProps {
    text: string
}

export function XsCopyButton({ text, ...props }: CopyButtonProps) {
    const copy = useCopy()
    return <XsButton {...props as any} ref={copy} data-clipboard-text={text} />
}
