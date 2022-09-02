import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    Button, Container, List, ListItem, ListItemText, Dialog, DialogTitle, DialogContent, DialogActions, TextField, Typography,
    Box, Stack, Paper
} from '@mui/material'
import { PageTitle, EmptyListItem, Forbidden, FileInput } from '../components'
import { NewGameEngineStore, useStore } from '../store'
import { GameEngineFragment } from '../schema'
import { Role, ForRole, forRole } from '../auth'

import AttachFileIcon from '@mui/icons-material/AttachFile'

function GameEnginesPage() {
    const { gameEngines } = useStore()

    return <Container>
        <PageTitle title='Game Engines'
                actions={<ForRole role={Role.GameMaster}>
                <Button variant='outlined' color='primary' size='large' onClick={gameEngines.beginNewEngine}>New Engine</Button>
            </ForRole>} />

        <Paper elevation={0} variant='outlined'>
            <List dense>
                { gameEngines.engines.value.length
                    ? gameEngines.engines.value.map(x => <GameEngineItem key={x.id} engine={x} />)
                    : <EmptyListItem />
                }
            </List>
        </Paper>

        <ObservableNewGameEngineDialog store={gameEngines.newEngine} />
    </Container>
}
export default forRole(observer(GameEnginesPage), Role.GameMaster, <Forbidden />)


interface GameEngineItemProps {
    engine: GameEngineFragment
}

function GameEngineItem({ engine }: GameEngineItemProps) {
    return <ListItem>
        <ListItemText primary={engine.name} secondary={engine.createdAt} />
    </ListItem>
}


interface NewGameEngineDialogProps {
    store: NewGameEngineStore
}

function NewGameEngineDialog({ store }: NewGameEngineDialogProps) {
    return <Dialog fullWidth maxWidth='sm' open={store.isOpen} onClose={store.cancel}>
        <DialogTitle>New game engine</DialogTitle>
        <DialogContent>
            <Stack gap={2}>
                <TextField label='Name' value={store.name} onChange={store.setName} />
                <Box sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'space-between'
                }}>
                    <Typography>
                        <span>File:{' '}</span>
                        <strong>{store.fileName}</strong>
                    </Typography>
                    <FileInput trigger={<Button variant='outlined' startIcon={<AttachFileIcon />}>
                        Select Game Engine
                    </Button>} onChange={store.setFile} />
                </Box>
            </Stack>

        </DialogContent>
        <DialogActions>
            <Button onClick={store.cancel}>Cancel</Button>
            <Button onClick={store.confirm} color='primary' variant='contained'>Upload game engine</Button>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameEngineDialog = observer(NewGameEngineDialog)
