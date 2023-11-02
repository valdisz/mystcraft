import * as React from 'react'
import { styled } from '@mui/material/styles'
import { Link, useLocation, useResolvedPath, useParams, Outlet } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { AppBar, Typography, Toolbar, IconButton, Table, TableHead, TableRow, TableCell, TableBody, Button,
    ButtonGroup, Chip, Avatar, Box, Tooltip, Card, CardContent, Stack, CircularProgress, Container, Grid,
    BoxProps, Badge, Dialog, DialogContent, DialogContentText, List, ListItem, ListItemButton,
    ButtonProps
} from '@mui/material'
import { useStore, GameLoadingStore, GameStore } from '../store'
import { observer } from 'mobx-react'
import {MapProvider, Paths, useMapContext} from '../map'
import { Region, ItemMap, Item, Unit, ICoords, Capacity, MoveType, Structure } from '../game'
import { UnitSummary, Orders, FloatingPanel, RegionSummary, RegionHeader, BattleList, FluidFab } from '../components'
import { green, lightBlue } from '@mui/material/colors'
import { useTheme } from '@mui/system'
// import SimpleBar from 'simplebar-react'
import { sumBy } from 'lodash'

import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddIcon from '@mui/icons-material/Add'
import RemoveIcon from '@mui/icons-material/Remove'
import DoneIcon from '@mui/icons-material/Done'
import CloseIcon from '@mui/icons-material/Close'

export interface GameMapProps {
    selectedRegion: ICoords | null
    onRegionSelected: (reg: Region) => void
    paths: Paths
}

export function GameMapComponent({ selectedRegion, onRegionSelected, paths }: GameMapProps) {
    const { game } = useStore()
    const context = useMapContext()

    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()

    React.useEffect(() => {
        if (!canvasRef) return

        const level = game.world.getLevel(1)

        const finalizer = context.initialize(canvasRef, level, onRegionSelected);

        const coords = context.findCoordsToCenterAt(game.world.factions.player.troops, game.region)
        if (coords) {
            context.map.centerAt(coords)
        }

        context.map.render()

        return () => {
            setCanvasRef(null)
            finalizer()
        }
    }, [ canvasRef ])

    React.useEffect(() => {
        if (!context.map) {
            return
        }

        context.map.select(selectedRegion)
    }, [ selectedRegion ])

    React.useEffect(() => {
        if (!context.map) {
            return
        }

        context.map.setPaths(paths);
    }, [ paths ])

    return <Box sx={{ bgcolor: 'black', width: '100%', height: '100%' }}>
        <Box component={'canvas'} sx={{ width: '100%', height: '100%' }} ref={setCanvasRef} />
    </Box>
}

const UnitsTable = styled(Table)`
    min-height: 0;
    width: 100%;

    td, th {
        white-space: nowrap;
        font-family: Fira Code, monospace;
        font-size: 80%;
    }

    th {
        font-weight: bold;
    }

    .faction-nr, .structure-nr, .unit-nr, .men-count, .mounts-count, .short-items-count {
        width: 1px;
        text-align: right;
        padding-right: 4px;
    }

    .faction-name, .structure-name, .unit-name, .men, .mounts, .short-items {
        width: 1px;
        padding-left: 4px;
    }

    .money, .movement, .weight, .capacity {
        width: 1px;
    }

    .weight {
        text-align: right;
    }

    .skills {
        white-space: normal;
        width: 50%;
    }

    .items {
        white-space: normal;
        width: 50%;
    }

    .description {
        white-space: normal;
        font-size: 70%;
        font-style: italic;
        padding-left: 4px;
    }

    .flags {

    }

    .no-border {
        .unit-nr, .unit-name, .men, .money, .mounts, .items, .skills, .movement, .weight, .capacity, .flags {
            border-bottom-width: 0;
        }
    }

    .Mui-selected, .Mui-selected:hover {
        background-color: ${lightBlue[300]};
    }

    .own {
        background-color: ${green[100]};
    }

    .own.Mui-selected {
        background-color: ${green[300]};
    }
`

