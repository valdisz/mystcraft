import * as React from 'react'
import styled from 'styled-components'
import { Button, ButtonProps } from '@material-ui/core'
import ClipboardJS from 'clipboard'
import { useCallbackRef } from '../lib'

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

export function CopyButton({ text, ...props }: CopyButtonProps) {
    const [ref, setRef] = useCallbackRef<HTMLButtonElement>()

    React.useEffect(() => {
        if (!ref) return

        const clip = new ClipboardJS(ref)

        return () => clip.destroy()
    }, [ ref ])

    return <XsButton {...props as any} ref={setRef as any} data-clipboard-text={text} />
}
