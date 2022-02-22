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
    Tabs,
    Tab,
    Paper,
    Button,
    DialogContent,
    DialogContentText,
    Theme,
    Chip,
    ButtonGroup,
    Avatar,
    Box,
    Tooltip,
} from '@mui/material';
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react'
import { HexMap2 } from '../map'
import { Region } from "../game"
import { GameRouteParams } from './game-route-params'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { ItemMap } from '../game'
import { Item } from '../game'
import { Unit } from '../game'
import { RegionSummary } from '../components/region-summary'
import { StatsPage } from './stats-page'
import Editor from 'react-simple-code-editor'
import Prism from 'prismjs'
import { Dialog } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import { GameStore, OrdersState } from '../store/game-store'
import { Ruleset } from '../game'
import { UnitSummary } from '../components'
import { Capacity } from '../game'
import { green, lightBlue } from '@mui/material/colors'
import { InterfaceCommand } from '../store/commands/move';
import { Coords, ICoords } from '../game'
// import { UniversityPage } from './university-page'

const GameContainer = styled('div')`
    width: 100%;
    height: 100%;

    display: flex;
    flex-direction: column;
`

const GameGrid = styled('div')`
    flex: 1;

    min-height: 0;
    display: grid;

    grid-template-columns: minmax(200px, 1fr) 5fr minmax(300px, 1fr);
    grid-template-rows: 2fr 1fr;
    grid-template-areas:
        "structures map details"
        "units units orders";

    @media (max-width: 640px) {
        grid-template-columns: 1fr;
        grid-template-rows: 1fr 2fr 1fr;
        grid-template-areas:
            "details"
            "map"
            "units";
    }
`

const OrdersContainer = styled('div')`
    grid-area: orders;
    min-height: 0;
    overflow-y: auto;
    border: 1px solid ${({ theme }) => theme.palette.divider};

    display: flex;
    align-items: stretch;
    flex-direction: column;

    @media (max-width: 640px) {
        display: none;
    }
`

const OrdersEditor = styled(Editor)`
    min-height: 100%;

    textarea {
        min-height: 100%;

        &:focus-visible {
            outline: none;
        }
    }

    .comment {
        font-style: italic;
    }

    .string {
        color: blue;
    }

    .directive {
        font-weight: bold;
        color: purple;
    }

    .number {
        color: green;
    }

    .repeat {
        color: #333;
    }

    .order {
        font-weight: bold;
    }

    .error {
        text-decoration: red wavy underline;
    }
`

const language = {
    directive: /^@;(needs|route|tag)/mi,
    comment: /^@?;.*/m,
    repeat: /^@/m,
    string: /"(?:""|[^"\r\f\n])*"/i,
    number: /\d+/,
    order: {
        pattern: /^(@?)(\w+)/m,
        lookbehind: true
    }
}

const ORDER_VALIDATOR = {
    isValidOrder: (order: string) => true
}

Prism.hooks.add('wrap', env => {
    if (env.type !== 'order') {
        return
    }

    if (!ORDER_VALIDATOR.isValidOrder(env.content)) {
        env.classes.push('error')
    }
})

function highlight(ruleset: Ruleset, s: string) {
    ORDER_VALIDATOR.isValidOrder = order => {
        return ruleset.orders.includes(order.toUpperCase())
    }

    const result = Prism.highlight(s, language, null)

    return result
}

const OrdersEditorBox = styled('div')`
    flex: 1;
    padding: 0.5rem;
    background-color: ${x => x.theme.palette.common.white};
`

const OrdersStatusBox = styled('div')`
    padding: 0.25rem;
    text-align: center;

    &.order-state--saved {
        color: ${x => x.theme.palette.success.main};
    }

    &.order-state--saving, &.order-state--unsaved {
        background-color: ${x => x.theme.palette.info.light};
        color: ${x => x.theme.palette.info.contrastText};
    }

    &.order-state--error {
        background-color: ${x => x.theme.palette.error.main};
        color: ${x => x.theme.palette.error.contrastText};
    }
`

function renderOrderState(state: OrdersState) {
    switch (state) {
        case 'ERROR': return 'Error'
        case 'SAVED': return 'Saved'
        default: return 'Saving...'
    }
}

interface OrdersStatusProps {
    state: OrdersState
}

function OrdersStatus({ state }: OrdersStatusProps) {
    return <OrdersStatusBox className={`order-state--${state.toLowerCase()}`}>
        <Typography variant='caption'>{renderOrderState(state)}</Typography>
    </OrdersStatusBox>
}

