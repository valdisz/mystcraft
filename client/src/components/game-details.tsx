import React from 'react'
import { observer } from 'mobx-react-lite'
import { Box, Container, Stack, Alert, CircularProgress, ButtonGroup, Button, AlertTitle, Paper, LinearProgress, List, Typography, IconButton, Chip, Avatar } from '@mui/material'
import PageTitle from './page-title'
import { GameDetailsStore } from '../store'
import { Operation, OperationError } from '../store/connection'
import { EmptyListItem } from './bricks'

import IconPlay from '@mui/icons-material/PlayArrow'
import IconPause from '@mui/icons-material/Pause'
import IconStop from '@mui/icons-material/Stop'
import IconNext from '@mui/icons-material/SkipNext'
import IconRepeat from '@mui/icons-material/Repeat'
import IconMore from '@mui/icons-material/MoreVert'
import LayersIcon from '@mui/icons-material/Layers'
import { styled } from '@mui/system'

export interface GameDetailsProps {
    store: GameDetailsStore
}

const ChipLabel = styled(Avatar)({
    width: 'auto !important',
    borderRadius: '9px',
    paddingLeft: '2px',
    paddingRight: '2px',
})

function GameDetails({ store }: GameDetailsProps) {
    // reading all observable variables into local variables to indicate that this component is observing them
    const isLoading = store.isLoading
    const error = store.source.error
    const game = store.game

    const operation: Operation<OperationError> = {
        isLoading: false,
        isFailed: false,
        error: null,
        isIdle: false,
        isReady: true,
        state: 'ready',
        reset: null,
    }

    // players
    const players = []
    const items = {
        isEmpty: false,
        map: (item: any) => {
            return null
        }
    }
    const content = items.map(players)

    const actions = (
        <Button color='primary' size='large'>Join Game</Button>
    )

    return (
        <Container>
            {isLoading && (
                <Box sx={{ m: 4, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <CircularProgress />
                </Box>
            )}

            {error && (
                <Alert severity='error'>{error.message}</Alert>
            )}

            {(!isLoading && !error) && (
                <Stack gap={6}>
                    <PageTitle title={game.name} back='/' actions={actions} />

                    <Stack direction='row' alignItems='center' gap={1}>
                        <Chip size='small' avatar={<LayersIcon />} label='3' />
                        <Chip size='small' avatar={<Avatar title='nexus'>nx</Avatar>} label={<span>1&times;1</span>} />
                        <Chip size='small' avatar={<Avatar title='surface'>su</Avatar>} label={<span>64&times;64</span>} />
                        <Chip size='small' avatar={<Avatar title='underworld'>uw</Avatar>} label={<span>32&times;32</span>} />
                    </Stack>

                    {/* Game master controls */}
                    <Stack direction='row' alignItems='center'>
                        <ButtonGroup size='medium'>
                            <Button startIcon={<IconPlay />}>Start</Button>
                            <Button startIcon={<IconPause />}>Pause</Button>
                            <Button startIcon={<IconStop />}>Stop</Button>
                            <Button startIcon={<IconRepeat />}>Re-run Turn</Button>
                            <Button startIcon={<IconNext />}>Next Turn</Button>
                        </ButtonGroup>

                        <Box sx={{ flex: 1 }} />

                        <ButtonGroup size='medium'>
                            <Button variant='text'>View Logs</Button>
                            <IconButton>
                                <IconMore />
                            </IconButton>
                        </ButtonGroup>
                    </Stack>

                    {/* Description */}
                    <Box>
                        <Typography align='justify'>
                            Step into the enigmatic realm of Atlantis, a Play-By-Email (PBEM) game that defies the conventional boundaries of strategy and role-playing, all accessible through a user-friendly website interface. Unlike traditional PBEM games, Atlantis is not an endless loop of resource gathering and skirmishes; it offers a tantalizing victory condition that keeps players on the edge of their seats.
                        </Typography>
                        <Typography align='justify'>
                            Set in a mythical underwater kingdom teetering on the brink of apocalypse, you assume the role of a faction leader vying for control of the <strong>Heart of Atlantis</strong> &mdash; a mystical artifact said to possess the power to either save or doom the realm. With a rich tapestry of lore, complex economic systems, and a myriad of magical and military units at your disposal, the game challenges you to forge alliances, outwit enemies, and navigate moral quandaries.
                        </Typography>
                        <Typography align='justify'>
                            Will you be the savior who unites Atlantis or the conqueror who plunges it into eternal darkness? The fate of an entire civilization rests on your strategic acumen. Welcome to Atlantis, where every decision echoes in the annals of history.
                        </Typography>
                    </Box>

                    { operation.isFailed && <Alert severity="error">
                        <AlertTitle>Error</AlertTitle>
                        { operation.error.message }
                    </Alert> }

                    <Paper elevation={0} variant='outlined'>
                        { operation.isLoading && <LinearProgress /> }

                        <List dense disablePadding>
                            { items.isEmpty
                                ? <EmptyListItem>{operation.isLoading ? 'Loading...' : null}</EmptyListItem>
                                : content
                            }
                        </List>
                    </Paper>

                    <p>Player controls: Join Game, Quit Game, Claim Faction</p>
                    <p>Delete Game</p>
                    <p>Schedule</p>
                    <p>Articles</p>
                    <p>Player List</p>
                    <p>Map information</p>
                    <p>Ruleset</p>
                    <p>Player turn history</p>
                    <p>Player article history</p>
                </Stack>
            )}

        </Container>
    )
}

export default observer(GameDetails)
