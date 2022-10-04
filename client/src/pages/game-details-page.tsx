import React, { useEffect } from 'react'
import { observer } from 'mobx-react-lite'
import {
    Box, Container, Stack, List, ListItem, ListItemButton, ListItemText, Typography, Paper, Button, Chip, Dialog, DialogActions,
    DialogContent, DialogContentText, DialogTitle, TextField, Alert, CircularProgress, Card, CardContent
} from '@mui/material'
import { styled } from '@mui/material/styles'
import { Link } from 'react-router-dom'
import { PageTitle } from '../components'
import { useParams } from 'react-router'
import { useStore, Player, TurnState } from '../store'
import { GameStatus } from '../schema'

function GameDetailsPage() {
    const { gameDetails } = useStore()
    const { gameId } = useParams()

    useEffect(() => {
        if (gameId) {
            gameDetails.setGameId(gameId)
        }
    }, [ gameId ])

    let content
    if (gameDetails.isLoading) {
        content = <Box sx={{ m: 4, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <CircularProgress />
        </Box>
    }
    else if (gameDetails.source.error) {
        content = <Alert severity="error">{gameDetails.source.error.message}</Alert>
    }
    else {
        content = <Stack gap={4}>
            <Stack direction='row' justifyContent='center' gap={4}>
                <Card variant='outlined'>
                    <CenterCardContent>
                        <Typography variant='caption'>Turn</Typography>
                        <Typography variant='h5'>{gameDetails.turnNumber}</Typography>
                    </CenterCardContent>
                </Card>
                <Card variant='outlined'>
                <CenterCardContent>
                        <Typography variant='caption'>Players</Typography>
                        <Typography variant='h5'>{gameDetails.playerCount}</Typography>
                    </CenterCardContent>
                </Card>
                <Card variant='outlined'>
                    <CenterCardContent>
                        <Typography variant='caption'>Local Players</Typography>
                        <Typography variant='h5'>{gameDetails.locaPlayerCount}</Typography>
                    </CenterCardContent>
                </Card>
            </Stack>

            { gameDetails.showOwnPlayers &&
                <Box>
                    <Typography variant='h5'>Own Faction</Typography>
                    <Paper variant='outlined'>
                        <PlayerList items={gameDetails.ownPlayers} />
                    </Paper>
                </Box> }

            { gameDetails.showClaimedPlayers &&
                <Box>
                    <Typography variant='h5'>Claimed Factions</Typography>
                    <Paper variant='outlined'>
                        <PlayerList items={gameDetails.claimedPlayers} />
                    </Paper>
                </Box> }

            { gameDetails.showRemotePlayers &&
                <Box>
                    <Typography variant='h5'>Remote Factions</Typography>
                    <Paper variant='outlined'>
                        <PlayerList items={gameDetails.remotePlayers} onClaim={gameDetails.claim} />
                    </Paper>
                </Box> }
        </Stack>
    }

    return <Container>
        <PageTitle
            title={gameDetails.name || 'Loading...'}
            back='/'
            actions={<GameActionsObserved />}
        />
        { content }
        <ClaimFactionPromptObserved />
    </Container>
}
export default observer(GameDetailsPage)

function GameActions() {
    const { gameDetails } = useStore()
    const { gameId } = useParams()

    if (gameDetails.status === GameStatus.New) return <Button variant='outlined' color='primary' size='large' onClick={gameDetails.start}>Start</Button>
    if (gameDetails.canPlay) return <Button variant='outlined' color='primary' size='large' component={Link} to={`/play/${gameId}`} replace>Play</Button>
}
const GameActionsObserved = observer(GameActions)

const CenterCardContent = styled(CardContent)({
    textAlign: 'center'
})

interface PlayerListProps {
    items: Player[]
    onClaim?: (player: Player) => void
}

function PlayerList({ items, onClaim }: PlayerListProps) {
    return <List dense disablePadding>
        { items.map(player => <ListItem key={player.number} disablePadding>
            <ListItemButton
                sx={{ justifyContent: 'space-between', gap: 3 }}
                onClick={() => onClaim && onClaim(player)}>
                <Stack direction='row' gap={2} alignItems='center'>
                    <Box sx={{ minWidth: '3ch', textAlign: 'right' }}>
                        <Typography variant='h6'>{player.number}</Typography>
                    </Box>
                    <ListItemText primary={<Typography fontWeight={player.isOwn ? 600 : 400}>{player.name}</Typography>} />
                </Stack>
                <Stack gap={4} direction='row'>
                    <Box>
                        <Typography variant='caption'>Orders History</Typography>
                        <History items={player.turns} prop='orders' />
                    </Box>
                    <Box>
                        <Typography variant='caption'>Times History</Typography>
                        <History items={player.turns} prop='times' />
                    </Box>
                </Stack>
            </ListItemButton>
        </ListItem>) }
    </List>
}

const Indicator = styled(Box)({
    width: '1ch',
    ':hover': {
        position: 'relative',
        transform: 'scale(1.2)'
    }
})

const YesIndicator = styled(Indicator)(({ theme }) => ({
    backgroundColor: theme.palette.success[theme.palette.mode]
}))

const NoIndicator = styled(Indicator)(({ theme }) => ({
    backgroundColor: theme.palette.error[theme.palette.mode]
}))

interface HistoryProps {
    items: TurnState[]
    prop: keyof TurnState
}

function History({ items, prop }: HistoryProps) {
    return <Stack direction='row' gap={.5} sx={{ height: '3ch' }}>
        {items.map(x => {
            const ItemIndicator = x[prop] ? YesIndicator : NoIndicator
            return <ItemIndicator key={x.turnNumber} title={x.turnNumber.toString()} />
        })}
    </Stack>
}

function ClaimFactionPrompt() {
    const store = useStore().gameDetails.claimFaction

    return <Dialog open={store.isOpen} onClose={store.close}>
        <DialogTitle>Claim <strong>{store.factionName} ({store.factionNumber})</strong></DialogTitle>
        <DialogContent>
            { !!store.error && <Alert severity="error" sx={{ mb: 4 }}>{store.error}</Alert> }
            <DialogContentText>
                To claim controle over the <strong>{store.factionName} ({store.factionNumber})</strong> faction provide password that you use to download orders from the game server.
            </DialogContentText>
            <TextField
                autoFocus
                disabled={store.isLoading}
                margin="dense"
                id="password"
                label="Password"
                type="password"
                fullWidth
                variant="outlined"
                value={store.password}
                onChange={store.onPasswordChange}
            />
        </DialogContent>
        <DialogActions>
            <Button disabled={store.isLoading} onClick={store.close}>Cancel</Button>
            <Box sx={{ position: 'relative' }}>
                <Button disabled={store.isLoading} variant='outlined' onClick={store.claim}>Claim</Button>
                { store.isLoading && <CircularProgress size={18}
                    sx={{
                        position: 'absolute',
                        top: '50%',
                        left: '50%',
                        marginTop: '-9px',
                        marginLeft: '-9px',
                    }}
                />
                }
            </Box>
        </DialogActions>
    </Dialog>
}
const ClaimFactionPromptObserved = observer(ClaimFactionPrompt)
