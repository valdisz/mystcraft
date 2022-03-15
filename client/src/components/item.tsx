import React from 'react'
import { Button, Box, Typography } from '@mui/material'
import { styled } from '@mui/system'
import { Item as GameItem } from '../game'

export interface ItemProps {
    used?: number
    value: GameItem
}

const SecondaryText = styled(Typography)`
    font-size: 75%;
`

const PrimaryText = styled(SecondaryText)`
    font-weight: bold;
`

const Row = styled(Box)`
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1;
`

export function Item({ used, value }: ItemProps) {
    return <Button size='small' color='inherit' variant='outlined' sx={{ display: 'flex', flexDirection: 'column', alignItems: 'stretch' }}>
        <Row>
            <PrimaryText>{value.name}</PrimaryText>
            { value.price > 0 && <SecondaryText>${value.price}</SecondaryText> }
        </Row>
        <Row>
            <SecondaryText>{ used > 0 ? `${value.amount - used} / ` : ''}{value.amount}</SecondaryText>
        </Row>
    </Button>
}
