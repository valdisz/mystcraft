import * as React from 'react'
import styled from 'styled-components'
import { List, ListItem, ListItemText, ListItemIcon, TextField, Button, Container, Card, CardHeader,
    ListItemSecondaryAction, DialogTitle, DialogContent, DialogActions, Dialog
} from '@material-ui/core'
import { observer, useObserver } from 'mobx-react-lite'
import { Link } from 'react-router-dom'
import GrainIcon from '@material-ui/icons/Grain';
import { useStore } from '../store'
import { SplitButton } from '../components'
import { GameListItemFragment } from '../schema'

function NoGames() {
    return <ListItem>
        <ListItemText><i>Empty</i></ListItemText>
    </ListItem>
}

interface GameItemProps {
    game: GameListItemFragment
}

function GameActions() {
    const { home } = useStore()
    return <SplitButton color='default' size='small' variant='outlined'
        onClick={home.triggerUploadReport}
        actions={[
            { content: 'Delete', onAction: () => { } }
        ]}>
            Load turn
        </SplitButton>
}

function Game({ name }) {
    return name ? <strong>{name}</strong> : <em>No name</em>
}

const FactionNumber = styled.code`
    font-weight: bold;
    &::before {
        content: '[';
    }
    &::after {
        content: ']';
    }
`

const UnstyledList = styled.ul`
    list-style: none;
    margin: 0;
    padding: 0;
`

function Faction({ name, number}) {
    return name && number
        ?<>
            {name} <FactionNumber>{number}</FactionNumber>
        </>
        : <em>Faction not yet determined</em>
}

function Ruleset({ name, version}) {
    return <div>
        {name} {version}
    </div>
}

function GameItem({ game }: GameItemProps) {
    return <ListItem button component={Link} to={`/game/${game.id}`} >
        <ListItemIcon>
            <GrainIcon />
        </ListItemIcon>
        <ListItemText
            primary={<Game name={game.name} />}
            secondary={<Ruleset name={game.rulesetName} version={game.rulesetVersion} />} />
        <ListItemText>
            <UnstyledList>
                <li>
                    <strong>Faction</strong> <Faction name={game.playerFactionName} number={game.playerFactionNumber} />
                </li>
                <li>
                    <strong>Turn</strong> {game.lastTurnNumber}
                </li>
            </UnstyledList>
        </ListItemText>
        <ListItemSecondaryAction>
            <GameActions />
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
                <input type='file' ref={home.setFileUpload} style={{ display: 'none' }} onChange={home.uploadReport} />
                <List component='nav' dense>
                    {useObserver(() => <>
                        { home.games.length
                            ? home.games.map(x => <GameItem key={x.id} game={x} />)
                            : <NoGames />
                        }
                    </>)}
                </List>
            </Card>
        </Container>
        <NewGameDialog />
    </CenterLayout>
}
