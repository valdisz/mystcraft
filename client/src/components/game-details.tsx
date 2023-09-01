import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    Box, Container, Stack, Alert, CircularProgress, ButtonGroup, Button, AlertTitle, Paper, LinearProgress, List, Typography, Chip,
    Avatar, MenuItem, Tabs, Tab, Divider, ListItem, ListItemButton, ListItemText, styled, Switch, FormControlLabel
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
import { blue, green, orange, red } from '@mui/material/colors'
import { BoxProps } from '@mui/system'

const gameMasterContext = React.createContext(false)

function useGameMaster() {
    return React.useContext(gameMasterContext)
}

const Indicator = styled(Box)({
    width: '4px',
    height: '8px',
    ':hover': {
        position: 'relative',
        transform: 'scale(1.2)'
    }
})

const YesIndicator = styled(Indicator)(({ theme }) => ({
    backgroundColor: theme.palette.success[theme.palette.mode]
}))

const NoIndicator = styled(Indicator)(({ theme }) => ({
    backgroundColor: theme.palette.warning[theme.palette.mode]
}))

const IndeterminateIndicator = styled(Indicator)(({ theme }) => ({
    backgroundColor: theme.palette.grey[400]
}))

interface HistoryProps {
    items: TurnState[]
    prop: keyof TurnState
}

function History({ items, prop }: HistoryProps) {
    return <Stack direction='row' gap='2px'>
        {items.map(x => {
            const ItemIndicator = x[prop] ? YesIndicator : NoIndicator
            return <ItemIndicator key={x.turnNumber} title={x.turnNumber.toString()} />
        })}
            {/* TODO: replace with actual data from props */}
            <Divider sx={theme => ({ mx: .5, borderColor: theme.palette.grey[700] })} orientation='vertical' flexItem />
            <IndeterminateIndicator />
        </Stack>
}

export interface GameDetailsProps {
    store: GameDetailsStore
}

function turns(num: number): TurnState[] {
    const result: TurnState[] = []
    for (let i = 1; i <= num; i++) {
        result.push({
            turnNumber: i,
            orders: Math.trunc(Math.random() * 100) % 2 === 0,
            times: Math.trunc(Math.random() * 100)  % 3 === 0
        })
    }
    return result
}

const NEW_FACTIONS: Player[] = [
    {
        "id": "21",
        "number": null,
        "isClaimed": false,
        "isOwn": false,
        stance: 'neutral',
        "name": "The Enclave of Elementalists",
        turns: []
    },
    {
        "id": "22",
        "number": null,
        "isClaimed": false,
        "isOwn": false,
        stance: 'neutral',
        "name": "The Society of Alchemists",
        turns: []
    },
    {
        "id": "23",
        "number": null,
        "isClaimed": false,
        "isOwn": false,
        stance: 'neutral',
        "name": "The Legion of the Fallen",
        turns: []
    }
]


