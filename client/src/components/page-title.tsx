import React from 'react'
import { Box, Typography, useTheme } from '@mui/material'

export interface PageTitleProps {
    title: string | React.ReactNode
    actions?: React.ReactNode
}

export function PageTitle({ title, actions }: PageTitleProps) {
    const theme = useTheme()
    return <Box sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: theme.spacing(2),
        mb: 4
    }}>
        <Box sx={{ minWidth: 0, flex: 1 }}>
            { typeof title === 'string'
                ? <Typography variant='h3'>{title}</Typography>
                : title
            }
        </Box>
        <Box sx={{ minWidth: 0 }}>
            {actions}
        </Box>
    </Box>
}
