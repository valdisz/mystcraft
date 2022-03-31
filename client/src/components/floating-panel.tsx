import * as React from 'react'
import { Box, Typography, IconButton, Card, Collapse, CardActions, CardProps, Stack } from '@mui/material'
import { ExpandMore } from './expand-more'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import CloseIcon from '@mui/icons-material/Close'

export interface FloatingPanelProps extends CardProps {
    header: React.ReactNode
    expanded?: boolean
    onExpand?: (expanded: boolean) => void
    onClose?: () => void

    children?: React.ReactNode
}

export function FloatingPanel({ header, expanded, onClose, onExpand, sx, children, ...props }: FloatingPanelProps) {
    const onExpandClick = () => {
        onExpand(!expanded)
    }

    return <Card {...props} sx={{ opacity: 0.92, ...(sx || { }) }}>
        <CardActions disableSpacing sx={{ gap: 1, minWidth: 0 }}>
            <Stack flex={1} spacing={1} direction='row' alignItems='center' justifyContent='flex-start' minWidth={0}>
                { typeof header === 'string' ? <Typography variant='h6'>{header}</Typography> : header }
            </Stack>
            <Stack spacing={1} direction='row' alignItems='center' justifyContent='flex-end' minWidth={0}>
                {  onExpand && <ExpandMore expand={expanded} onClick={onExpandClick}>
                    <ExpandMoreIcon />
                </ExpandMore> }
                { onClose && <IconButton onClick={onClose}>
                    <CloseIcon />
                </IconButton> }
            </Stack>
        </CardActions>
        { onExpand
            ? <Collapse in={expanded}>
                {children}
            </Collapse>
            : children
        }
    </Card>
}
