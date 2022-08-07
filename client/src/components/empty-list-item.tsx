import React from 'react'
import { Box, Typography, ListItem, ListItemText } from '@mui/material'

export interface EmptyListItemProps {
    children?: React.ReactNode
}

export function EmptyListItem({ children }: EmptyListItemProps) {
    return <ListItem>
        <ListItemText>
            <Box sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center'
            }}>
                { children || <Typography variant='subtitle1'>Empty</Typography> }
            </Box>
        </ListItemText>
    </ListItem>
}
