import * as React from 'react'
import { styled } from '@mui/material/styles'
import { Link, useParams, Outlet } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import {
    AppBar,
    Typography,
    Toolbar,
    IconButton,
    Table,
    TableHead,
    TableRow,
    TableCell,
    TableBody,
    Paper,
    Button,
    DialogContent,
    DialogContentText,
    ButtonGroup,
    Chip,
    Avatar,
    Box,
    Tooltip,
    Card,
    CardContent,
    Stack,
    CircularProgress,
    Container,
    Grid,
    CardHeader,
    Collapse,
    CardActions,
    CardProps,
    BoxProps
} from '@mui/material'
import { useStore, GameStore, GameLoadingStore } from '../store'
import { observer } from 'mobx-react'
import { HexMap2, Resources } from '../map'
import { Region, ItemMap, Item, Unit, Coords, ICoords, Capacity, World, Level, Faction, Troops } from '../game'
import { Dialog } from '@mui/material'
import { UnitSummary, Orders, ExpandMore, RegionSummary, RegionHeader } from '../components'
import { green, lightBlue } from '@mui/material/colors'
import { InterfaceCommand } from '../store/commands/move'
import { Loader } from 'pixi.js'
import SimpleBar from 'simplebar-react'

import CloseIcon from '@mui/icons-material/Close'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddIcon from '@mui/icons-material/Add'
import RemoveIcon from '@mui/icons-material/Remove'
import DoneIcon from '@mui/icons-material/Done'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'

const GameContainer = styled(Box)`
    width: 100%;
    height: 100%;

    display: flex;
    flex-direction: column;
`

const GameInfo = styled('div')`
    margin-left: 1rem;
`

class MapContext {
    constructor() {
        this.resources = new Resources(new Loader())
    }

    readonly resources: Resources
    map: HexMap2

    load() {
        return this.resources.load()
    }

    initialize(canvas: HTMLCanvasElement, level: Level, onRegionSelected: (reg: Region) => void) {
        this.map = new HexMap2(canvas, this.resources, level.width, level.height, {
            onClick: onRegionSelected
        })

        const regions = level.toArray()
        this.map.setRegions(regions)

        return (() => this.map.destroy())
    }

    findCoordsToCenterAt(troops: Troops, region?: Region) {
        let coords: Coords = region?.coords

        if (!coords) {
            const lastLocation = window.localStorage.getItem('coords')
            if (lastLocation) {
                coords = JSON.parse(lastLocation)
                if (coords?.x == null || coords?.y == null) {
                    coords = null
                }
            }
        }

        if (!coords) {
            const unit = troops.first()
            if (unit) {
                coords = unit.region.coords
            }
        }

        return coords
    }
}

const mapContext = React.createContext<MapContext>(null)

interface MapProviderProps {
    children: React.ReactNode
}

function MapProvider({ children }: MapProviderProps) {
    const [ map ] = React.useState(() => new MapContext())

    return <mapContext.Provider value={map}>
        {children}
    </mapContext.Provider>
}

function useMapContext() {
    return React.useContext(mapContext)
}

interface GameMapProps {
    selectedRegion: ICoords | null
    onRegionSelected: (reg: Region) => void
}

