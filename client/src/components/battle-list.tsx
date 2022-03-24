import * as React from 'react'
import { Box, BoxProps, Stack, StackProps, Typography, List, ListSubheader, ListItemButton, Chip, Dialog,
    DialogTitle, DialogContent, DialogActions, Button, IconButton, Divider
} from '@mui/material'
import { Battle, BattleUnit, Faction as GameFaction, Region } from '../game'
import { FixedTypography } from './fixed-typography'
import CloseIcon from '@mui/icons-material/Close'
import LocationSearchingIcon from '@mui/icons-material/LocationSearching'
import { useStore } from '../store'
import { useMapContext } from '../map'

type Size = 'normal' | 'small'

interface ANumberProps {
    value: number
    size: Size
}

function ANumber({ value, size, sx = [], ...props }: ANumberProps & BoxProps) {
    return <Box {...props} sx={[
        (theme) => ({
            borderRadius: theme.shape.borderRadius,
            bgcolor: theme.palette.divider,
            px: 1,
        }),
        ...(Array.isArray(sx) ? sx : [sx])
    ]}>
        <Typography variant='caption'>{value}</Typography>
    </Box>
}

interface AnEntityProps {
    num?: number
    name: string
    size: Size
}

function AnEntity({ num, name, size, sx = [], ...props }: AnEntityProps & BoxProps) {
    return <Box {...props} sx={[
        {
            minWidth: 0,
            display: 'flex',
            gap: 1
        },
        ...(Array.isArray(sx) ? sx : [sx])
    ]}>
        <FixedTypography title={name}>{name}</FixedTypography>
        { num && <ANumber size={size} value={num} /> }
    </Box>
}

interface FactionProps {
    faction: GameFaction
}

function Faction({ faction: { num, name, known } }: FactionProps) {
    return known
        ? <AnEntity size='normal' num={num} name={name} />
        : <AnEntity size='normal' name='Unknown faction' />
}

interface UnitProps {
    unit: BattleUnit
}

function Unit({ unit: { number, name } }: UnitProps) {
    return <AnEntity size='normal' num={number} name={name} />
}

interface InfoCardProps {
    title: string
    value: React.ReactNode
    change?: React.ReactNode
}

function InfoCard({ title, value, change }: InfoCardProps) {
    return <Stack alignItems='center'>
        <Typography variant='caption' sx={{ color: 'text.secondary' }}>{title}</Typography>
        <Stack direction='row' justifyContent='center' alignItems='baseline' spacing={1}>
            <Typography variant='h5'>{value}</Typography>
            {change && <Typography variant='body1' sx={{ color: 'text.secondary' }}>{change}</Typography> }
        </Stack>
    </Stack>
}

interface ArmyProps extends StackProps {
    side: string
    leader: BattleUnit
    troops: number
    lost: number
    victory: boolean
}

function Army({ side, leader, troops, lost, victory, sx, ...props }: ArmyProps) {
    return <Stack {...props} sx={[{
                flex: 1,
                minWidth: 0,
                my: 2
            },
            ...(Array.isArray(sx) ? sx : [sx])
        ]} spacing={2}>
            <Stack>
                <Stack direction='row' spacing={1} alignItems='center'>
                    <Typography variant='caption' sx={{ color: 'text.secondary' }}>{side}</Typography>
                    { victory && <Chip color='success' label='victory' size='small' sx={{
                        py: 0, px: 1,
                        height: 'auto',
                        '& .MuiChip-label': {
                            p: 0
                        }
                    }} /> }
                </Stack>
                <Faction faction={leader.faction} />
            </Stack>
            <Stack>
                <Typography variant='caption' sx={{ color: 'text.secondary' }}>Unit</Typography>
                <Unit unit={leader} />
            </Stack>
            <Stack direction='row' justifyContent='space-around' alignItems='center' spacing={2}>
                <InfoCard title='Troops' value={troops} />
                <InfoCard title='Lost' value={lost} change={`${((lost / troops) * 100).toFixed(0)}%`} />
            </Stack>
        </Stack>
}

interface BattleItemProps {
    battle: Battle
    onClick: () => void
}

function countSoldiers(units: Iterable<BattleUnit>) {
    let count = 0
    for (const unit of units) {
        for (const item of unit.items) {
            if (!item.isManLike) {
                continue
            }

            count += item.amount
        }
    }

    return count
}

