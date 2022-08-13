import React from 'react'
import { Box, Typography, ListItem, ListItemText } from '@mui/material'

export interface EmptyListItemProps {
    children?: React.ReactNode
}

export function EmptyListItem({ children }: EmptyListItemProps) {
    return <ListItem>
        <Box sx={{
            flex: 1,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center'
        }}>
            { children || <Typography variant='subtitle1'>Empty</Typography> }
        </Box>
    </ListItem>
}
