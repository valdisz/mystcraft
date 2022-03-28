import * as React from 'react'
import { Box, BoxProps, Stack, StackProps, Typography, List, ListSubheader, ListItem, ListItemButton, Chip, Dialog,
    DialogTitle, DialogContent, DialogActions, Button, IconButton, Divider, Tabs, Tab, Grid
} from '@mui/material'
import { Battle, BattleUnit, Faction as GameFaction, Region, Casualties } from '../game'
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
    items?: boolean
    skills?: boolean
}

function Unit({ items, skills, unit: { number, name, items: inventory, skills: unitSkills } }: UnitProps) {
    return !items
        ? <AnEntity size='normal' num={number} name={name} />
        : <Stack sx={{ flex: 1, minWidth: 0 }}>
            <AnEntity size='normal' num={number} name={name} />
            {items && <Box sx={{ flex: 1, maxWidth: 0, pl: 4 }}>
                <Stack direction='row' spacing={2}>
                    <Typography>Men</Typography>
                    138 (111 orcs, 15 goblins)
                </Stack>
                <Stack direction='row' spacing={2}>
                    <Typography>Mounts</Typography>
                    128 (138 horses)
                </Stack>
                <Stack direction='row' spacing={2}>
                    <Typography>Weapons</Typography>
                    128 swords
                </Stack>
                <Stack direction='row' spacing={2}>
                    <Typography>Armour</Typography>
                    128 plate armors
                </Stack>
                <Stack direction='row' spacing={2}>
                    <Typography>Other</Typography>
                    none
                </Stack>
                {/* { items && <Stack direction='row' spacing={2} flexWrap='wrap'>
                    { inventory.toArray().map(x => <Box key={x.code}>{x.amount} {x.name} [{x.code}]</Box>) }
                </Stack> }
                { (skills && unitSkills.size > 0) && <Stack direction='row' spacing={2} flexWrap='wrap'>
                    { unitSkills.toArray().map(x => <Box key={x.code}>{x.name} [{x.code}] {x.level} ({x.days})</Box>) }
                </Stack> } */}
            </Box> }
        </Stack>
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
                gap: 2,
                minWidth: 0
            }}>
                <Army side='Attacker' leader={attacker} victory={true} troops={attackingTroops} lost={attackerCasualties.lost} />
                <Divider orientation='vertical' flexItem />
                <Army side='Defender' leader={defender} victory={false} troops={defendungTroops} lost={defenderCasualties.lost} />
            </Box>
    </ListItemButton>
}

interface ArmyCasulatiesProps {
    casulaties: Casualties
}

function ArmyCasulaties({ casulaties: { lost, damagedUnits } }: ArmyCasulatiesProps) {
    return lost
        ? <>
            <Box>
                <Typography>Lost</Typography>
                <Typography>{lost}</Typography>
            </Box>
            <Typography>Damaged units</Typography>
            <List>
                { damagedUnits.map(x =><ListItem key={x.number}>
                    <Stack direction='row' spacing={2} alignItems='center'>
                        <Unit unit={x} />
                    </Stack>
                </ListItem>) }
            </List>
        </>
        : <Typography>None</Typography>
}

function groupByFaction(units: Iterable<BattleUnit>) {
    const items = new Map<GameFaction, BattleUnit[]>()
    for (const unit of units) {
        if (!items.has(unit.faction)) {
            items.set(unit.faction, [ ])
        }

        items.get(unit.faction).push(unit)
    }

    return items
}

interface ParticipantsProps {
    items: Map<GameFaction, BattleUnit[]>
}

function Participants({ items }: ParticipantsProps) {
    return <List>
        { Array.from(items.keys()).map(f => <React.Fragment key={f.num}>
            <ListSubheader disableGutters sx={{ lineHeight: 1 }}>
                <Faction faction={f} />
            </ListSubheader>
            { items.get(f).map(u => <ListItem key={u.number}>
                <Unit unit={u} items skills />
            </ListItem>) }
        </React.Fragment>) }
    </List>
}