const OrdersTitle = styled(Typography)`
    padding: 0.5rem;
`

interface OrdersProps {
    readOnly: boolean
}

const Orders = observer(({ readOnly }: OrdersProps) => {
    const store = useStore()
    const game = store.game
    const ruleset = game.world.ruleset

    return <OrdersContainer>
        <OrdersTitle variant='subtitle1'>Orders</OrdersTitle>
        <OrdersEditorBox>
            <OrdersEditor readOnly={readOnly} value={game.unitOrders ?? ''} onValueChange={game.setOrders} highlight={s => highlight(ruleset, s)} />
        </OrdersEditorBox>
        <OrdersStatus state={game.ordersState} />
    </OrdersContainer>
})

const MapContainer = styled('div')`
    grid-area: map;
    background-color: #000;
    min-height: 0;
`

const MapCanvas = styled('canvas')`
    width: 100%;
    height: 100%;
`

const GameInfo = styled('div')`
    margin-left: 1rem;
`

interface GameMapProps {
    selectedRegion: ICoords | null
    onRegionSelected: (reg: Region) => void
}

function GameMapComponent({ selectedRegion, onRegionSelected }: GameMapProps) {
    const { game } = useStore()

    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const [ gameMap, setGameMap ] = React.useState<HexMap2>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        const level = game.world.getLevel(1)
        const map = new HexMap2(canvasRef, level.width, level.height, {
            onClick: onRegionSelected
        })

        map.load()
            .then(() => {
                const regions = level.toArray()
                map.setRegions(regions)

                const { player } = game.world.factions
                let coords: Coords = game.region?.coords

                if (!coords) {
                    const lastLocation = window.localStorage.getItem('coords')
                    if (lastLocation) {
                        coords = JSON.parse(lastLocation)
                    }
                }

                if (!coords) {
                    const unit = player.troops.first()
                    if (unit) {
                        coords = unit.region.coords
                    }
                }

                if (coords) {
                    map.centerAt(coords)
                }

                setGameMap(map)
                map.render()
            })

        return (() => map.destroy())
    }, [ canvasRef ])

    React.useEffect(() => {
        if (!gameMap) {
            return
        }

        gameMap.select(selectedRegion)
    }, [ selectedRegion, gameMap ])

    return <MapContainer>
        <MapCanvas ref={setCanvasRef} />
    </MapContainer>
}

