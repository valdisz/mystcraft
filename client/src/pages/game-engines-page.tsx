import React from 'react'
import { observer, Observer } from 'mobx-react-lite'
import {
    Button, Container, List, ListItem, ListItemButton, ListItemText, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
    Alert, AlertTitle, Stack, Paper, IconButton, LinearProgress, Box
} from '@mui/material'
import { LoadingButton } from '@mui/lab'
import { PageTitle, EmptyListItem, Forbidden, FileInputField, Confirm, DateTime } from '../components'
import { NewGameEngineStore, useStore } from '../store'
import { GameEngineFragment } from '../schema'
import { Role, ForRole, forRole } from '../auth'

import DeleteIcon from '@mui/icons-material/Delete'

function GameEnginesPage() {
    const { gameEngines } = useStore()

    const engines = gameEngines.engines
    const items = engines.map(x => <GameEngineItem key={x.id} engine={x} onDelete={() => gameEngines.delete(x.id)} />)

    return <Container>
        <PageTitle
            title='Game Engines'
            actions={
                <ForRole role={Role.GameMaster}>
                    <Button variant='outlined' color='primary' size='large' onClick={gameEngines.newEngine.open}>New Engine</Button>
                </ForRole>
            }
        />

        <Stack gap={6}>
            { engines.isFailed && <Alert severity="error">
                <AlertTitle>Error</AlertTitle>
                { engines.error.message }
            </Alert> }

            <Paper elevation={0} variant='outlined'>
                { engines.isLoading && <LinearProgress /> }

                <List dense disablePadding>
                    { engines.isEmpty
                        ? <EmptyListItem>{engines.isLoading ? 'Loading...' : null}</EmptyListItem>
                        : items
                    }
                </List>
            </Paper>
        </Stack>

        <ObservableNewGameEngineDialog model={gameEngines.newEngine} />
    </Container>
}

export default forRole(observer(GameEnginesPage), Role.GameMaster, <Forbidden />)


interface GameEngineItemProps {
    engine: GameEngineFragment
    onDelete: () => void
}

function GameEngineItem({ engine, onDelete }: GameEngineItemProps) {
    return <ListItem
        secondaryAction={
            <Confirm onConfirm={onDelete}>
                <IconButton><DeleteIcon /></IconButton>
            </Confirm>
        }
        disablePadding
    >
        <ListItemButton>
            <ListItemText primary={engine.name} secondary={<DateTime value={engine.createdAt} TypographyProps={{ variant: 'body2' }} />} />
        </ListItemButton>
    </ListItem>
}

interface NewGameEngineDialogProps {
    model: NewGameEngineStore
}

function NewGameEngineDialog({ model }: NewGameEngineDialogProps) {
    return <Dialog fullWidth maxWidth='sm' open={model.isOpen} onClose={model.autoClose}>
        <DialogTitle>New game engine</DialogTitle>
        <DialogContent>
            { model.error
                ? <Alert severity="error">
                    <AlertTitle>Error</AlertTitle>
                    {model.error}
                </Alert>
                : null
            }
            <Stack gap={2}>
                <TextField label='Name' value={model.name} onChange={model.setName} />

                <FileInputField title='Game Engine' model={model.content} />
                <FileInputField title='Ruleset' model={model.ruleset} />
            </Stack>

        </DialogContent>
        <DialogActions>
            <Button disabled={model.inProgress} onClick={model.close}>Cancel</Button>
            <LoadingButton loading={model.inProgress} onClick={model.confirm} color='primary' variant='contained'>Upload game engine</LoadingButton>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameEngineDialog = observer(NewGameEngineDialog)