interface BattleViewProps {
    battle: Battle
    open: boolean
    onClose: () => void

}

function BattleView({ battle, open, onClose }: BattleViewProps) {
    const attackers = React.useMemo(() => battle ? groupByFaction(battle.attackers) : null, [ battle ])
    const defenders = React.useMemo(() => battle ? groupByFaction(battle.defenders) : null, [ battle ])

    if (!battle) return null

    return <Dialog maxWidth='md' fullWidth scroll='paper' open={open} onClose={onClose}>
        <Box sx={{ py: 3, px: 6, position: 'relative' }}>
            <Stack direction='row' spacing={2} alignItems='center'>
                <Unit unit={battle.attacker} />
                <Typography>attacks</Typography>
                <Unit unit={battle.defender} />
                <Typography>in {battle.region.toString()}</Typography>
            </Stack>
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
                <IconButton onClick={onClose}>
                    <CloseIcon />
                </IconButton>
            </Box>
        </Box>
        <DialogContent dividers>
            <Grid container>
                <Grid item xs={6}>
                    <Typography variant='h6'>Attackers</Typography>
                    <Participants items={attackers} />
                </Grid>

                <Grid item xs={6}>
                    <Typography variant='h6'>Defenders</Typography>
                    <Participants items={defenders} />
                </Grid>
            </Grid>


            {battle.rounds.map((x, i) => <React.Fragment key={i}>
                <Typography variant='h6'>Round {i + 1}</Typography>
                <Box component='pre' key={i}>{x.log}</Box>
            </React.Fragment>)}

            <Grid container>
                <Grid item xs={6}>
                    <Typography variant='h6'>Attacker Casulaties</Typography>
                    <ArmyCasulaties casulaties={battle.attackerCasualties} />
                </Grid>

                <Grid item xs={6}>
                    <Typography variant='h6'>Defender Casulaties</Typography>
                    <ArmyCasulaties casulaties={battle.defenderCasualties} />
                </Grid>
            </Grid>

            <Typography variant='h6'>Spoils</Typography>
            <List>
                { battle.spoils.toArray().map(x =><ListItem key={x.code}>
                    {x.amount} {x.name} [{x.code}]
                </ListItem>) }
            </List>

        </DialogContent>
        <DialogActions>
            <Button autoFocus onClick={onClose}>Close</Button>
        </DialogActions>
    </Dialog>
}

export interface BattleListProps {
    battles: Battle[]
}

export function BattleList({ battles }: BattleListProps) {
    const mapContext = useMapContext()
    const { game } = useStore()
    const [ open, setOpen ] = React.useState<Battle>(null)

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


    function openDialog(battle: Battle) {
        setOpen(battle)
    }

    function closeDialog() {
        setOpen(null)
    }

    const isOpen = open !== null

    function centerAt(region: Region) {
        mapContext.map.centerAt(region.coords)
        game.selectRegion(region)
    }

    return <>
        <List disablePadding>
            { Array.from(groups.keys()).map((region, i) => <React.Fragment key={i}>
                <ListSubheader sx={{
                    pt: 4, pb: 1,
                    borderBottom: 1,
                    borderColor: 'divider',
                    minWidth: 0,
                    '&:first-of-type': {
                        pt: 0
                    }
                }}>
                    <Stack direction='row' spacing={2} alignItems='center' sx={{ minWidth: 0 }}>
                        <FixedTypography variant='subtitle1' title={region.toString()} sx={{ flex: 1, minWidth: 0 }}>{region.toString()}</FixedTypography>
                        <IconButton onClick={() => centerAt(region)}>
                            <LocationSearchingIcon />
                        </IconButton>
                    </Stack>
                </ListSubheader>
                { groups.get(region).map((b, i) => <BattleItem key={i} battle={b} onClick={() => openDialog(b)} />) }
            </React.Fragment>) }
        </List>
        <BattleView battle={open} open={isOpen} onClose={closeDialog} />
    </>
}

