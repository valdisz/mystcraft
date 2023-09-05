import React from 'react'
import { observer } from 'mobx-react-lite'
import {
    styled,
    Box, Container, Stack, Alert, CircularProgress, ButtonGroup, Button, Typography, Chip,
    Avatar, Tabs, Tab, Divider, Switch, FormControlLabel, Pagination,
    List, ListItemButton, ListItemText,
} from '@mui/material'
import PageTitle from './page-title'
import { GameDetailsStore, Player, TurnState } from '../store'
import { ActionGroup, ActionItem } from './bricks'

import IconPlay from '@mui/icons-material/PlayArrow'
import IconPause from '@mui/icons-material/Pause'
import IconStop from '@mui/icons-material/Stop'
import IconNext from '@mui/icons-material/SkipNext'
import IconRepeat from '@mui/icons-material/Repeat'

import LayersIcon from '@mui/icons-material/Layers'
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import MiscellaneousServicesIcon from '@mui/icons-material/MiscellaneousServices'
import GameStatusIcon from './game-status-icon'
import { blue, green, orange, red } from '@mui/material/colors'
import { BoxProps, SxProps, Theme } from '@mui/system'

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

const FACTION_NUMBER_STYLE: SxProps<Theme> = {
    minWidth: '3ch',
    textAlign: 'right'
}