function unitMenCount(items: ItemMap<Item>) {
    const men = items.filter(x => x.isManLike)

    const total = men.map(x => x.amount).reduce((value, next) => value + next, 0)

    return total
}


function unitMen(items: ItemMap<Item>) {
    const men = items.filter(x => x.isManLike)

    men.sort((a, b) => b.amount - a.amount)
    const names = men.map(x => x.name).join(', ')

    return names
}

function unitMountsCount(items: ItemMap<Item>) {
    const mounts = items.filter(x => x.isMount)
    if (mounts.length === 0) {
        return null
    }

    const total = mounts.map(x => x.amount).reduce((value, next) => value + next, 0)

    return total
}

function unitMounts(items: ItemMap<Item>) {
    const mounts = items.filter(x => x.isMount)
    if (mounts.length === 0) {
        return null
    }

    mounts.sort((a, b) => b.amount - a.amount)
    const names = mounts.map(x => x.name).join(', ')

    return names
}

function unitWeaponsCount(items: ItemMap<Item>) {
    const weapons = items.filter(x => x.info.category === 'weapon')
    if (weapons.length === 0) {
        return null
    }

    const total = weapons.map(x => x.amount).reduce((value, next) => value + next, 0)

    return total
}

function unitWeapons(items: ItemMap<Item>) {
    const weapons = items.filter(x => x.info.category === 'weapon')
    if (weapons.length === 0) {
        return null
    }

    weapons.sort((a, b) => b.amount - a.amount)
    const names = weapons.map(x => x.name).join(', ')

    return names
}

interface MoveIconProps {
    moveType: MoveType
}

const MoveAvatar = styled(Avatar)(() => ({
    width: '1.5rem',
    height: '1.5rem',
    fontSize: '1rem',
    background: 'transparent'
}))

function MoveIcon({ moveType }: MoveIconProps) {
    switch (moveType) {
        case 'walk': return <MoveAvatar>🚶</MoveAvatar>
        case 'swim': return <MoveAvatar>🏊🏽</MoveAvatar>
        case 'ride': return <MoveAvatar>🐎</MoveAvatar>
        case 'fly': return <MoveAvatar>💨</MoveAvatar>
    }
}

function renderUnitMovement(unit: Unit) {
    if (!unit) return null

    if (unit.isOverweight) {
        return <Chip size='small' color='error' label='Weight!' title='Overweight' />
    }

    let move: React.ReactNode = null
    const icon = <MoveIcon moveType={unit.moveType} />
    switch (unit.moveType) {
        case 'walk':
            move = icon
            break

        case 'ride':
        case 'fly':
            const evasion = unit.evasion
            move = evasion
                ? <Chip size='small' avatar={icon} title={unit.moveType} label={evasion} />
                : icon
            break
    }

    return <Box sx={{ display: 'flex', gap: 1 }}>
        {move}
        { unit.canSwim && <MoveIcon moveType='swim' /> }
    </Box>
}

const UnitCapacityContainer = styled('div')`
    display: flex;
    gap: 0.5rem;
`

const UnitCapacityItem = styled('span')`
    white-space: pre;
`

interface UnitCapacityProps {
    weight: number
    capacity: Capacity
}

function UnitCapacity({ weight, capacity }: UnitCapacityProps) {
    return <UnitCapacityContainer>
        { capacity.fly > 0 && <Chip size='small' avatar={<Avatar>F</Avatar>} label={<UnitCapacityItem>{capacity.fly - weight}</UnitCapacityItem>} /> }
        { capacity.ride > 0 && <Chip size='small' avatar={<Avatar>R</Avatar>} label={<UnitCapacityItem>{capacity.ride - weight}</UnitCapacityItem>} /> }
        { capacity.walk > 0 && <Chip size='small' avatar={<Avatar>W</Avatar>} label={<UnitCapacityItem>{capacity.walk - weight}</UnitCapacityItem>} /> }
        { capacity.swim > 0 && <Chip size='small' avatar={<Avatar>S</Avatar>} label={<UnitCapacityItem>{capacity.swim - weight}</UnitCapacityItem>} /> }
    </UnitCapacityContainer>
}

