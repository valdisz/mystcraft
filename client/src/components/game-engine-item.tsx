import React, { memo } from 'react'
import { GameEngineFragment } from '../schema'
import { IconButton, ListItem, ListItemButton, ListItemText } from '@mui/material'
import { Confirm, DateTime } from './bricks'

import DeleteIcon from '@mui/icons-material/Delete'


export interface GameEngineItemProps {
    engine: GameEngineFragment
    onDelete?: (id: string) => void
}

function GameEngineItem({ engine, onDelete }: GameEngineItemProps) {
    console.log('rendering game engine item', engine.id)

    return <ListItem
        secondaryAction={
            <Confirm>
                <IconButton onClick={() => onDelete?.(engine.id)}><DeleteIcon /></IconButton>
            </Confirm>
        }
        disablePadding
    >
        <ListItemButton>
            <ListItemText primary={engine.name} secondary={<DateTime value={engine.createdAt} TypographyProps={{ variant: 'body2' }} />} />
        </ListItemButton>
    </ListItem>
}

export default memo(GameEngineItem)
