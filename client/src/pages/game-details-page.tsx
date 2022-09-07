import React, { useEffect } from 'react'
import { observer } from 'mobx-react-lite'
import { Container, Stack, List, ListItem, ListItemButton, ListItemText, Typography, Paper, Button, Chip } from '@mui/material'
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
        content = 'Loading...'
    }
    else if (gameDetails.source.error) {
        content = 'Error...'
    }
    else {
        content = <List dense disablePadding>
            { gameDetails.players.map(x => <ListItem key={x.number} disablePadding>
                <ListItemButton sx={{ justifyContent: 'space-between', gap: 3 }}>
                    <ListItemText primary={<Typography component={x.id ? 'strong' : 'div'}>{x.name}</Typography>} secondary={<Typography variant='caption'>Remote</Typography>} />
                    <Stack direction='row' gap={3}>
                        <Chip sx={{ opacity: x.orders ? 1 : 0 }} label='Orders' />
                        <Chip sx={{ opacity: x.times ? 1 : 0 }} label='Times' />
                    </Stack>
                </ListItemButton>
            </ListItem>) }
        </List>
    }

    return <Container>
        <PageTitle
            title={gameDetails.name || 'Loading...'}
            back='/'
            actions={<Button variant='outlined' color='primary' size='large'>Open</Button>}
        />
        <Paper elevation={0} variant='outlined'>
            { content }
        </Paper>
    </Container>
}
export default observer(GameDetailsPage)
