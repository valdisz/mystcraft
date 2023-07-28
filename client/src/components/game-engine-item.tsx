import React, { memo } from 'react'
import { GameEngineFragment } from '../schema'
import { Chip, IconButton, ListItem, ListItemButton, Typography, Stack, Box } from '@mui/material'
import { Confirm, DateTime } from './bricks'

import DeleteIcon from '@mui/icons-material/Delete'


export interface GameEngineItemProps {
    engine: GameEngineFragment
    onDelete?: (id: string) => void
}

function GameEngineItem({ engine, onDelete }: GameEngineItemProps) {
    return <ListItem sx={{
            ':not(:last-child)': {
                borderBottomWidth: '1px',
                borderBottomStyle: 'dashed',
                borderBottomColor: 'divider',
            }
        }}
        secondaryAction={
            <Confirm>
                <IconButton onClick={() => onDelete?.(engine.id)}><DeleteIcon /></IconButton>
            </Confirm>
        }
        disablePadding
    >
        <ListItemButton>
            <Stack gap={2} flex={1} minWidth={0}>
                <Typography variant='h6'>{engine.name}</Typography>
                <Typography variant='body1'>{engine.description}</Typography>
                <Typography variant='subtitle2'>
                    <DateTime value={engine.createdAt} />
                    {' '}
                    by
                    {' '}
                    {engine.createdBy?.name || 'unknown'}
                </Typography>
            </Stack>
            <Box flex={1} minWidth={0}>
                <Chip variant={engine.remote ? 'filled' : 'outlined' } label={engine.remote ? 'Remote' : 'Local'} />
            </Box>
        </ListItemButton>
    </ListItem>
}

export default memo(GameEngineItem)