function FactionItem({ canClaim, canContact, player, withActions, withTurns }: FactionItemProps) {
    const isGameMaster = useGameMaster()

    const actions: ActionItem[] = []
    if (canClaim) {
        actions.push({ content: 'Claim', onAction: () => {} })
    }

    if (canContact) {
        actions.push({ content: 'Contact', onAction: () => {} })
    }

    const gameMasterActions: ActionItem[] = isGameMaster
        ? [
            { content: 'Quit', onAction: () => { } },
            { content: 'Delete', onAction: () => { } },
        ]
        : []

    return (
        <tr>
            <td className='shrink right'>
                {player.number
                    ? <Typography variant='body1' sx={FACTION_NUMBER_STYLE}>{player.number}</Typography>
                    : <Typography variant='body2' sx={FACTION_NUMBER_STYLE}>new</Typography>
                }
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
                    <ActionGroup size='small' actions={actions} additionalActions={gameMasterActions} />
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
                        py: 0,
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

function GameMasterControls() {
    return (
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

                <ActionGroup
                    size='medium'
                    variant='text'
                    actions={[
                        { content: 'View Logs', onAction: () => { } },
                    ]}
                    additionalActions={[
                        { content: 'Change Schedule', onAction: () => { } },
                        { content: 'Change Engine', onAction: () => { } },
                        '---',
                        { content: 'Delete', onAction: () => { } },
                    ]}
                />
            </Stack>
        </Stack>
    )
}

function Description() {
    return (
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
    )
}

interface FactionsProps {
    inGame?: boolean
}

function Factions({ inGame }: FactionsProps) {
    return (
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
    )
}

const LONG_ARTICLE = `
  _.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._.-=-._
.----      - ---     --     ---   -----   - --       ----  ----   -     ----.
 )                                                                         (
(                             NewOrigins Events                             )
 )                               May, Year 5                               (
(                                                                           )
 )     Numerous wanderers have heard about a fight between combatants      (
(        happened in the desert of Great Bletchvil Desert where many        )
 )      soldiers will never fight again. This battle will be known as      (
(                  Battle near the ocean of Lochael River.                  )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(     Handful peasants are worried about the battle between two armies      )
 )    happened in the mountains of Ibinmor Heights where 1combatant was    (
(      killed. The defenders were furious and put all attackers to the      )
 )    sword. This battle will be known as Siege of the Magical Fortress    (
(                               Building [9].                               )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(        Multiple druids have heard about the battle between enemies        )
 )      happened in the desert of Great Perikeros Desert where battle      (
(       ended in a slaughter of one of the sides. This battle will be       )
 )            known as Battle near the ocean of Sinalizo Pond.             (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )     Dozen merchants have heard about a fight between hostile forces     (
(     happened in the desert of Great Lurgbuz Desert where battle ended     )
 )    in a slaughter of one of the sides. This battle will be known as     (
(                  Battle near the ocean of Gwalall River.                  )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(     Many locals are worried about the village of Balkhagal that lies      )
 )     in the Great Morwaz Desert desert, where a partisans destroyed      (
(                                  guards.                                  )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(      Many peasants are discussing about the city of 38 Epikatarirmos      )
 )    that lies in the Glanar River swamps, where an opposition put to     (
(                             the sword guards.                             )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(     Many refugees are rumoring about hunters who have murdered Sphinx     )
 )      (20307) near mountains of Durwaz Rocks. Freeing the desert of      (
(                 Great Bletchvil Desert from their terror.                 )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(       Some pilgrims have heard about witchers who have extinguished       )
 )      Undead (18600) near volcanoes of Lurggar Volcano. Freeing the      (
(            mountains of Bolagal Mountains from their anxiety.             )
 )                                                                         (
(                          .:*~*:._.:*~*:._.:*~*:.                          )
 )                                                                         (
(         Numerous locals are whispering about daredevils who have          )
 )      decimated Giant Scorpions (391) near volcanoes of Bletchgrove      (
(        Peak. Freeing the desert of Great Volbuz Desert from their         )
 )                                 horror.                                 (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )      In the desert of Great Birmington Desert, near ocean of Great      (
(     Methan River, Giant Scorpions (134) who continue to cause horror      )
 )                          to local inhabitants.                          (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )     In the desert of Great Bletchgrove Desert, near ocean of Great      (
(     Lochash River, Giant Scorpions (108) who continue to cause terror     )
 )                          to local inhabitants.                          (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )    In the desert of Great Volbuz Desert, near forests of Ametanopos     (
(        Forest, Sphinx (631) who continue to cause terror to local         )
 )                              inhabitants.                               (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )         In the desert of Great Ialihn Desert, near mountains of         (
(      Antanoatres Peak, Undead (18828) who continue to cause fear to       )
 )                           local inhabitants.                            (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )       Worrisome news were coming from the desert of Great Dunlock       (
(      Desert. Inhabitants were shocked by the assassination near the       )
 )    mountains of Khimilmud Mountains. The assassin escaped unnoticed.    (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )      Frightening news were coming from the desert of Great Durbuz       (
(      Desert. Commoners were terrified by the assassination near the       )
 )       ocean of Great Amain River. The assassin escaped unnoticed.       (
(                                                                           )
 )                         .:*~*:._.:*~*:._.:*~*:.                         (
(                                                                           )
 )      Horrifying news were coming from the desert of Great Dunlock       (
(        Desert. Citizens were afraid by the assassination near the         )
 )    mountains of Khimilmud Mountains. The assassin escaped unnoticed.    (
(__       _       _       _       _       _       _       _       _       __)
   '-._.-' (___ _) '-._.-' '-._.-' )     ( '-._.-' '-._.-' (__ _ ) '-._.-'
           ( _ __)                (_     _)                (_ ___)
           (__  _)                 '-._.-'                 (___ _)
           '-._.-'                                         '-._.-'
`


function Articles() {
    return (
        <>
            <Stack
                direction='row'
                justifyContent='center'
            >
                <Stack>
                    <Typography variant='caption'>Issue</Typography>
                    <Pagination count={10} defaultPage={10} />
                </Stack>
            </Stack>

            <Stack
                direction='row'
                gap={2}
            >
                <List>
                    <ListItemButton>
                        <ListItemText primary='Quests' />
                    </ListItemButton>
                    <ListItemButton>
                        <ListItemText primary='Articles' />
                    </ListItemButton>
                    <ListItemButton>
                        <ListItemText primary='Rumours' />
                    </ListItemButton>
                </List>

                <Box
                    sx={{
                        flex: 1,
                        minWidth: 0,
                        display: 'flex',
                        flexDirection: 'column',
                        alignItems: 'center',
                    }}
                >
                    <Typography whiteSpace='pre'>
                        {LONG_ARTICLE}
                    </Typography>
                </Box>
            </Stack>
        </>
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
        <ActionGroup
            size='large'
            color='primary'
            variant='contained'
            slotProps={{
                trigger: {
                    variant: 'text',
                }
            }}
            actions={[
                { content: inGame ? 'Open Game' : 'Join Game', onAction: handleJoinGame },
            ]}
            additionalActions={[
                ...(inGame
                    ? [
                        { content: 'Quit', onAction: () => { } },
                    ]
                    : []
                )
            ]}
        />
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
                        <GameMasterControls />
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
                        <Description />
                    )}

                    {/* Factions */}
                    {pane === 'factions' && (
                        <Factions inGame={inGame} />
                    )}

                    {/* Articles */}
                    {pane === 'articles' && (
                        <Articles />
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
