import * as React from 'react'
import { styled } from '@mui/system'
import { List, ListItem, ListItemText, ListItemIcon, TextField, Button, Container, Card, CardHeader,
    ListItemSecondaryAction, DialogTitle, DialogContent, DialogActions, Dialog, Box, Grid, Menu, IconButton, MenuItem, IconButtonProps, MenuProps
} from '@mui/material'
import { Observer, observer } from 'mobx-react'
import { Link } from 'react-router-dom'
import { useStore } from '../store'
import { SplitButton } from '../components'
import { GameHeaderFragment, PlayerHeaderFragment } from '../schema'
import cronstrue from 'cronstrue'
import { ForRole } from '../auth'
import { Role } from '../roles'

import GrainIcon from '@mui/icons-material/Grain'

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
    return <SplitButton disabled={disabled} color='secondary' size='small' variant='outlined'
        onClick={onUploadReport}
        actions={[
            { content: 'Import map', onAction: onUploadMap },
            { content: 'Delete', onAction: onDelete },
            { content: 'Update Ruleset', onAction: onRuleset }
        ]}>
            Upload report
    </SplitButton>
}

function Game({ name }) {
    return name ? <strong>{name}</strong> : <em>No name</em>
}

const FactionNumber = styled('code')`
    font-weight: bold;
    &::before {
        content: '[';
    }
    &::after {
        content: ']';
    }
`

function Faction({ name, number}) {
    return name && number
        ?<>
            {name} <FactionNumber>{number}</FactionNumber>
        </>
        : <em>Faction not yet determined</em>
}

function GamePlayer({ name, number, lastTurnNumber }: PlayerHeaderFragment) {
    return <ListItemText>
        <Box sx={{ listStyle: 'none', m: 0, p: 0 }} component='ul'>
            <li>
                <strong>Faction</strong> <Faction name={name} number={number} />
            </li>
            <li>
                <strong>Turn</strong> {lastTurnNumber}
            </li>
        </Box>
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
        props.replace = true
    }

    const cron = game.options.schedule
        ? `${cronstrue.toString(game.options.schedule, { use24HourTimeFormat: true })} (${game.options.timeZone ?? 'UTC'})`
        : null

    return <ListItem {...props}>
        <ListItemIcon>
            <GrainIcon />
        </ListItemIcon>
        <Grid container>
            <Grid item xs={12} md={6}>
                <ListItemText
                    primary={<Game name={game.name} />} />
            </Grid>
            <Grid item xs={12} md={6}>
                { game.me && <GamePlayer {...game.me} />}
            </Grid>
            { cron && <Grid item xs={12}>
                { cron }
            </Grid> }
        </Grid>
        <ListItemSecondaryAction>
            <Observer>
                {() => playerJoind
                    ? <>
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
        <DialogTitle id="form-dialog-title">New local game</DialogTitle>
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
            <input
                style={{ display: 'none' }}
                id='upload-game-engine'
                type='file'
                onChange={({ target }) => newGame.setFile('engine', target.files[0])}
            />
            <input
                style={{ display: 'none' }}
                id='upload-players'
                type='file'
                onChange={({ target }) => newGame.setFile('players', target.files[0])}
            />
            <input
                style={{ display: 'none' }}
                id='upload-game'
                type='file'
                onChange={({ target }) => newGame.setFile('game', target.files[0])}
            />
            <label htmlFor='upload-game-engine'>
                <Button variant='outlined' component='span'>Select Game Engine</Button>
            </label>
            <label htmlFor='upload-players'>
                <Button variant='outlined' component='span'>Select players.*</Button>
            </label>
            <label htmlFor='upload-game'>
                <Button variant='outlined' component='span'>Select game.*</Button>
            </label>
        </DialogContent>
        <DialogActions>
            <Button onClick={newGame.cancel}>
                Cancel
            </Button>
            <Button onClick={home.confirmNewGame} color='primary' variant='outlined'>
                Start new game
            </Button>
        </DialogActions>
    </Dialog>
})

export function HomePage() {
    const { home } = useStore()

    React.useEffect(() => { home.load() }, [])

    return <>
        <Container >
            <Card>
                <CardHeader title='Games'
                    action={<ForRole role={Role.GameMaster}>
                        <Button variant='outlined' color='primary' size='large' onClick={home.newGame.open}>New game</Button>
                    </ForRole>}
                />
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
    </>
}
