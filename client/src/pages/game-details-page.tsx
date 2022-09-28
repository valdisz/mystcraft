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
import { useStore } from '../store'

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
        content = <>
            <Stack mb={4} direction='row' justifyContent='center' gap={4}>
                <Card elevation={0} variant='outlined'>
                    <CenterCardContent>
                        <Typography variant='caption'>Turn</Typography>
                        <Typography variant='h5'>{gameDetails.turnNumber}</Typography>
                    </CenterCardContent>
                </Card>
                <Card elevation={0} variant='outlined'>
                <CenterCardContent>
                        <Typography variant='caption'>Players</Typography>
                        <Typography variant='h5'>{gameDetails.playerCount}</Typography>
                    </CenterCardContent>
                </Card>
                <Card elevation={0} variant='outlined'>
                    <CenterCardContent>
                        <Typography variant='caption'>Local Players</Typography>
                        <Typography variant='h5'>{gameDetails.locaPlayerCount}</Typography>
                    </CenterCardContent>
                </Card>
            </Stack>
            <Paper elevation={0} variant='outlined'>
                <List dense disablePadding>
                    { gameDetails.players.map(player => <ListItem key={player.number} disablePadding>
                        <ListItemButton sx={{ justifyContent: 'space-between', gap: 3 }} onClick={() => gameDetails.claim(player)}>
                            <ListItemText
                                primary={<Typography fontWeight={player.isOwn ? 600 : 400}>{player.name}</Typography>}
                                secondary={<Typography variant='caption' component='div'>{player.isClaimed ? 'Claimed' : 'Remote'}</Typography>}
                            />
                            <Stack direction='row' gap={3}>
                                <Chip sx={{ opacity: player.orders ? 1 : 0 }} label='Orders' />
                                <Chip sx={{ opacity: player.times ? 1 : 0 }} label='Times' />
                            </Stack>
                        </ListItemButton>
                    </ListItem>) }
                </List>
            </Paper>
        </>
    }

    return <Container>
        <PageTitle
            title={gameDetails.name || 'Loading...'}
            back='/'
            actions={gameDetails.canPlay && <Button variant='outlined' color='primary' size='large' component={Link} to={`/play/${gameId}`}>Play</Button>}
        />
        { content }
        <ClaimFactionPromptObserved />
    </Container>
}
export default observer(GameDetailsPage)

const CenterCardContent = styled(CardContent)({
    textAlign: 'center'
})

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