const UnitRow = observer(({ unit, game }: { unit: Unit, game: GameStore }) => {
    const rows = unit.description ? 2 : 1
    const noBorder = rows > 1 ? 'no-border' : ''

    return <React.Fragment>
        <TableRow className={noBorder} onClick={() => game.selectUnit(unit)} selected={unit.num === game.unit?.num}>
            <TableCell rowSpan={rows}>
                { game.isAttacker(unit)
                    ? <Chip label='Attacker' color='primary' onClick={() => game.removeFromBattleSim(unit)} />
                    : game.isDefender(unit)
                        ? <Chip label='Defender' color='secondary' onClick={() => game.removeFromBattleSim(unit)} />
                        : <ButtonGroup>
                            <Button variant='outlined' onClick={() => game.addAttacker(unit)}>Attacker</Button>
                            <Button variant='outlined' onClick={() => game.addDefender(unit)}>Defender</Button>
                        </ButtonGroup>
                }
            </TableCell>
            <TableCell rowSpan={rows} className='structure-nr'>{unit.structure?.num ?? null}</TableCell>
            <TableCell rowSpan={rows} className='structure-name'>{unit.structure?.name ?? null}</TableCell>
            <TableCell rowSpan={rows} className='faction'>{unit.faction.known ? `${unit.faction.name} (${unit.faction.num})` : ''}</TableCell>
            <TableCell className='unit-nr'>{unit.num}</TableCell>
            <TableCell component='th' className={`unit-name ${noBorder}`}>{unit.name}</TableCell>
            <TableCell className='men-count'>
                { unitMenCount(unit.inventory.items)}
            </TableCell>
            <TableCell className='men'>
                { unitMen(unit.inventory.items) }
            </TableCell>
            <TableCell className='mounts-count'>
                { unitMountsCount(unit.inventory.items) }
            </TableCell>
            <TableCell className='mounts'>
                { unitMounts(unit.inventory.items) }
            </TableCell>
            <TableCell className='items'>{unit.inventory.items.filter(x => !x.isManLike && !x.isMoney && !x.isMount).map(x => `${x.amount} ${x.name}`).join(', ')}</TableCell>
            <TableCell className='skills'>{unit.skills.map(x => `${x.name} ${x.level} (${x.days})`).join(', ')}</TableCell>
        </TableRow>
        { rows > 1 && <TableRow>
            <TableCell colSpan={3}></TableCell>
            <TableCell colSpan={5} className='description'>
                {unit.description}
            </TableCell>
        </TableRow> }
    </React.Fragment>
})

const BattleSim = observer(() => {
    const { game } = useStore()
    return <Dialog fullScreen  open={game.battleSimOpen} onClose={game.closeBattleSim}>
        <AppBar sx={{ position: 'relative' }}>
            <Toolbar>
                <IconButton edge="start" color="inherit" onClick={game.closeBattleSim} size="large"><CloseIcon /></IconButton>
                <Typography variant="h6" sx={{
                    marginLeft: 2,
                    flex: 1
                }}>Battle Sim</Typography>
                <Button onClick={game.resetBattleSim} color="inherit">Reset</Button>
                <Button color="inherit" onClick={game.toBattleSim}>Get BattleSim JSON</Button>
            </Toolbar>
        </AppBar>
        <DialogContent>
            <DialogContentText>Select battle sim sides</DialogContentText>
            <UnitsTable size='small' stickyHeader>
                <TableHead>
                    <TableRow>
                        <TableCell></TableCell>
                        <TableCell className='structure-nr'></TableCell>
                        <TableCell className='structure-name'>Structure</TableCell>
                        <TableCell className='faction'>Faction</TableCell>
                        <TableCell className='unit-nr'></TableCell>
                        <TableCell className='unit-name'>Unit</TableCell>
                        <TableCell className='men-count'></TableCell>
                        <TableCell className='men'>Men</TableCell>
                        <TableCell className='mounts-count'></TableCell>
                        <TableCell className='mounts'>Mounts</TableCell>
                        <TableCell className='items'>Items</TableCell>
                        <TableCell className='skills'>Skills</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {game.units.map(unit => <UnitRow key={unit.num} unit={unit} game={game} />)}
                </TableBody>
            </UnitsTable>
        </DialogContent>
    </Dialog>
})

