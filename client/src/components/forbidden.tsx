import React from 'react'
import { Box, Typography, Stack } from '@mui/material'

import LockTwoToneIcon from '@mui/icons-material/LockTwoTone'

export function Forbidden() {
    return <Box sx={{
        flex: 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
    }}>
        <Stack alignItems='center'>
            <LockTwoToneIcon fontSize='large' />
            <Typography variant='h4'>Forbidden</Typography>
            <Typography>You don't have access here</Typography>
        </Stack>
    </Box>
}