const ITEMS: Player[] = [
    {
        "id": "1",
        "number": 3,
        "isClaimed": true,
        "stance": "own",
        "name": "The Atlantean Empire",
        turns: turns(5)
    },
    {
        "id": "2",
        "number": 4,
        "isClaimed": true,
        "stance": "ally",
        "name": "Brotherhood of the Black Flag",
        turns: turns(5)
    },
    {
        "id": "3",
        "number": 5,
        "isClaimed": false,
        "stance": "ally",
        "name": "Order of the Golden Dawn",
        turns: turns(5)
    },
    {
        "id": "4",
        "number": 6,
        "isClaimed": true,
        "stance": "ally",
        "name": "The Elven Dominion",
        turns: turns(5)
    },
    {
        "id": "5",
        "number": 7,
        "isClaimed": true,
        "stance": "friendly",
        "name": "The Dwarven Coalition",
        turns: turns(5)
    },
    {
        "id": "6",
        "number": 8,
        "isClaimed": false,
        "stance": "friendly",
        "name": "The Cult of the Dragon",
        turns: turns(5)
    },
    {
        "id": "7",
        "number": 9,
        "isClaimed": true,
        "stance": "unfriendly",
        "name": "The Kingdom of the Sun",
        turns: turns(5)
    },
    {
        "id": "8",
        "number": 10,
        "isClaimed": false,
        "stance": "unfriendly",
        "name": "The Shadow Syndicate",
        turns: turns(5)
    },
    {
        "id": "9",
        "number": 11,
        "isClaimed": true,
        "stance": "hostile",
        "name": "The Council of Mages",
        turns: turns(5)
    },
    {
        "id": "10",
        "number": 12,
        "isClaimed": true,
        "stance": "hostile",
        "name": "The Orcish Horde",
        turns: turns(5)
    },
    {
        "id": "11",
        "number": 13,
        "isClaimed": false,
        "stance": "hostile",
        "name": "The Guild of Thieves",
        turns: turns(5)
    },
    {
        "id": "12",
        "number": 14,
        "isClaimed": true,
        "stance": "neutral",
        "name": "The Knights of the Holy Grail",
        turns: turns(5)
    },
    {
        "id": "13",
        "number": 15,
        "isClaimed": false,
        "stance": "neutral",
        "name": "The Gnomish Inventors",
        turns: turns(5)
    },
    {
        "id": "14",
        "number": 16,
        "isClaimed": true,
        "stance": "neutral",
        "name": "The Celestial Guardians",
        turns: turns(5)
    },
    {
        "id": "15",
        "number": 17,
        "isClaimed": true,
        "stance": "neutral",
        "name": "The Dark Necromancers",
        turns: turns(5)
    },
    {
        "id": "16",
        "number": 18,
        "isClaimed": false,
        "stance": "neutral",
        "name": "The Druidic Circle",
        turns: turns(5)
    },
    {
        "id": "17",
        "number": 19,
        "isClaimed": true,
        "stance": "neutral",
        "name": "The Amazon Warriors",
        turns: turns(5)
    },
    {
        "id": "18",
        "number": 20,
        "isClaimed": false,
        "stance": "neutral",
        "name": "The Sea Serpent Pirates",
        turns: turns(5)
    },
    {
        "id": "19",
        "number": 21,
        "isClaimed": true,
        "stance": "neutral",
        "name": "The Vampire Coven",
        turns: turns(5)
    },
    {
        "id": "20",
        "number": 22,
        "isClaimed": false,
        "stance": "neutral",
        "name": "The Werewolf Clan",
        turns: turns(5)
    }
]

interface FactionItemProps {
    player: Player
    withTurns: boolean
    withActions: boolean
    canClaim: boolean
    canContact: boolean
}

function FactionItem({ canClaim, canContact, player, withActions, withTurns }: FactionItemProps) {
    const isGameMaster = useGameMaster()
    return (
        <tr>
            <td className='shrink right'>
                <Typography variant='body1' fontSize='1rem' sx={{ minWidth: '3ch', textAlign: 'right' }}>{player.number}</Typography>
            </td>
            <td>
                <Typography fontWeight={player.isOwn ? 600 : 400}>{player.name}</Typography>
            </td>
            {withTurns && (
                <>
                    <td className='shrink right'>
                        <History items={player.turns} prop='orders' />
                    </td>
                    <td className='shrink right'>
                        <History items={player.turns} prop='times' />
                    </td>
                </>
            )}
            {withActions && (
                <td className='shrink right'>
                    <ButtonGroup size='small'>
                        {canClaim && (
                            <Button>Claim</Button>
                        )}
                        {canContact && (
                            <Button>Contact</Button>
                        )}
                        {isGameMaster && (
                            <MenuIconButton icon={<IconMore />} useButton>
                                <MenuItem>Quit</MenuItem>
                                <MenuItem>Delete</MenuItem>
                            </MenuIconButton>
                        )}
                    </ButtonGroup>
                </td>
            )}
        </tr>
    )
}

interface FactionsTableProps extends BoxProps {
    items: Player[]
    caption: string
    inGame: boolean
    canContact: (player: Player) => boolean
    canClaim: (player: Player) => boolean
}

