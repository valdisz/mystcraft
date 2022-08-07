import React from 'react'
import { observer } from 'mobx-react-lite'
import { Button, Card, Container, List, ListItem, ListItemText, Dialog, DialogTitle, DialogContent, DialogActions } from '@mui/material'
import { PageTitle, EmptyListItem, Forbidden } from '../components'
import { NewGameEngineStore, useStore } from '../store'
import { GameEngineFragment } from '../schema'
import { Role, ForRole, forRole } from '../auth'

function GameEnginesPage() {
    const { gameEngines } = useStore()

    return <Container>
        <PageTitle title='Game Engines'
                actions={<ForRole role={Role.GameMaster}>
                <Button variant='outlined' color='primary' size='large' onClick={gameEngines.beginNewEngine}>New Engine</Button>
            </ForRole>} />

        <Card>
            <List dense>
                { gameEngines.engines.length
                    ? gameEngines.engines.map(x => <GameEngineItem key={x.id} engine={x} />)
                    : <EmptyListItem />
                }
            </List>
        </Card>

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
    return <Dialog open={store.isOpen} onClose={store.cancel}>
        <DialogTitle>New game engine</DialogTitle>
        <DialogContent>
        </DialogContent>
        <DialogActions>
            <Button onClick={store.cancel}>Cancel</Button>
            <Button onClick={store.confirm} color='primary' variant='contained'>Upload game engine</Button>
        </DialogActions>
    </Dialog>
}

const ObservableNewGameEngineDialog = observer(NewGameEngineDialog)
