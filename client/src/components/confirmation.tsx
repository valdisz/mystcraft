import { Button, Stack } from '@mui/material'
import React, { useState, cloneElement, useEffect, useCallback } from 'react'

export interface ConfirmationProps {
    children: React.ReactElement
    confirm?: React.ReactElement | string | number
    reject?: React.ReactNode | string | number
    container?: React.ComponentType<React.PropsWithChildren>
    timeout?: number
    onClick?: () => void
}

function actionContent(action: React.ReactNode | string | number, onClick: () => void) {
    if (typeof action === 'string' || typeof action === 'number') {
        return <Button size='small' onClick={onClick}>{action}</Button>
    }

    return cloneElement(action as React.ReactElement, { onClick })
}

function Confirmation({ confirm, reject, container, timeout, onClick, children }: ConfirmationProps) {
    const [pending, setPending] = useState(false)

    const makePending = useCallback(() => setPending(true), [ setPending ])
    const reset = useCallback(() => setPending(false), [ setPending ])

    useEffect(() => {
        if (!pending) return

        const handle = setTimeout(reset, timeout || 5000)

        return () => clearTimeout(handle)
    }, [ pending, reset, timeout ])

    if (pending) {
        const yesAction = actionContent(confirm || 'Yes', () => {
            onClick?.()
            reset()
        })

        const noAction = actionContent(reject || 'No', reset)

        if (container) {
            const Container = container
            return <Container>
                {yesAction}
                {noAction}
            </Container>
        }

        return <Stack direction='row' gap={2}>
            {yesAction}
            {noAction}
        </Stack>
    }

    return cloneElement(children, { onClick: makePending })
}

export default Confirmation