function FactionsTable({ items, caption, inGame, canContact, canClaim, sx, ...props }: FactionsTableProps) {
    const isGameMaster = useGameMaster()

    const withActions = isGameMaster || items.some(x => canContact(x) || canClaim(x))
    const withTurns = items.some(x => x.turns.length > 0)

    return (
        <Box
            component='table'
            sx={[
                {
                    borderCollapse: 'collapse',
                    '.shrink': {
                        width: 0
                    },
                    '.left': {
                        textAlign: 'left'
                    },
                    '.right': {
                        textAlign: 'right'
                    },
                    'td, th': {
                        px: 2,
                        py: 2,
                        verticalAlign: 'baseline',
                    },
                    'th': {
                        py: 0
                    },
                    'td': {
                        borderColor: 'divider',
                        borderStyle: 'solid',
                        borderWidth: 0,
                        borderTopWidth: 1,
                        borderBottomWidth: 1,
                    },
                    'td:first-child': {
                        borderLeftWidth: 1,
                    },
                    'td:last-child': {
                        borderRightWidth: 1,
                    },
                },
                ...(Array.isArray(sx) ? sx : [sx])
            ]}
            {...props}
        >
            <thead>
                <tr>
                    <th className='left' colSpan={2}>
                        <Typography variant='caption'>{caption}</Typography>
                    </th>
                    {withTurns && (
                        <>
                            <th className='shrink right'>
                                <Typography variant='caption'>Orders</Typography>
                            </th>
                            <th className='shrink right'>
                                <Typography variant='caption'>Times</Typography>
                            </th>
                        </>
                    )}
                    {withActions && (
                        <th className='shrink right' />
                    )}
                </tr>
            </thead>
            <tbody>
                {items.map(item => <FactionItem key={item.number} player={item} withTurns={withTurns} withActions={withActions} canClaim={canClaim(item)} canContact={canContact(item)} />)}
            </tbody>
        </Box>
    )

}

