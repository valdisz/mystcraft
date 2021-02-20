import * as React from 'react'
import styled from 'styled-components'
import { List, ListItem, ListItemText, ListItemIcon, TextField, Button, Container, Card, CardHeader,
    ListItemSecondaryAction, DialogTitle, DialogContent, DialogActions, Dialog, ButtonGroup
} from '@material-ui/core'
import { observer, useObserver } from 'mobx-react-lite'
import { Link } from 'react-router-dom'
import GrainIcon from '@material-ui/icons/Grain';
import { useStore } from '../store'
import { SplitButton } from '../components'



function NoGames() {
    return <ListItem>
        <ListItemText><i>Empty</i></ListItemText>
    </ListItem>
}

interface GameItemProps {
    gameId: string
    factionName: string
    gameName: string
}

function GameActions() {
    return <ButtonGroup>
        <Button color='secondary' size='small'>Delete</Button>
        <Button color='secondary' size='small'>Delete</Button>
    </ButtonGroup>
}

function GameItem({ gameId, factionName, gameName }: GameItemProps) {
    return <ListItem button component={Link} to={`/game/${gameId}`} >
        <ListItemIcon>
            <GrainIcon />
        </ListItemIcon>
        <ListItemText secondary={factionName}>
            { gameName ? <strong>{gameName}</strong> : <em>No name</em> }
        </ListItemText>
        <ListItemSecondaryAction>
            <SplitButton color='default' size='small' variant='outlined' actions={[
                { content: 'Delete', onAction: () => { } }
            ]}>Load turn</SplitButton>
        </ListItemSecondaryAction>
    </ListItem>
}


const NewGameDialog = observer(() => {
    const { home } = useStore()
    const { newGame } = home

    return <Dialog open={newGame.isOpen} onClose={newGame.cancel} aria-labelledby="form-dialog-title">
        <DialogTitle id="form-dialog-title">New game</DialogTitle>
        <DialogContent>
            <TextField
                autoFocus
                margin="dense"
                id="name"
                label="New game name"
                type="text"
                fullWidth
                value={newGame.name}
                onChange={newGame.setName}
            />
        </DialogContent>
        <DialogActions>
            <Button onClick={newGame.cancel}>
                Cancel
            </Button>
            <Button onClick={home.confirmNewGame} color='primary' variant='outlined'>
                Add new game
            </Button>
        </DialogActions>
    </Dialog>
})

const CenterLayout = styled.div`
    display: flex;
    min-height: 0;
    height: 100%;
    flex-direction: column;
    align-items: center;
    justify-content: center;
`

export function HomePage() {
    const { home } = useStore()
    React.useEffect(() => {
        home.load()
    }, [])

    return <CenterLayout>
        <Container>
            <Card>
                <CardHeader title='Games' action={<Button variant='outlined' color='primary' size='large' onClick={home.newGame.open}>New game</Button>} />
                <List component='nav' dense>
                    {useObserver(() => <>
                        { home.games.length
                            ? home.games.map(x => <GameItem key={x.id} gameId={x.id} factionName={x.playerFactionName} gameName={x.name} />)
                            : <NoGames />
                        }
                    </>)}
                </List>
            </Card>
        </Container>
        <NewGameDialog />
    </CenterLayout>
}
