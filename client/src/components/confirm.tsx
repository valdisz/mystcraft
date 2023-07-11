import { Box, Typography, CircularProgress, IconButton, Stack } from '@mui/material'
import React, { useState, cloneElement, useEffect, useCallback, useRef } from 'react'

import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'

export interface ConfirmProps {
    children: React.ReactElement
    yes?: React.ReactNode
    no?: React.ReactNode
    title?: React.ReactNode
    timeout?: number
    onConfirm: () => void
    onReject?: () => void
}

function Confirm({ yes, no, timeout, title, onConfirm, onReject, children }: ConfirmProps) {
    const pendingTimeout = timeout || 5000

    const [pending, setPending] = useState(false)
    const [elapsed, setElapsed] = useState(0)

    const makePending = useCallback(() => {
        setElapsed(0)
        setPending(true)
    }, [setPending, setElapsed])

    const reset = useCallback(() => {
        setPending(false)
    }, [setPending])

    const reject = useCallback(() => {
        onReject?.()
        reset()
    }, [onReject, reset])

    const confirm = useCallback(() => {
        onConfirm?.()
        reset()
    }, [onConfirm, reset])

    const animationRef = useRef<number>()
    const animationStartTimeRef = useRef<DOMHighResTimeStamp>()

    useEffect(() => {
        if (!pending) {
            return
        }

        const updateProgress = (timestamp: DOMHighResTimeStamp) => {
            if (!animationStartTimeRef.current) {
                animationStartTimeRef.current = timestamp
            }

            setElapsed(Math.trunc(timestamp - animationStartTimeRef.current))

            animationRef.current = requestAnimationFrame(updateProgress)
        }

        animationStartTimeRef.current = 0
        animationRef.current = requestAnimationFrame(updateProgress)

        return () => {
            if (animationRef.current) {
                cancelAnimationFrame(animationRef.current)
            }
        }
    }, [pending, pendingTimeout, reset])

    const progress = Math.trunc(Math.min((elapsed / pendingTimeout) * 100, 100))
    useEffect(() => {
        if (progress >= 100) {
            setTimeout(reject, 500)
        }
    }, [progress, reject])

    if (pending) {
        return <Stack>
            { !title || [ 'string', 'number', 'boolean' ].includes(typeof title)
                ? <Typography variant='caption'>{ title || 'Are you sure?' }</Typography>
                : title
            }
            <Stack direction='row' gap={2} alignItems='center'>
                <IconButton size='small' onClick={confirm}>{yes || <CheckIcon />}</IconButton>
                <IconButton size='small' onClick={reject}>
                    <Box sx={{ position: 'relative', display: 'inline-flex' }}>
                        <CircularProgress size='1.5rem' variant="determinate" value={progress} />
                        <Box sx={{
                                top: 0,
                                left: 0,
                                bottom: 0,
                                right: 0,
                                position: 'absolute',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                            }}
                        >
                            {no || <CloseIcon />}
                        </Box>
                    </Box>
                </IconButton>
            </Stack>
        </Stack>
    }

    return cloneElement(children, { onClick: makePending })
}

export default Confirm
