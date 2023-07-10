import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    Button, Container, List, ListItem, ListItemText, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Typography,
    Box, Stack, Paper, IconButton
} from '@mui/material'
import { PageTitle, EmptyListItem, Forbidden, FileInputField, Confirmation } from '../components'
import { NewGameEngineStore, useStore } from '../store'
import { GameEngineFragment } from '../schema'
import { Role, ForRole, forRole } from '../auth'

import DeleteIcon from '@mui/icons-material/Delete'
import DateTime from '../components/date-time'

function GameEnginesPage() {
    const { gameEngines } = useStore()

    return <Container>
        <PageTitle title='Game Engines'
                actions={<ForRole role={Role.GameMaster}>
                <Button variant='outlined' color='primary' size='large' onClick={gameEngines.newEngine.open}>New Engine</Button>
            </ForRole>} />

        <Paper elevation={0} variant='outlined'>
            <List dense>
                { gameEngines.engines.value.length
                    ? gameEngines.engines.value.map(x => <GameEngineItem key={x.id} engine={x} onDelete={() => gameEngines.delete(x.id)} />)
                    : <EmptyListItem />
                }
            </List>
        </Paper>

        <ObservableNewGameEngineDialog model={gameEngines.newEngine} />
    </Container>
}

export default forRole(observer(GameEnginesPage), Role.GameMaster, <Forbidden />)


interface GameEngineItemProps {
    engine: GameEngineFragment
    onDelete: () => void
}

function GameEngineItem({ engine, onDelete }: GameEngineItemProps) {
    return <ListItem secondaryAction={
        <Confirmation confirm='Delete' reject='Cancel' onClick={onDelete}>
            <IconButton onClick={onDelete}><DeleteIcon /></IconButton>
        </Confirmation>
    }>
        <ListItemText primary={engine.name} secondary={<DateTime value={engine.createdAt} TypographyProps={{ variant: 'body2' }} />} />
    </ListItem>
}

interface NewGameEngineDialogProps {
    model: NewGameEngineStore
}

function NewGameEngineDialog({ model }: NewGameEngineDialogProps) {
    return <Dialog fullWidth maxWidth='sm' open={model.isOpen} onClose={model.close}>
        <DialogTitle>New game engine</DialogTitle>
        <DialogContent>
            <Stack gap={2}>
                <TextField label='Name' value={model.name} onChange={model.setName} />

                <FileInputField title='Game Engine' model={model.content} />
                <FileInputField title='Ruleset' model={model.ruleset} />
            </Stack>

        </DialogContent>
        <DialogActions>
            <Button onClick={model.close}>Cancel</Button>
            <Button onClick={model.confirm} color='primary' variant='contained'>Upload game engine</Button>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameEngineDialog = observer(NewGameEngineDialog)
