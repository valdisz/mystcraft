import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    Box, Button, Dialog, DialogActions,
    DialogContent, DialogContentText, DialogTitle, TextField, Alert, CircularProgress
} from '@mui/material'
import { GameDetails } from '../components'
import { useLoaderData, useParams } from 'react-router'
import { useStore, Player, TurnState, GameDetailsStore } from '../store'

export default function GameDetailsPage() {
    const store = useLoaderData() as GameDetailsStore;
    return <GameDetails store={store} />
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
