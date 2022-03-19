import React from 'react'
import { Button, Box, Typography, TypographyProps } from '@mui/material'
import { styled } from '@mui/system'
import { Item as GameItem } from '../game'

export interface ItemProps {
    used?: number
    value: GameItem
}

function SecondaryText({ variant, ...props }: TypographyProps) {
    return <Typography {...props} variant={variant || 'body2'} sx={{ textAlign: 'left' }} />
}

const PrimaryText = styled(SecondaryText)`
    font-weight: bold;
`

const Col = styled(Box)`
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    justify-content: space-between;
    gap: 1;
`

export function Item({ used, value }: ItemProps) {
    return <Button size='small' color='inherit' variant='outlined' sx={{ display: 'flex', flexDirection: 'row', justifyContent: 'space-between' }}>
        <Col>
            <SecondaryText>{value.name}</SecondaryText>
            <PrimaryText>{ used > 0 ? `${value.amount - used} / ` : ''}{value.amount}</PrimaryText>
        </Col>
        <Col>
            { value.price > 0 && <Typography>${value.price}</Typography> }
        </Col>
    </Button>
}
