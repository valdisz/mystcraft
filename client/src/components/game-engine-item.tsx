import React, { memo } from 'react'
import { GameEngineFragment } from '../schema'
import { Chip, IconButton, ListItem, ListItemButton, ListItemText, Typography } from '@mui/material'
import { Confirm, DateTime } from './bricks'

import DeleteIcon from '@mui/icons-material/Delete'


export interface GameEngineItemProps {
    engine: GameEngineFragment
    onDelete?: (id: string) => void
}

function GameEngineItem({ engine, onDelete }: GameEngineItemProps) {
    console.log('rendering game engine item', engine.id)

    const created = <Typography variant='subtitle2'>
        <DateTime value={engine.createdAt} />
        {' '}
        by
        {' '}
        {engine.createdBy?.name || 'unknown'}
    </Typography>

    return <ListItem
        secondaryAction={
            <Confirm>
                <IconButton onClick={() => onDelete?.(engine.id)}><DeleteIcon /></IconButton>
            </Confirm>
        }
        disablePadding
    >
        <ListItemButton>
            <ListItemText primary={engine.name} primaryTypographyProps={{ variant: 'subtitle1' }} secondary={created} />
            <ListItemText primary={engine.description} />
            <Chip size='small' variant={engine.remote ? 'filled' : 'outlined' } label={engine.remote ? 'remote' : 'local'} />
        </ListItemButton>
    </ListItem>
}

export default memo(GameEngineItem)