function BattleItem({ battle, onClick }: BattleItemProps) {
    const { attacker, defender, attackers, defenders, attackerCasualties, defenderCasualties } = battle

    const attackingTroops = countSoldiers(attackers)
    const defendungTroops = countSoldiers(defenders)

    return <ListItemButton divider onClick={onClick}>
            <Box sx={{
                flex: 1,
                display: 'flex',
                flexDirection: 'row',
                justifyContent: 'space-between',
                alignItems: 'stretch',
                gap: 2
            }}>
                <Army side='Attacker' leader={attacker} victory={true} troops={attackingTroops} lost={attackerCasualties.lost} />
                <Divider orientation='vertical' flexItem />
                <Army side='Defender' leader={defender} victory={false} troops={defendungTroops} lost={defenderCasualties.lost} />
            </Box>
    </ListItemButton>
}

export interface BattleListProps {
    battles: Battle[]
}

export function BattleList({ battles }: BattleListProps) {
    const mapContext = useMapContext()
    const { game } = useStore()
    const [ open, setOpen ] = React.useState(false)
    const groups = React.useMemo(() => {
        const items = new Map<Region, Battle[]>()
        for (const battle of battles) {
            const r = battle.region
            if (!items.has(r)) {
                items.set(r, [])
            }

            const g = items.get(r)
            g.push(battle)
        }

        return items
    }, [ battles ])

    function openDialog() {
        setOpen(true)
    }

    function closeDialog() {
        setOpen(false)
    }

    function centerAt(region: Region) {
        mapContext.map.centerAt(region.coords)
        game.selectRegion(region)
    }

    return <>
        <List>
            { Array.from(groups.keys()).map((region, i) => <React.Fragment key={i}>
                <ListSubheader sx={{ py: 2 }}>
                    <Stack direction='row' spacing={2} alignItems='center'>
                        <FixedTypography variant='subtitle1' title={region.toString()} sx={{ flex: 1, minWidth: 0 }}>{region.toString()}</FixedTypography>
                        <IconButton onClick={() => centerAt(region)}>
                            <LocationSearchingIcon />
                        </IconButton>
                    </Stack>
                </ListSubheader>
                { groups.get(region).map((b, i) => <BattleItem key={i} battle={b} onClick={openDialog} />) }
            </React.Fragment>) }
        </List>
        <Dialog maxWidth='md' fullWidth scroll='paper' open={open} onClose={closeDialog}>
            <DialogTitle sx={{ position: 'relative' }}>
                <span>Battle</span>
                <Box sx={{
                    position: 'absolute',
                    top: 0,
                    right: 0,
                    bottom: 0,
                    display: 'flex',
                    justifyContent: 'center',
                    alignItems: 'center',
                    mr: 3
                }}>
                    <IconButton>
                        <CloseIcon />
                    </IconButton>
                </Box>
            </DialogTitle>
            <DialogContent dividers>
                {battles[0].rounds.map((x, i) => <Box component='pre' key={i}>{x.log}</Box>)}
            </DialogContent>
            <DialogContent dividers>
                <Box><Typography variant='h1'>Heading 1</Typography></Box>
                <Box><Typography variant='h2'>Heading 2</Typography></Box>
                <Box><Typography variant='h3'>Heading 3</Typography></Box>
                <Box><Typography variant='h4'>Heading 4</Typography></Box>
                <Box><Typography variant='h5'>Heading 5</Typography></Box>
                <Box><Typography variant='h6'>Heading 6</Typography></Box>
                <Box><Typography variant='subtitle1'>subtitle1</Typography></Box>
                <Box><Typography variant='subtitle2'>subtitle2</Typography></Box>
                <Box><Typography variant='body1'>body1</Typography></Box>
                <Box><Typography variant='body2'>body2</Typography></Box>
                <Box><Typography variant='caption'>caption</Typography></Box>
                <Box><Typography variant='button'>button</Typography></Box>
                <Box><Typography variant='overline'>overline</Typography></Box>
            </DialogContent>
            <DialogActions>
                <Button autoFocus onClick={closeDialog}>Close</Button>
            </DialogActions>
        </Dialog>
    </>
}