const UnitsContainer = styled('div')`
    grid-area: units;

    display: flex;
    flex-direction: column;
    min-height: 0;
    overflow: auto;
`

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

    .faction-nr, .structure-nr, .unit-nr {
        width: 1px;
        text-align: right;
        padding-right: 4px;
    }

    .faction-name, .structure-name, .unit-name {
        width: 1px;
        padding-left: 4px;
    }

    .men, .money, .mounts, .movement, .weight, .capacity {
        width: 1px;
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

function UnitMen({ items }: { items: ItemMap<Item> }) {
    const men = items.filter(x => x.isManLike)
    men.sort((a, b) => b.amount - a.amount)
    const total = men.map(x => x.amount).reduce((value, next) => value + next, 0)
    const names = men.map(x => x.name).join(', ')

    return <>{total} {names}</>
}

function UnitMounts({ items }: { items: ItemMap<Item> }) {
    const mounts = items.filter(x => x.isMount)
    mounts.sort((a, b) => b.amount - a.amount)

    const total = mounts.map(x => x.amount).reduce((value, next) => value + next, 0)
    if (!total) return null

    const names = mounts.map(x => x.name).join(', ')

    return <>{total} {names}</>
}

function UnitRow({ unit, game }: { unit: Unit, game: GameStore }) {
    const rows = unit.description ? 2 : 1
    const noBorder = rows > 1 ? 'no-border' : ''

    return <React.Fragment>
        <TableRow className={noBorder} onClick={() => game.selectUnit(unit)} selected={unit.num === game.unit?.num}>
            <TableCell rowSpan={rows}>
                { game.isAttacker(unit)
                    ? <Chip label='Attacker' color='primary' />
                    : game.isDefender(unit)
                        ? <Chip label='Defender' color='secondary' />
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
            <TableCell className='men'>
                <UnitMen items={unit.inventory.items} />
            </TableCell>
            <TableCell className='mounts'>
                <UnitMounts items={unit.inventory.items} />
            </TableCell>
            <TableCell className='items'>{unit.inventory.items.filter(x => !x.isManLike && !x.isMoney && !x.isMount).map(x => `${x.amount} ${x.name}`).join(', ')}</TableCell>
            <TableCell className='skills'>{unit.skills.map(x => `${x.name} ${x.level} (${x.days})`).join(', ')}</TableCell>
        </TableRow>
        { rows > 1 && <TableRow>
            <TableCell className='unit-nr'></TableCell>
            <TableCell colSpan={5} className='description'>
                {unit.description}
            </TableCell>
        </TableRow> }
    </React.Fragment>
}

function renderUnitMovement(unit: Unit) {
    if (!unit) return null

    const move = unit.isOverweight
        ? <Chip size='small' color='error' label='overweight' />
        : <Chip size='small' label={unit.moveType} />

    const swim = unit.canSwim
        ? <Chip size='small' color='info' label='swim' />
        : null

    return <>
        {move}
        {swim}
    </>
}

const UnitCapacityContainer = styled('div')`
    display: flex;
    gap: 0.5rem;
`

const UnitCapacityItem = styled('span')`
    white-space: pre;
`

function renderCapacity(value: number) {
    const s = (value || 0).toString()
    return s.padStart(5, ' ')
}

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

const UnitsComponent = observer(() => {
    const { game } = useStore()

    return (
        <UnitsContainer>
            <Paper>
                <Box p={1}>
                    <Button onClick={game.openBattleSim}>Battle Sim</Button>
                    {game.commands.filter(x => x.visible).map(x => { console.log(x); return <CommandButton key={x.title} command={x} /> })}
                </Box>

                <Dialog fullScreen  open={game.battleSimOpen} onClose={game.closeBattleSim}>
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
                </Dialog>
            </Paper>
            <UnitsTable size='small' stickyHeader>
                <TableHead>
                    <TableRow>
                        <TableCell className='structure-nr'></TableCell>
                        <TableCell className='structure-name'>Structure</TableCell>
                        <TableCell className='faction-nr'></TableCell>
                        <TableCell className='faction-name'>Faction</TableCell>
                        <TableCell className='unit-nr'></TableCell>
                        <TableCell className='unit-name'>Unit</TableCell>
                        <TableCell className='men'>Men</TableCell>
                        <TableCell className='money'>Money</TableCell>
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
                            <TableCell className='men'>
                                <UnitMen items={unit.inventory.items} />
                            </TableCell>
                            <TableCell className='money'>{unit.money ? unit.money : null}</TableCell>
                            <TableCell className='mounts'>
                                <UnitMounts items={unit.inventory.items} />
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
        </UnitsContainer>
    );
})

const StructuresContainer = styled('div')`
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

const RegionBody = styled('div')`
    min-height: 0;
    margin: 1rem;
    font-family: Fira Code, monospace;
`

const RegionComponent = observer(() => {
    const { game: { region, unit } } = useStore()

    return <RegionContainer>
        <RegionSummary region={region} />
        { unit && <UnitSummary unit={unit} /> }
    </RegionContainer>
})

export const MapTab = observer(() => {
    const { game } = useStore()

    return <GameGrid>
        <GameMapComponent selectedRegion={game.region?.coords} onRegionSelected={game.selectRegion} />
        <UnitsComponent />
        <StructuresComponent />
        { game.region && <RegionComponent /> }
        { game.isOrdersVisible && <Orders readOnly={game.isOrdersReadonly} /> }
    </GameGrid>
})

const GameComponent = observer(() => {
    const { game } = useStore()

    return (
        <GameContainer>
            <AppBar position='static' color='primary'>
                <Toolbar>
                    <IconButton component={Link} to='/' edge='start' color='inherit' size="large">
                        <ArrowBackIcon />
                    </IconButton>
                    <Typography variant='h6'>{ game.name }</Typography>
                    <GameInfo>
                        <Typography variant='subtitle2'>{ game.factionName } [{ game.factionNumber }]</Typography>
                    </GameInfo>
                    <GameInfo>
                        <Typography variant='subtitle2'>Turn: { game.turn.number }</Typography>
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

export function GamePage() {
    const { game } = useStore()
    const { gameId } = useParams()

    React.useEffect(() => {
        game.load(gameId)
    }, [ gameId ])

    return <Observer>
        {() => game.loading
            ? <div>{ game.loadingMessage }</div>
            : <GameComponent />
        }
    </Observer>
}