const UnitsComponent = observer(({ sx, ...props }: BoxProps) => {
    const { game } = useStore()

    return (
        <Box {...props} sx={{ display: 'flex', flexDirection: 'column', ...(sx || { }) }}>
            <Box sx={{ flex: 1, minHeight: 0, overflow: 'auto' }}>
            {/* <Box component={SimpleBar} sx={{ height: '100%' }} autoHide={false}> */}
            <Box sx={{ height: '100%' }}>
                <UnitsTable size='small' stickyHeader >
                    <TableHead>
                        <TableRow>
                            <TableCell className='structure-nr'></TableCell>
                            <TableCell className='structure-name'>Structure</TableCell>
                            <TableCell className='faction-nr'></TableCell>
                            <TableCell className='faction-name'>Faction</TableCell>
                            <TableCell className='unit-nr'></TableCell>
                            <TableCell className='unit-name'>Unit</TableCell>
                            <TableCell className='money'>Money</TableCell>
                            <TableCell className='men-count'></TableCell>
                            <TableCell className='men'>Men</TableCell>
                            <TableCell className='mounts-count'></TableCell>
                            <TableCell className='short-items'>Weapons</TableCell>
                            <TableCell className='short-items-count'></TableCell>
                            <TableCell className='mounts'>Mounts</TableCell>
                            <TableCell className='movement'>Movement</TableCell>
                            <TableCell className='weight'>Weight</TableCell>
                            <TableCell className='capacity'>Capacity</TableCell>
                            <TableCell className='flags'>Flags</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                    {game.units.map((unit) => {
                        const rows = unit.description ? 2 : 1
                        const noBorder = rows > 1 ? 'no-border' : ''
                        // const ownUnit = unit.isPlayer ? 'own' : ''
                        const ownUnit = ''

                        const unitClasses = [ownUnit, noBorder].join(' ')

                        return <React.Fragment key={unit.id}>
                            <TableRow className={unitClasses} onClick={() => game.selectUnit(unit)} selected={unit.num === game.unit?.num}>
                                <TableCell className='structure-nr'>{unit.structure?.num ?? null}</TableCell>
                                <TableCell className='structure-name'>{unit.structure?.name ?? null}</TableCell>
                                <TableCell className='faction-nr'>{unit.faction.known ? unit.faction.num : null}</TableCell>
                                <TableCell component='th' className='faction-name'>{unit.faction.known ? unit.faction.name : null}</TableCell>
                                <TableCell className='unit-nr'>{unit.num}</TableCell>
                                <TableCell component='th' className={`unit-name ${noBorder}`}>
                                    {unit.name}
                                    { ' ' }
                                    { unit.onGuard && <Chip size='small' color='primary' label='G' /> }
                                </TableCell>
                                <TableCell className='money'>{unit.money ? unit.money : null}</TableCell>
                                <TableCell className='men-count'>
                                    { unitMenCount(unit.inventory.items)}
                                </TableCell>
                                <TableCell className='men'>
                                    { unitMen(unit.inventory.items) }
                                </TableCell>
                                <TableCell className='short-items-count'>
                                    { unitWeaponsCount(unit.inventory.items) }
                                </TableCell>
                                <TableCell className='short-items'>
                                    { unitWeapons(unit.inventory.items) }
                                </TableCell>
                                <TableCell className='mounts-count'>
                                    { unitMountsCount(unit.inventory.items) }
                                </TableCell>
                                <TableCell className='mounts'>
                                    { unitMounts(unit.inventory.items) }
                                </TableCell>
                                <TableCell className='movement'>{renderUnitMovement(unit)}</TableCell>
                                <TableCell className='weight'>{unit.weight}</TableCell>
                                <TableCell className='capacity'>
                                    <UnitCapacity weight={unit.weight} capacity={unit.capacity} />
                                </TableCell>
                                <TableCell className='flags'>{unit.flags.join(', ')}</TableCell>
                            </TableRow>
                            { rows > 1 && <TableRow className={ownUnit} selected={unit.num === game.unit?.num}>
                                <TableCell colSpan={5} className={noBorder} />
                                <TableCell colSpan={9} className='description'>
                                    {unit.description}
                                </TableCell>
                            </TableRow> }
                        </React.Fragment>
                    })}
                    </TableBody>
                </UnitsTable>
            </Box>
        </Box>
    </Box>
    );
})

