import * as React from 'react'
import { Typography, TypographyProps } from '@mui/material'

export function FixedTypography({ sx, ...props }: TypographyProps) {
    return<Typography {...props} sx={{ overflow: 'hidden', whiteSpace: 'nowrap', textOverflow: 'ellipsis', ...(sx || { })  }} />
}
