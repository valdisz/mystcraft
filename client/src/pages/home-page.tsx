import * as React from 'react'
import styled from 'styled-components'
import { List, ListItem, ListItemText, ListItemIcon, TextField, Button, Container, Card, CardHeader,
    ListItemSecondaryAction, DialogTitle, DialogContent, DialogActions, Dialog
} from '@material-ui/core'
import { action, observable, runInAction, transaction } from 'mobx'
import { useObserver } from 'mobx-react-lite'
import { CLIENT } from '../client'
import { GameListItemFragment, GetGamesQuery, GetGames, NewGame, NewGameMutation, NewGameMutationVariables } from '../schema'
import { Link } from 'react-router-dom'
import GrainIcon from '@material-ui/icons/Grain';

export class NewGameStore {
    @observable adding = false
    @observable name = ''

    @action add = () => {
        this.name = ''
        this.adding = true
    }

    @action setName = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.name = e.target.value
    }

    @action cancel = () => {
        this.adding = false
    }
}

export class HomeStore {
    readonly games = observable<GameListItemFragment>([])

    load = () => {
        CLIENT.query<GetGamesQuery>({
            query: GetGames
        }).then(response => {
            runInAction(() => {
                this.games.replace(response.data.games.nodes)
            })
        })
    }

    readonly newGame = new NewGameStore()

    confirmNewGame = async () => {
        const response = await CLIENT.mutate<NewGameMutation, NewGameMutationVariables>({
            mutation: NewGame,
            variables: {
                name: this.newGame.name
            }
        })

        transaction(() => {
            this.games.push(response.data.newGame)
            this.newGame.cancel()
        })
    }
}

export const homeStore = new HomeStore()

function NewGameItem() {
    return <ListItem button onClick={homeStore.newGame.add}>
        <ListItemText>+ New Game</ListItemText>
    </ListItem>
}

function AddingNewGame() {
    return <ListItem>
        <ListItemText>
            {useObserver(() => <>
                <TextField variant='outlined' value={homeStore.newGame.name} onChange={homeStore.newGame.setName} />
                <Button onClick={homeStore.confirmNewGame}>Add game</Button>
                <Button onClick={homeStore.newGame.cancel}>Cancel</Button>
            </>)}
        </ListItemText>
    </ListItem>
}

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

function GameItem({ gameId, factionName, gameName }: GameItemProps) {
    return <ListItem button component={Link} to={`/game/${gameId}`} >
        <ListItemIcon>
            <GrainIcon />
        </ListItemIcon>
        <ListItemText secondary={factionName}>
            { gameName ? <strong>{gameName}</strong> : <em>No name</em> }
        </ListItemText>
        <ListItemSecondaryAction>
            <Button color='secondary' size='small'>Delete</Button>
        </ListItemSecondaryAction>
    </ListItem>
}

const CenterLayout = styled.div`
    display: flex;
    min-height: 0;
    height: 100%;
    flex-direction: column;
    align-items: center;
    justify-content: center;
`

export function HomePage() {
    React.useEffect(() => {
        homeStore.load()
    }, [])

    const newGame = homeStore.newGame

    return <CenterLayout>
        <Container>
            <Card>
                <CardHeader title='Games' action={<Button variant='outlined' color='primary' size='large' onClick={newGame.add}>New game</Button>} />
                <List component='nav' dense>
                    {useObserver(() => <>
                        { homeStore.games.length
                            ? homeStore.games.map(x => <GameItem key={x.id} gameId={x.id} factionName={x.playerFactionName} gameName={x.name} />)
                            : <NoGames />
                        }
                    </>)}
                </List>
            </Card>
        </Container>
        {useObserver(() => <Dialog open={newGame.adding} onClose={newGame.cancel} aria-labelledby="form-dialog-title">
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
                <Button onClick={homeStore.confirmNewGame} color='primary' variant='outlined'>
                    Add new game
                </Button>
            </DialogActions>
        </Dialog> )}
    </CenterLayout>
}