function GameDetails({ store }: GameDetailsProps) {
    // TODO: replace with routing
    const [pane, setPane] = React.useState('description')
    const handleTabChange = (event: React.SyntheticEvent, newValue: string) => {
        setPane(newValue)
    }

    // TODO: replace with store
    const [inGame, setInGame] = React.useState(false)
    const handleJoinGame = () => {
        setInGame(!inGame)
    }

    const [isGameMaster, setGameMaster] = React.useState(false)
    const handleToggleGameMaster = (event: React.ChangeEvent<HTMLInputElement>) => {
        setGameMaster(event.target.checked);
    }

    // reading all observable variables into local variables to indicate that this component is observing them
    const isLoading = store.isLoading
    const error = store.source.error
    const game = store.game

    const actions = (
        <Stack direction='row' alignItems='center' gap={1}>
            <Button color='primary' size='large' onClick={handleJoinGame}>{ inGame ? 'Open Game' : 'Join Game'}</Button>
            {inGame && (
                <MenuIconButton icon={<IconMore />}>
                    <MenuItem>Quit</MenuItem>
                </MenuIconButton>
            )}
        </Stack>
    )

    const title = (
        <Stack direction='row' alignItems='center' gap={1}>
            <GameStatusIcon status={game.status} />
            <Typography variant='h3'>{game.name}</Typography>
        </Stack>
    )

    return (
        <gameMasterContext.Provider value={isGameMaster}>
            <Container>
                <Box
                    sx={{
                        position: 'fixed',
                        bottom: 0,
                        left: 0,
                        p: 2,
                    }}
                >
                    <FormControlLabel
                        control={
                            <Switch
                                checked={isGameMaster}
                                onChange={handleToggleGameMaster}
                                inputProps={{ 'aria-label': 'controlled' }}
                            />
                        }
                        label="Game Master Mode"
                    />
                </Box>
            {isLoading && (
                <Box sx={{ m: 4, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <CircularProgress />
                </Box>
            )}

            {error && (
                <Alert severity='error'>{error.message}</Alert>
            )}

            {(!isLoading && !error) && (
                <Stack
                    gap={6}
                    sx={{
                        mb: 6
                    }}
                >
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
                    {isGameMaster && (
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
                                <MenuIconButton icon={<IconMore />} useButton ButtonProps={{ variant: 'text', size: 'medium' }}>
                                    <MenuItem>Change Schedule</MenuItem>
                                    <MenuItem>Change Engine</MenuItem>
                                    <Divider />
                                    <MenuItem>Delete</MenuItem>
                                </MenuIconButton>
                            </Stack>
                        </Stack>
                    )}


                    <Tabs value={pane} onChange={handleTabChange}>
                        <Tab label='Description' value='description'  />
                        <Tab label={
                            <Typography>
                                Factions
                                <Chip size='small' label='3' />
                            </Typography>
                        } value='factions' />
                        <Tab label='Articles' value='articles' />
                        <Tab label='Rules' value='rules' />
                        <Tab label='Lore' value='lore' />
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

                    {/* Factions */}
                    {pane === 'factions' && (
                        <>
                            <FactionsTable
                                items={NEW_FACTIONS}
                                caption={'New Factions'}
                                inGame={inGame}
                                canClaim={player => false}
                                canContact={player => false}
                            />
                            {inGame
                                ? (
                                    <>
                                        <FactionsTable
                                            items={ITEMS.filter(player => player.stance === 'own')}
                                            caption={'My Faction'}
                                            inGame={inGame}
                                            canClaim={player => false}
                                            canContact={player => false}
                                            sx={{
                                                'td': {
                                                    bgcolor: blue[50],
                                                }

                                            }}
                                        />
                                        <FactionsTable
                                            items={ITEMS.filter(player => player.stance === 'ally')}
                                            caption={'Allies'}
                                            inGame={inGame}
                                            canClaim={player => false}
                                            canContact={player => inGame}
                                            sx={{
                                                'td': {
                                                    bgcolor: green[100],
                                                }

                                            }}
                                        />
                                        <FactionsTable
                                            items={ITEMS.filter(player => player.stance === 'friendly')}
                                            caption={'Friends'}
                                            inGame={inGame}
                                            canClaim={player => false}
                                            canContact={player => inGame}
                                            sx={{
                                                'td': {
                                                    bgcolor: green[50],
                                                }

                                            }}
                                        />
                                        <FactionsTable
                                            items={ITEMS.filter(player => player.stance === 'hostile')}
                                            caption={'Enemies'}
                                            inGame={inGame}
                                            canClaim={player => false}
                                            canContact={player => inGame}
                                            sx={{
                                                'td': {
                                                    bgcolor: red[50],
                                                }
                                            }}
                                        />
                                        <FactionsTable
                                            items={ITEMS.filter(player => player.stance === 'unfriendly')}
                                            caption={'Rivals'}
                                            inGame={inGame}
                                            canClaim={player => false}
                                            canContact={player => inGame}
                                            sx={{
                                                'td': {
                                                    bgcolor: orange[50],
                                                }

                                            }}
                                        />
                                        <FactionsTable
                                            items={ITEMS.filter(player => player.stance === 'neutral')}
                                            caption={'Neutrals'}
                                            inGame={inGame}
                                            canClaim={player => false}
                                            canContact={player => inGame}
                                        />
                                    </>
                                )
                                : (
                                    <FactionsTable
                                        items={ITEMS}
                                        caption={'Factions'}
                                        inGame={inGame}
                                        canClaim={player => !player.isClaimed}
                                        canContact={player => inGame}
                                    />
                                )
                            }
                        </>
                    )}

                    {/* Articles */}
                    {pane === 'articles' && (
                        <>
                            <p>ToDo: articles</p>
                        </>
                    )}

                    {/* Rules */}
                    {pane === 'rules' && (
                        <>
                            <p>ToDo: rules</p>
                        </>
                    )}

                    {/* Lore */}
                    {pane === 'lore' && (
                        <>
                            <p>ToDo: lore</p>
                        </>
                    )}
                </Stack>
            )}
        </Container>
        </gameMasterContext.Provider>
    )
}

export default observer(GameDetails)
