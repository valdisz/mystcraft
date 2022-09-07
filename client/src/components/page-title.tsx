import React from 'react'
import { Box, IconButton, Stack, Typography, useTheme } from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { Link } from 'react-router-dom'

export interface PageTitleProps {
    title: string | React.ReactNode
    back?: string
    actions?: React.ReactNode
}

export function PageTitle({ title, back, actions }: PageTitleProps) {
    const theme = useTheme()
    return <Box sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: theme.spacing(2),
        mb: 4
    }}>
        <Stack direction='row' flex={1} minWidth={0} alignItems='center' gap={2}>
            { back && <IconButton component={Link} to={back}><ArrowBackIcon fontSize='large' /></IconButton> }
            { typeof title === 'string'
                ? <Typography variant='h3'>{title}</Typography>
                : title
            }
        </Stack>
        <Box sx={{ minWidth: 0 }}>
            {actions}
        </Box>
    </Box>
}