const StructuresContainer = styled(Box)`
    grid-area: structures;

    display: flex;
    flex-direction: column;
    min-height: 0;
    overflow: auto;

    @media (max-width: 640px) {
        && {
            display: none;
        }
    }
`

interface StuctureItemProps extends BoxProps {
    item: Structure
}

function StuctureItem({ item, sx, ...props }: StuctureItemProps) {
    // const game = useStore()
    const theme = useTheme()

    const color = item.isFinished
        ? theme.palette.text.primary
        : theme.palette.warning.main

    const inside = sumBy(item.units, x => x.inventory.menCount)
    // const { world } = game.game

    return <Box columnGap={2} sx={{
        display: 'grid',
        gridTemplateColumns: 'minmax(2ch, min-content) minmax(0, 1fr) minmax(5ch, min-content) minmax(5ch, min-content)',
        gridAutoRows: 'min-content',
        justifyItems: 'center',
        alignItems: 'center',
        ...(sx || {})
    }} {...props}>
        <Typography variant='h6' sx={{ gridRow: 2, gridColumn: 1, justifySelf: 'end'  }}>{item.num}</Typography>
        <Typography variant='caption' sx={{ gridRow: 1, gridColumn: 2, justifySelf: 'start', alignSelf: 'center' }}>{item.type}</Typography>

        <Typography sx={{ gridRow: 2, gridColumn: 2, justifySelf: 'start' }} fontWeight={600} color={color}>{item.name}</Typography>

        { inside > 0 && <>
            <Typography variant='caption' sx={{ gridRow: 1, gridColumn: 3 }}>Inside</Typography>
            <Typography sx={{ gridRow: 2, gridColumn: 3 }}>{inside}</Typography>
        </> }

        { !item.isFinished && <>
            <Typography variant='caption' sx={{ gridRow: 1, gridColumn: 4 }}>Needs</Typography>
            <Chip sx={{ gridRow: 2, gridColumn: 4 }} size='small' label={item.needs.toString()} color='warning' />
        </> }

        { item.description && <Typography sx={{ gridRow: 3, gridColumn: 2, justifySelf: 'start' }} color='GrayText' fontStyle='italic' variant='body2'>{item.description}</Typography> }
    </Box>
}

const StructuresComponent = observer(() => {
    const { game } = useStore()
    const theme = useTheme()
    return <StructuresContainer>
        <List>
            {game.structures.map((row) => <ListItem key={row.id} disablePadding>
                <ListItemButton>
                    <StuctureItem flex={1} item={row} />
                </ListItemButton>
            </ListItem>)}
        </List>
    </StructuresContainer>
})

