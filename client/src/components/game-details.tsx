import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    Box, Container, Stack, Alert, CircularProgress, ButtonGroup, Button, AlertTitle, Paper, LinearProgress, List, Typography, Chip,
    Avatar, MenuItem, Tabs, Tab, Divider, ListItem, ListItemButton, ListItemText, styled
} from '@mui/material'
import PageTitle from './page-title'
import { GameDetailsStore, Player, TurnState } from '../store'
import { Operation, OperationError } from '../store/connection'
import { EmptyListItem } from './bricks'

import IconPlay from '@mui/icons-material/PlayArrow'
import IconPause from '@mui/icons-material/Pause'
import IconStop from '@mui/icons-material/Stop'
import IconNext from '@mui/icons-material/SkipNext'
import IconRepeat from '@mui/icons-material/Repeat'
import IconMore from '@mui/icons-material/MoreVert'
import { MenuIconButton } from './menu-icon-button'

import LayersIcon from '@mui/icons-material/Layers'
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import MiscellaneousServicesIcon from '@mui/icons-material/MiscellaneousServices'
import GameStatusIcon from './game-status-icon'

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

export interface GameDetailsProps {
    store: GameDetailsStore
}

function GameDetails({ store }: GameDetailsProps) {
    // TODO: replace with routing
    const [pane, setPane] = React.useState('description')
    const handleTabChange = (event: React.SyntheticEvent, newValue: string) => {
        setPane(newValue)
      };

    // reading all observable variables into local variables to indicate that this component is observing them
    const isLoading = store.isLoading
    const error = store.source.error
    const game = store.game

    const actions = (
        <Button color='primary' size='large'>Join Game</Button>
    )

    const title = <Stack direction='row' alignItems='center' gap={1}>
        <GameStatusIcon status={game.status} />
        <Typography variant='h3'>{game.name}</Typography>
    </Stack>

    const faction = (player: Player) => (
        <ListItem key={player.number} disablePadding>
            <ListItemButton
                sx={{
                    justifyContent: 'space-between',
                    gap: 3
                }}
            >
                <Stack direction='row' gap={2} alignItems='center'>
                    <Box sx={{ minWidth: '3ch', textAlign: 'right' }}>
                        <Typography variant='h6'>{player.number}</Typography>
                    </Box>
                    <ListItemText primary={<Typography fontWeight={player.isOwn ? 600 : 400}>{player.name}</Typography>} />
                </Stack>
                <Stack gap={4} direction='row'>
                    <Box>
                        <Typography variant='caption'>Orders</Typography>
                        <History items={player.turns} prop='orders' />
                    </Box>
                    <Box>
                        <Typography variant='caption'>Times</Typography>
                        <History items={player.turns} prop='times' />
                    </Box>
                </Stack>
            </ListItemButton>
        </ListItem>
    )

    const items: Player[] = [
        {
            id: '1',
            number: 3,
            isClaimed: true,
            isOwn: true,
            name: 'The Atlantean Empire',
            turns: [
                { turnNumber: 1, orders: true, times: true },
                { turnNumber: 2, orders: true, times: true },
                { turnNumber: 3, orders: true, times: false },
            ],
        },
        {
            id: '2',
            number: 4,
            isClaimed: true,
            isOwn: false,
            name: 'Brotherhood of the Black Flag',
            turns: [
                { turnNumber: 1, orders: true, times: true },
                { turnNumber: 2, orders: true, times: true },
                { turnNumber: 3, orders: true, times: true },
            ],
        },
        {
            id: '3',
            number: 5,
            isClaimed: false,
            isOwn: false,
            name: 'Order of the Golden Dawn',
            turns: [
                { turnNumber: 1, orders: true, times: false },
                { turnNumber: 2, orders: true, times: false },
                { turnNumber: 3, orders: true, times: false },
            ]
        },
    ]
    const content = items.map(faction)

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
                    <PageTitle title={title} back='/' actions={actions} />

                    <Stack direction='row' alignItems='center' gap={1}>
                        <Chip size='small' avatar={<CalendarMonthIcon />} label='MON,TUE,FRI @ 12:00' />
                        <Chip size='small' avatar={<LayersIcon />} label='3' />
                        <Chip size='small' avatar={<Avatar title='nexus'>nx</Avatar>} label={<span>1&times;1</span>} />
                        <Chip size='small' avatar={<Avatar title='surface'>sf</Avatar>} label={<span>64&times;64</span>} />
                        <Chip size='small' avatar={<Avatar title='underworld'>uw</Avatar>} label={<span>32&times;32</span>} />
                        <Chip size='small' avatar={<MiscellaneousServicesIcon />} label={'Atlantis 5 Standard'} />
                    </Stack>

                    {/* Game master controls */}
                    <Stack>
                        <Typography variant='caption'>Game Master Controls</Typography>
                        <Stack direction='row' alignItems='center'
                            sx={{
                                bgcolor: 'grey.100',
                            }}
                            >
                            <ButtonGroup size='medium'>
                                <Button startIcon={<IconPlay />}>Start</Button>
                                <Button startIcon={<IconPause />}>Pause</Button>
                                <Button startIcon={<IconStop />}>Stop</Button>
                                <Button startIcon={<IconRepeat />}>Re-run Turn</Button>
                                <Button startIcon={<IconNext />}>Next Turn</Button>
                            </ButtonGroup>

                            <Box sx={{ flex: 1 }} />

                            <Button size='medium' variant='text'>View Logs</Button>
                            <MenuIconButton icon={<IconMore />}>
                                <MenuItem>Change Schedule</MenuItem>
                                <MenuItem>Change Engine</MenuItem>
                                <Divider />
                                <MenuItem>Delete</MenuItem>
                            </MenuIconButton>
                        </Stack>
                    </Stack>

                    <Tabs value={pane} onChange={handleTabChange}>
                        <Tab label='Description' value='description'  />
                        <Tab label='Factions' value='factions' />
                        <Tab label='Articles' value='articles' />
                    </Tabs>

                    {/* Description */}
                    {pane === 'description' && (
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
                    )}

                    {pane === 'factions' && (
                        <Paper elevation={0} variant='outlined'>
                            <List dense disablePadding>
                                {content}
                            </List>
                        </Paper>
                    )}

                    {pane === 'articles' && (
                        <>
                            <p>ToDo:</p>
                            <p>Player controls: Quit Game, Claim Faction, Open Game</p>
                            <p>Articles</p>
                            <p>Ruleset</p>
                        </>
                    )}
                </Stack>
            )}

        </Container>
    )
}

export default observer(GameDetails)