function GameMapComponent({ selectedRegion, onRegionSelected }: GameMapProps) {
    const { game } = useStore()
    const context = useMapContext()

    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()

    React.useEffect(() => {
        if (!canvasRef) return

        const level = game.world.getLevel(1)
        const finalizer = context.initialize(canvasRef, level, onRegionSelected)

        const coords = context.findCoordsToCenterAt(game.world.factions.player.troops, game.region)
        if (coords) {
            context.map.centerAt(coords)
        }

        context.map.render()

        return finalizer
    }, [ canvasRef ])

    React.useEffect(() => {
        if (!context.map) {
            return
        }

        context.map.select(selectedRegion)
    }, [ selectedRegion ])

    return <Box sx={{ bgcolor: 'black', width: '100%', height: '100%' }}>
        <Box component={'canvas'} sx={{ width: '100%', height: '100%' }} ref={setCanvasRef} />
        { (false && !!context.map) && <Box sx={{ position: 'absolute', display: 'flex', top: 0, left: 0, right: 0, bottom: 0, pointerEvents: 'none' }}>
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
        </Box> }
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

    .faction-nr, .structure-nr, .unit-nr, .men-count, .mounts-count {
        width: 1px;
        text-align: right;
        padding-right: 4px;
    }

    .faction-name, .structure-name, .unit-name, .men, .mounts {
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
    const total = mounts.map(x => x.amount).reduce((value, next) => value + next, 0)
    const names = mounts.map(x => x.name).join(', ')

    return names
}

// function UnitRow({ unit, game }: { unit: Unit, game: GameStore }) {
//     const rows = unit.description ? 2 : 1
//     const noBorder = rows > 1 ? 'no-border' : ''

//     return <React.Fragment>
//         <TableRow className={noBorder} onClick={() => game.selectUnit(unit)} selected={unit.num === game.unit?.num}>
//             <TableCell rowSpan={rows}>
//                 { game.isAttacker(unit)
//                     ? <Chip label='Attacker' color='primary' />
//                     : game.isDefender(unit)
//                         ? <Chip label='Defender' color='secondary' />
//                         : <ButtonGroup>
//                             <Button variant='outlined' onClick={() => game.addAttacker(unit)}>Attacker</Button>
//                             <Button variant='outlined' onClick={() => game.addDefender(unit)}>Defender</Button>
//                         </ButtonGroup>
//                 }
//             </TableCell>
//             <TableCell rowSpan={rows} className='structure-nr'>{unit.structure?.num ?? null}</TableCell>
//             <TableCell rowSpan={rows} className='structure-name'>{unit.structure?.name ?? null}</TableCell>
//             <TableCell rowSpan={rows} className='faction'>{unit.faction.known ? `${unit.faction.name} (${unit.faction.num})` : ''}</TableCell>
//             <TableCell className='unit-nr'>{unit.num}</TableCell>
//             <TableCell component='th' className={`unit-name ${noBorder}`}>{unit.name}</TableCell>
//             <TableCell className='men'>
//                 <UnitMen items={unit.inventory.items} />
//             </TableCell>
//             <TableCell className='mounts'>
//                 <UnitMounts items={unit.inventory.items} />
//             </TableCell>
//             <TableCell className='items'>{unit.inventory.items.filter(x => !x.isManLike && !x.isMoney && !x.isMount).map(x => `${x.amount} ${x.name}`).join(', ')}</TableCell>
//             <TableCell className='skills'>{unit.skills.map(x => `${x.name} ${x.level} (${x.days})`).join(', ')}</TableCell>
//         </TableRow>
//         { rows > 1 && <TableRow>
//             <TableCell className='unit-nr'></TableCell>
//             <TableCell colSpan={5} className='description'>
//                 {unit.description}
//             </TableCell>
//         </TableRow> }
//     </React.Fragment>
// }

function renderUnitMovement(unit: Unit) {
    if (!unit) return null

    let moveType = ''
    switch (unit.moveType) {
        case 'walk':
            moveType = '🚶'
            break

        case 'ride':
            moveType = '🐎'
            break

        case 'fly':
            moveType = '💨'
            break
    }

    const move = unit.isOverweight
        ? <Chip size='small' color='error' label='Weight!' title='Overweight' />
        : <Typography sx={{ fontSize: '150%' }} title={unit.moveType}>{moveType}</Typography>
        // : <Chip size='small' label={unit.moveType} />

    const swim = unit.canSwim
        ? <Typography sx={{ fontSize: '150%' }} title='Can swim'>🏊🏽</Typography>
        : null

    return <Box sx={{ display: 'flex', gap: 1 }}>
        {move}
        {swim}
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

interface CommandButtonProps {
    command: InterfaceCommand
}

const CommandButton = observer(({ command: { title, tooltip, canExecute, error, execute } }: CommandButtonProps) => {
    const text = []
    if (tooltip) text.push(tooltip)
    if (error) text.push(`!: ${error}`)

    console.log('canExecute', canExecute)

    const btn = <Button disabled={!canExecute} onClick={execute} variant='contained'>{title}</Button>

    return text.length
        ? <Tooltip title={text.join('\n')}><span>{btn}</span></Tooltip>
        : btn
})

const UnitsComponent = observer(({ sx, ...props }: BoxProps) => {
    const { game } = useStore()

    return (
        <Box {...props} sx={{ display: 'flex', flexDirection: 'column', ...(sx || { }) }}>
            {/* <Box sx={{ p: 1 }}>
                <Button onClick={game.openBattleSim}>Battle Sim</Button>
                {game.commands.filter(x => x.visible).map(x => { console.log(x); return <CommandButton key={x.title} command={x} /> })}
            </Box> */}

            {/* <Dialog fullScreen  open={game.battleSimOpen} onClose={game.closeBattleSim}>
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
                                <TableCell className='unit-nr'>Unit Nr.</TableCell>
                                <TableCell className='unit-name'>Name</TableCell>
                                <TableCell className='men'>Men</TableCell>
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
            </Dialog> */}

            <Box sx={{ flex: 1, minHeight: 0, overflow: 'auto' }}>

            <Box component={SimpleBar} sx={{ height: '100%' }} autoHide={false}>
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
                        const ownUnit = unit.isPlayer ? 'own' : ''

                        const unitClasses = [ownUnit, noBorder].join(' ')

                        return <React.Fragment key={unit.id}>
                            <TableRow className={unitClasses} onClick={() => game.selectUnit(unit)} selected={unit.num === game.unit?.num}>
                                <TableCell rowSpan={rows} className='structure-nr'>{unit.structure?.num ?? null}</TableCell>
                                <TableCell rowSpan={rows} className='structure-name'>{unit.structure?.name ?? null}</TableCell>
                                <TableCell rowSpan={rows} className='faction-nr'>{unit.faction.known ? unit.faction.num : null}</TableCell>
                                <TableCell component='th' rowSpan={rows} className='faction-name'>{unit.faction.known ? unit.faction.name : null}</TableCell>
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

const StructuresBody = styled('div')`
    min-height: 0;
`

const StructureItem = styled('div')`
    margin: 0.5rem 1rem;
`

const StructuresComponent = observer(() => {
    const { game } = useStore()
    return <StructuresContainer>
        <StructuresBody>
            {game.structures.map((row) => (
                <StructureItem key={row.id}>
                    {row.name} [{row.num}]: {row.type}
                </StructureItem>
            ))}
        </StructuresBody>
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

interface MapPanelProps extends CardProps {
    header: React.ReactNode
    children?: React.ReactNode
}

function MapPanel({ header, children, sx, ...props }: MapPanelProps) {
    const [ expanded, setExpanded ] = React.useState(true)

    const onExpandClick = () => {
        setExpanded(!expanded)
    }

    return <Card {...props} sx={{ opacity: 0.92, ...(sx || { }) }}>
        <CardActions disableSpacing sx={{ gap: 1 }}>
            { typeof header === 'string' ? <Typography variant='h6'>{header}</Typography> : header }
            <ExpandMore expand={expanded} onClick={onExpandClick}>
                <ExpandMoreIcon />
            </ExpandMore>
        </CardActions>
        <Collapse in={expanded}>
            {children}
        </Collapse>
    </Card>
}

function noop(e: any) {
    e.preventDefault()
}

export const MapTab = observer(() => {
    const { game } = useStore()

    return <Box sx={{
        flex: 1,
        minHeight: 0,
        position: 'relative'
    }}>
        <GameMapComponent selectedRegion={game.region?.coords} onRegionSelected={game.selectRegion} />

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
            gridTemplateColumns: 'minmax(min-content, 400px) 1fr minmax(min-content, 400px)',
            gridTemplateRows: 'minmax(0, 33vh) 1fr minmax(min-content, 0)'
        }}>
            {/* Right Panel */}
            <Box sx={{
                gridColumnStart: 3, gridColumnEnd: 4,
                gridRowStart: 1, gridRowEnd: 3,
                height: '100%'
            }}>
                <Box component={SimpleBar} autoHide={false} sx={{
                    pointerEvents: 'all'
                }} onMouseDown={noop} onMouseUp={noop} onClick={noop}>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        { game.region && <MapPanel header={<RegionHeader region={game.region} sx={{ flex: 1, minWidth: 0 }} />}>
                            <RegionComponent />
                        </MapPanel> }
                        { game.structures?.length > 0 && <MapPanel header='Structures'><StructuresComponent /></MapPanel> }
                    </Box>
                </Box>
            </Box>

            {/* Bottom Panel */}
            <MapPanel header='Units' sx={{
                gridColumnStart: 1, gridColumnEnd: 4,
                gridRow: 3,
                alignSelf: 'flex-end',
                minHeight: 0,
                pointerEvents: 'all'
            }}>
                <Box sx={{ height: '30vh', display: 'flex' }}>
                    <Box sx={{ flex: 1, minHeight: 0, minWidth: 0 }}>
                        <UnitsComponent sx={{ width: '100%', height: '100%' }} />
                    </Box>
                    { game.unit && <UnitSummary unit={game.unit} sx={{ width: '25vw', maxWidth: '400px' }} /> }
                    { game.isOrdersVisible && <Orders readOnly={game.isOrdersReadonly} sx={{ width: '25vw', maxWidth: '400px' }} /> }
                </Box>
            </MapPanel>
        </Box>
    </Box>
})

const GameComponent = observer(() => {
    const { game } = useStore()
    const { world } = game
    const { player } = world.factions

    return (
        <GameContainer>
            <AppBar position='static' color='primary'>
                <Toolbar>
                    <IconButton component={Link} to='/' edge='start' color='inherit' size="large">
                        <ArrowBackIcon />
                    </IconButton>
                    <Typography variant='h6'>{ game.name }</Typography>
                    <GameInfo>
                        <Typography variant='subtitle2'>{ player.name } ({ player.num })</Typography>
                    </GameInfo>
                    <GameInfo>
                        <Typography variant='subtitle2'>Turn: { world.turnNumber }</Typography>
                    </GameInfo>
                    <Button color='inherit' variant='outlined' component={Link as any} to={``}>Map</Button>
                    <Button color='inherit' variant='outlined' component={Link as any} to={`stats`}>Statistics</Button>
                    { game.university?.locations?.length > 0 && <Button color='inherit' variant='outlined' component={Link as any} to={`university`}>University</Button> }
                    <Box flex={1} />
                    <Button color='inherit' onClick={game.getOrders}>Download Orders</Button>
                </Toolbar>
            </AppBar>
            <Outlet />
        </GameContainer>
    );
})

interface ProgressItemProps {
    text: string
    progress: number
}

function ProgressItem({ text, progress }: ProgressItemProps) {
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

interface LoadingProps {
    loading: GameLoadingStore
}

const Loading = observer(({ loading }: LoadingProps) => {

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

const GameInner = observer(() => {
    const [ loading ] = React.useState(() => new GameLoadingStore())
    const { gameId } = useParams()
    const { game } = useStore()
    const mapContext = useMapContext()

    React.useEffect(() => {
        game.load(gameId, loading)
            .then(() => {
                loading.begin('Map graphics')
                return mapContext.load()
            })
            .then(() => loading.end())
    }, [ gameId ])

    return loading.isLoading
        ? <Loading loading={loading} />
        : <GameComponent />
})

export function GamePage() {
    return <MapProvider>
        <GameInner />
    </MapProvider>
}