const RegionContainer = styled('div')`
    grid-area: details;

    display: flex;
    flex-direction: column;
    min-height: 0;
    overflow: auto;
`
const RegionComponent = observer(() => {
    const { game: { region, unit } } = useStore()

    return <RegionContainer>
        <RegionSummary region={region} />
    </RegionContainer>
})

function noop(e: any) {
    e.preventDefault()
}

export const MapTab = observer(() => {
    const { game } = useStore()

    const { battles } = game.world
    const context = useMapContext()

    return <GameLayout>
        <Box sx={{
            flex: 1,
            minHeight: 0,
            position: 'relative'
        }}>
            <GameMapComponent
                selectedRegion={game.region?.coords}
                onRegionSelected={game.selectRegion}
                paths={game.paths}
            />

            <Box sx={{
                position: 'absolute',
                top: 0,
                right: 0,
                bottom: 0,
                left: 0,
                pointerEvents: 'none',
                display: 'grid',
                gap: 2,
                m: 2,
                gridTemplateColumns: 'minmax(min-content, 50vw) 1fr minmax(min-content, 400px)',
                gridTemplateRows: 'minmax(0, 33vh) 1fr minmax(min-content, 0)'
            }}>

                {/* Left Panel */}
                <Box sx={{
                    gridColumnStart: 1, gridColumnEnd: 2,
                    gridRowStart: 1, gridRowEnd: 3,
                    overflow: 'hidden',
                    position: 'relative',
                    minHeight: 0
                }}>
                    {/* Game Screens */}
                    <Box sx={{ position: 'absolute', display: 'flex', top: 0, left: 0, right: 0, bottom: 0, pointerEvents: 'none' }}>
                        <Box sx={{ m: 2, flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
                            <ButtonGroup sx={{ pointerEvents: 'all' }} orientation='vertical' size='small' color='inherit'>
                                <Button variant='contained' onClick={() => context.map.zoomIn()}>
                                    <AddIcon />
                                </Button>
                                <Button variant='contained' onClick={() => context.map.zoomOut()}>
                                    <RemoveIcon />
                                </Button>
                            </ButtonGroup>
                        </Box>
                    </Box>
                    { !game.battlesVisible && <Box sx={{
                        position: 'absolute',
                        top: 0,
                        left: 70,
                        display: 'flex',
                        flexDirection: 'column',
                        justifyContent: 'flex-start',
                        alignItems: 'center',
                        gap: 2,
                        mt: 4,
                        pointerEvents: 'all'
                    }}>
                        { battles.length > 0 && <Badge badgeContent={battles.length} color='error'>
                            <FluidFab icon={<Box component='span' sx={{ fontSize: '24px' }}>⚔</Box>} sx={{ zIndex: 0 }} onClick={game.toggleBattles}>
                                Battles
                            </FluidFab>
                        </Badge> }
                    </Box> }

                    { game.battlesVisible && <FloatingPanel header='Battles' onClose={game.hideBattles} sx={{
                        maxWidth: '400px',
                        height: '100%',
                        minHeight: 0,
                        pointerEvents: 'all',
                        display: 'flex',
                        flexDirection: 'column'
                    }}>
                        <Box sx={{ flex: 1, minHeight: 0 }}>
                            {/* <Box component={SimpleBar} autoHide={false} sx={{ height: '100%' }}> */}
                            <Box sx={{ height: '100%' }}>
                                <BattleList battles={battles} />
                            </Box>
                        </Box>
                    </FloatingPanel> }

                    {/* <Box component={SimpleBar} autoHide={false} sx={{
                        pointerEvents: 'all',
                        minHeight: 0,
                        height: '100%',
                        zIndex: 'drawer'
                    }} onMouseDown={noop} onMouseUp={noop} onClick={noop}> */}
                    {/* </Box> */}
                </Box>

                {/* Right Panel */}
                <Box sx={{
                    gridColumnStart: 3, gridColumnEnd: 4,
                    gridRowStart: 1, gridRowEnd: 3,
                    overflow: 'hidden'
                }}>
                    {/* <Box component={SimpleBar} autoHide={false} sx={{ */}
                    <Box sx={{
                        pointerEvents: 'all',
                        minHeight: 0,
                        maxHeight: '100%'
                    }} onMouseDown={noop} onMouseUp={noop} onClick={noop}>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                            { game.region && <FloatingPanel header={<RegionHeader region={game.region} sx={{ flex: 1, minWidth: 0 }} />} expanded={game.regionPanel && game.region.explored} onExpand={game.exapandRegion}>
                                <RegionComponent />
                            </FloatingPanel> }
                            { game.structures?.length > 0 && <FloatingPanel header='Structures' expanded={game.structuresPanel} onExpand={game.exapandStructures}>
                                <StructuresComponent />
                            </FloatingPanel> }
                        </Box>
                    </Box>
                </Box>

                {/* Bottom Panel */}
                { game.region && <FloatingPanel header={<Stack direction='row' spacing={2}>
                        <Typography variant='h6'>Units</Typography>
                        <Button variant='outlined' onClick={game.openBattleSim}>Battle Sim</Button>
                    </Stack>}
                sx={{
                    gridColumnStart: 1, gridColumnEnd: 4,
                    gridRow: 3,
                    alignSelf: 'flex-end',
                    minHeight: 0,
                    pointerEvents: 'all'
                }} expanded={game.unitsPanel} onExpand={game.exapandUnits}>
                    <Box sx={{ height: '30vh', display: 'flex' }}>
                        <Box sx={{ flex: 1, minHeight: 0, minWidth: 0 }}>
                            <UnitsComponent sx={{ width: '100%', height: '100%' }} />
                        </Box>
                        { game.unit && <UnitSummary unit={game.unit} sx={{ width: '25vw', maxWidth: '400px' }} /> }
                        { game.isOrdersVisible && <Orders readOnly={game.isOrdersReadonly} sx={{ width: '25vw', maxWidth: '400px' }} /> }
                    </Box>
                    <BattleSim />
                </FloatingPanel> }
            </Box>
        </Box>
    </GameLayout>
})

const MONTH_NAME = {
    1: 'January',
    2: 'February',
    3: 'March',
    4: 'April',
    5: 'May',
    6: 'June',
    7: 'July',
    8: 'August',
    9: 'September',
    10: 'October',
    11: 'November',
    12: 'December',
}

export interface ProgressItemProps {
    text: string
    progress: number
}

export function ProgressItem({ text, progress }: ProgressItemProps) {
    return <Box sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between'
    }}>
        <Typography>{text}</Typography>
        { progress === 100
            ? <DoneIcon sx={{ fontSize: '1rem' }} />
            : <CircularProgress
                size='1rem'
                variant={ progress === 0 ? 'indeterminate' : 'determinate' }
                value={progress}
            /> }
    </Box>
}

