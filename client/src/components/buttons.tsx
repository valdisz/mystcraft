import * as React from 'react'
import styled from 'styled-components'
import { Button, ButtonProps } from '@material-ui/core'
import { useCopy } from '../lib'

export const XsButton = styled(Button).attrs({
    size: 'small'
})`
    padding: 2px !important;
    min-width: 20px !important;
    font-size: 11px;
    font-family: Fira Code, Roboto Mono, monospace;
`

export interface CopyButtonProps extends ButtonProps {
    text: string
}

export function XsCopyButton({ text, ...props }: CopyButtonProps) {
    const copy = useCopy()
    return <XsButton {...props as any} ref={copy} data-clipboard-text={text} />
}
