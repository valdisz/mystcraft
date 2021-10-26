import * as React from 'react'
import styled from 'styled-components'
import { List, ListItem, ListItemText, ListItemIcon, TextField, Button, Container, Card, CardHeader,
    ListItemSecondaryAction, DialogTitle, DialogContent, DialogActions, Dialog, ButtonGroup, Box
} from '@material-ui/core'
import { Observer, observer } from 'mobx-react-lite'
import { Link } from 'react-router-dom'
import GrainIcon from '@material-ui/icons/Grain';
import { useStore } from '../store'
import { SplitButton } from '../components'
import { GameHeaderFragment, PlayerHeaderFragment } from '../schema'
import ExitToAppIcon from '@material-ui/icons/ExitToApp'

function NoGames() {
    return <ListItem>
        <ListItemText><i>Empty</i></ListItemText>
    </ListItem>
}

interface GameItemProps {
    game: GameHeaderFragment
}

interface GameActionsProps {
    disabled: boolean
    onUploadReport: () => void
    onUploadMap: () => void
    onDelete: () => void
    onRuleset: () => void
}

function GameActions({ disabled, onUploadReport, onDelete, onUploadMap, onRuleset }: GameActionsProps) {
    return <SplitButton disabled={disabled} color='default' size='small' variant='outlined'
        onClick={onUploadReport}
        actions={[
            { content: 'Import map', onAction: onUploadMap },
            { content: 'Delete', onAction: onDelete },
            { content: 'Update Ruleset', onAction: onRuleset }
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
    return <span>
        {name} {version}
    </span>
}

function GamePlayer({ name, number, lastTurnNumber }: PlayerHeaderFragment) {
    return <ListItemText>
        <UnstyledList>
            <li>
                <strong>Faction</strong> <Faction name={name} number={number} />
            </li>
            <li>
                <strong>Turn</strong> {lastTurnNumber}
            </li>
        </UnstyledList>
    </ListItemText>
}

function GameItem({ game }: GameItemProps) {
    const { home } = useStore()

    const props = { } as any

    const playerJoind = !!game.me
    const playerFactionKnown = !!game.me?.number

    if (playerFactionKnown) {
        props.to = `/game/${game.id}`
        props.button = true
        props.component = Link
    }

    return <ListItem {...props}>
        <ListItemIcon>
            <GrainIcon />
        </ListItemIcon>
        <ListItemText
            primary={<Game name={game.name} />}
            secondary={<Ruleset name={game.rulesetName} version={game.rulesetVersion} />} />

        { game.me && <GamePlayer {...game.me} />}

        <ListItemSecondaryAction>
            <Observer>
                {() => playerJoind
                    ? <>
                        { playerFactionKnown && <Box mr={1} clone>
                            <Button color='primary' size='small' variant='contained' component={Link} to={`/game/${game.id}/university`}>University</Button>
                        </Box> }
                        <GameActions
                            disabled={home.uploading}
                            onUploadReport={() => home.triggerUploadReport(game.me.id)}
                            onUploadMap={() => home.triggerImportMap(game.me.id)}
                            onDelete={() => home.deleteGame(game.id)}
                            onRuleset={() => home.triggerRuleset(game.id)} />
                    </>
                    : <Button color='primary' size='small' variant='outlined' onClick={() => home.joinGame(game.id)}>Join</Button> }
            </Observer>
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
    position: relative;
`

const TopRightCorner = styled.div`
    top: 1rem;
    right: 1rem;
    position: absolute;
`

export function HomePage() {
    const { home } = useStore()

    React.useEffect(() => {
        home.load()
    }, [])

    return <CenterLayout>
        <TopRightCorner>
            <Button component={'a'} startIcon={<ExitToAppIcon />} href='/logout' >Sign out</Button>
        </TopRightCorner>
        <Container>
            <Card>
                <CardHeader title='Games' action={<Button variant='outlined' color='primary' size='large' onClick={home.newGame.open}>New game</Button>} />
                <input type='file' ref={home.setFileUpload} style={{ display: 'none' }} onChange={home.uploadFile} />
                <List component='nav' dense>
                    <Observer>
                        {() => <>{ home.games.length
                            ? home.games.map(x => <GameItem key={x.id} game={x} />)
                            : <NoGames />
                            }</>
                        }
                    </Observer>
                </List>
            </Card>
        </Container>
        <NewGameDialog />
    </CenterLayout>
}