export interface LoadingProps {
    loading: GameLoadingStore
}

export const Loading = observer(({ loading }: LoadingProps) => {
    return <Container sx={{ height: '100%' }}>
        <Grid sx={{ height: '100%' }} container justifyContent='center' alignItems='center'>
            <Grid item xs={12} md={6} lg={3}>
                <Card variant='outlined'>
                    <CardContent>
                        <Typography variant='h5'>Loading</Typography>
                        <Stack spacing={1}>
                            { loading.done.map((x, i) => <ProgressItem key={i} text={x} progress={100} />) }
                            { loading.isLoading && <ProgressItem text={loading.phase} progress={loading.progress} /> }
                        </Stack>
                    </CardContent>
                </Card>
            </Grid>
        </Grid>
    </Container>
})

function GameLayout({ children }: React.PropsWithChildren) {
    const { game } = useStore()
    const { world } = game
    const { player } = world.factions

    return <>
            <AppBar position='static' variant='outlined' elevation={0}>
                <Toolbar>
                    <IconButton component={Link} to={`/games/${game.gameId}`} edge='start' color='inherit' size="large">
                        <ArrowBackIcon />
                    </IconButton>
                    <Typography variant='h6'>{ game.name }</Typography>

                    <Stack direction='row' ml={8} gap={2} alignItems='center'>
                        <Stack direction='row' gap={4}>
                            <Stack>
                                <Typography variant='caption'>Turn</Typography>
                                <Typography fontWeight='bold'>{ world.turnNumber }</Typography>
                            </Stack>
                            <Stack>
                                <Typography variant='caption'>Year</Typography>
                                <Typography fontWeight='bold'>{ world.year }</Typography>
                            </Stack>
                            <Stack>
                                <Typography variant='caption'>Month</Typography>
                                <Typography fontWeight='bold'>{ MONTH_NAME[world.month] }</Typography>
                            </Stack>
                            <Stack>
                                <Typography variant='caption'>Faction</Typography>
                                <Typography fontWeight='bold'>{ player.name }</Typography>
                            </Stack>
                            <Stack>
                                <Typography variant='caption'>Number</Typography>
                                <Typography fontWeight='bold'>{ player.num }</Typography>
                            </Stack>
                        </Stack>

                        <Button component={Link} to='stats' sx={{
                            color: 'inherit',
                            textTransform: 'inherit',
                            textAlign: 'left'
                        }} variant='outlined'>
                            <Stack direction='row' gap={4}>
                                <Stack>
                                    <Typography variant='caption'>Unclaimed Silver</Typography>
                                    <Typography fontWeight='bold'>{ game.unclaimed }</Typography>
                                </Stack>
                                <Stack>
                                    <Typography variant='caption'>Silver</Typography>
                                    <Typography fontWeight='bold'>{ game.money }</Typography>
                                </Stack>
                                <Stack>
                                    <Typography variant='caption'>Food</Typography>
                                    <Typography fontWeight='bold'>{ game.food }</Typography>
                                </Stack>
                                <Stack>
                                    <Typography variant='caption'>Men</Typography>
                                    <Typography fontWeight='bold'>{ game.men }</Typography>
                                </Stack>
                                <Stack>
                                    <Typography variant='caption'>Mounts</Typography>
                                    <Typography fontWeight='bold'>{ game.mounts }</Typography>
                                </Stack>
                                <Stack>
                                    <Typography variant='caption'>Weapons</Typography>
                                    <Typography fontWeight='bold'>{ game.weapons }</Typography>
                                </Stack>
                            </Stack>
                        </Button>

                        <Button variant='outlined' sx={{ color: 'inherit', alignSelf: 'stretch' }}>Attitudes</Button>
                        <Button variant='outlined' sx={{ color: 'inherit', alignSelf: 'stretch' }}>Mages</Button>
                        <Button variant='outlined' sx={{ color: 'inherit', alignSelf: 'stretch' }}>Alliance</Button>
                    </Stack>

                    <Box flex={1} />

                    {/* <Tabs value={location.pathname}>
                        { routes.filter(x => x.enabled).map(({ label, path }) => <Tab key={path} label={label} value={path} to={path} component={Link} />) }
                    </Tabs>

                    <Box flex={1} /> */}

                    <Button size='medium' color='inherit' onClick={game.getOrders}>Download Orders</Button>
                </Toolbar>
            </AppBar>
            {children}
        </>
}

function _GamePage() {
    const { loading } = useStore()

    if (loading.isLoading) {
        return <Loading loading={loading} />
    }

    return <Box sx={{
        width: '100%',
        height: '100%',
        display: 'flex',
        flexDirection: 'column'
    }}>
        <Outlet />
    </Box>
}

export const GamePage = observer(_GamePage)
