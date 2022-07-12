import React from 'react'
import { Link, Typography } from '@mui/material'

export function Copyright() {
    return (
        <Typography variant='body2' color='textSecondary' align='center'>
            { `Copyright ©  ${new Date().getFullYear()} ` }
            <Link color='inherit' href='https://advisor.azurewebsites.net'>Valdis Zobēla</Link>
        </Typography>
    );
}
