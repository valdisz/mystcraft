import * as React from 'react'
import { Typography, IconButton, Card, Collapse, CardActions, CardProps } from '@mui/material'
import { ExpandMore } from './expand-more'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import CloseIcon from '@mui/icons-material/Close'

export interface FloatingPanelProps extends CardProps {
    header: React.ReactNode
    expanded: boolean
    onExpand: (expanded: boolean) => void
    onClose?: () => void

    children?: React.ReactNode
}

export function FloatingPanel({ header, expanded, onClose, onExpand, sx, children, ...props }: FloatingPanelProps) {
    const onExpandClick = () => {
        onExpand(!expanded)
    }

    return <Card {...props} sx={{ opacity: 0.92, ...(sx || { }) }}>
        <CardActions disableSpacing sx={{ gap: 1 }}>
            { typeof header === 'string' ? <Typography variant='h6'>{header}</Typography> : header }
            <ExpandMore expand={expanded} onClick={onExpandClick}>
                <ExpandMoreIcon />
            </ExpandMore>
            { onClose && <IconButton onClick={onClose}>
                <CloseIcon />
            </IconButton> }
        </CardActions>
        <Collapse in={expanded}>
            {children}
        </Collapse>
    </Card>
}
