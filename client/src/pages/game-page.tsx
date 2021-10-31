import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams, Switch, Route, useRouteMatch } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import {
    AppBar, Typography, Toolbar, IconButton, Table, TableHead, TableRow, TableCell, TableBody, Tabs, Tab, Paper, Button,
    DialogContent, DialogContentText,
    makeStyles, createStyles, Theme, Chip, ButtonGroup, Avatar, Box
} from '@material-ui/core'
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react-lite'
import { HexMap } from '../map'
import { Region } from "../store/game/types"
import { GameRouteParams } from './game-route-params'
import ArrowBackIcon from '@material-ui/icons/ArrowBack'
import { List } from '../store/game/list'
import { Item } from '../store/game/item'
import { Unit } from '../store/game/unit'
import { RegionSummary } from '../components/region-summary'
import { StatsPage } from './stats-page'
import Editor from 'react-simple-code-editor'
import Prism from 'prismjs'
import { Dialog } from '@material-ui/core'
import CloseIcon from '@material-ui/icons/Close'
import { GameStore, OrdersState } from '../store/game-store'
import { Ruleset } from '../store/game/ruleset'
import { UnitSummary } from '../components'
import { Capacity } from '../store/game/move-capacity'

import green from '@material-ui/core/colors/green'
import lightBlue from '@material-ui/core/colors/lightBlue'

const GameContainer = styled.div`
    width: 100%;
    height: 100%;

    display: flex;
    flex-direction: column;
`

const GameGrid = styled.div`
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

const OrdersContainer = styled.div`
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

const OrdersEditorBox = styled.div`
    flex: 1;
    padding: 0.5rem;
    background-color: ${x => x.theme.palette.common.white};
`

const OrdersStatusBox = styled.div`
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

const OrdersTitle = styled(Typography).attrs({
    variant: 'subtitle1'
})`
    padding: 0.5rem;
`

const Orders = observer(() => {
    const store = useStore()
    const game = store.game
    const ruleset = game.world.ruleset

    return <OrdersContainer>
        <OrdersTitle>Orders</OrdersTitle>
        <OrdersEditorBox>
            <OrdersEditor value={game.unitOrders ?? ''} onValueChange={game.setOrders} highlight={s => highlight(ruleset, s)} />
        </OrdersEditorBox>
        <OrdersStatus state={game.ordersState} />
    </OrdersContainer>
})

const MapContainer = styled.div`
    grid-area: map;
    background-color: #000;
    min-height: 0;
`

const MapCanvas = styled.canvas`
    width: 100%;
    height: 100%;
`

const GameInfo = styled.div`
    margin-left: 1rem;
`

interface GameMapProps {
    getRegion: (col: number, row: number) => Region
    onRegionSelected: (col: number, row: number) => void
}

function GameMapComponent({ getRegion, onRegionSelected }: GameMapProps) {
    const { game } = useStore()

    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const gameMapRef = React.useRef<HexMap>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        if (!gameMapRef.current) {
            gameMapRef.current = new HexMap(canvasRef, { width: 72, height: 96 }, getRegion)
            gameMapRef.current.onRegionSelected = onRegionSelected
        }

        const gameMap = gameMapRef.current
        gameMap
            .load()
            .then(() => {
                gameMap.turnNumber = game.turn.number



                if (game.region) {
                    const { x, y } = game.region.coords
                    gameMap.centerAt(x, y)
                }
                else {
                    let x;
                    let y;
                    let z = 1;

                    const coords : {
                        x: number,
                        y: number,
                        z: number
                    } = JSON.parse(window.localStorage.getItem('coords'))

                    if (coords) {
                        x = coords.x
                        y = coords.y
                        z = coords.z
                    }
                    else {
                        const c = game.world.levels[1].regions[0].coords
                        x = c.x
                        y = c.y
                        z = 1
                    }

                    gameMap.centerAt(x, y)
                }
            })

        return (() => {
            gameMap.destroy()
            gameMapRef.current = null
        })
    }, [ canvasRef ])

    return <MapContainer>
        <MapCanvas ref={setCanvasRef} />
    </MapContainer>
}

const UnitsContainer = styled.div`
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

function UnitMen({ items }: { items: List<Item> }) {
    const men = items.all.filter(x => x.isManLike)
    men.sort((a, b) => b.amount - a.amount)
    const total = men.map(x => x.amount).reduce((value, next) => value + next, 0)
    const names = men.map(x => x.name).join(', ')

    return <>{total} {names}</>
}

function UnitMounts({ items }: { items: List<Item> }) {
    const mounts = items.all.filter(x => x.isMount)
    mounts.sort((a, b) => b.amount - a.amount)

    const total = mounts.map(x => x.amount).reduce((value, next) => value + next, 0)
    if (!total) return null

    const names = mounts.map(x => x.name).join(', ')

    return <>{total} {names}</>
}

const useStyles = makeStyles((theme: Theme) =>
    createStyles({
        appBar: {
            position: 'relative',
        },
        title: {
            marginLeft: theme.spacing(2),
            flex: 1
        }
    })
)

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
            <TableCell className='items'>{unit.inventory.items.all.filter(x => !x.isManLike && !x.isMoney && !x.isMount).map(x => `${x.amount} ${x.name}`).join(', ')}</TableCell>
            <TableCell className='skills'>{unit.skills.all.map(x => `${x.name} ${x.level} (${x.days})`).join(', ')}</TableCell>
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
        : <Chip size='small' label={unit.movement} />

    const swim = unit.canSwim
        ? <Chip size='small' color='info' label='swim' />
        : null

    return <>
        {move}
        {swim}
    </>
}

const UnitCapacityContainer = styled.div`
    display: flex;
    gap: 0.5rem;
`

const UnitCapacityItem = styled.span`
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

const UnitsComponent = observer(() => {
    const { game } = useStore()
    const classes = useStyles()

    return <UnitsContainer>
        <Paper>
            <Button onClick={game.openBattleSim}>Battle Sim</Button>
            <Button onClick={game.markStart}>MARK START</Button>
            <Button onClick={game.searchPath}>SEARCH PATH</Button>
            <Dialog fullScreen  open={game.battleSimOpen} onClose={game.closeBattleSim}>
                <AppBar className={classes.appBar}>
                    <Toolbar>
                        <IconButton edge="start" color="inherit" onClick={game.closeBattleSim}><CloseIcon /></IconButton>
                        <Typography variant="h6" className={classes.title}>Battle Sim</Typography>
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
})

const StructuresContainer = styled.div`
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

const StructuresBody = styled.div`
    min-height: 0;
`

const StructureItem = styled.div`
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

const RegionContainer = styled.div`
    grid-area: details;

    display: flex;
    flex-direction: column;
    min-height: 0;
    overflow: auto;
`

const RegionBody = styled.div`
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

const MapTab = observer(() => {
    const { game } = useStore()

    return <GameGrid>
        <GameMapComponent getRegion={(x, y) => game.world.getRegion(x, y, 1)} onRegionSelected={game.selectRegion} />
        <UnitsComponent />
        <StructuresComponent />
        { game.region && <RegionComponent /> }
        { game.isOrdersVisible && <Orders /> }
    </GameGrid>
})

const GameComponent = observer(() => {
    const { game } = useStore()
    const { path, url } = useRouteMatch()

    return <GameContainer>
        <AppBar position='static' color='primary'>
            <Toolbar>
                <IconButton component={Link} to='/' edge='start' color='inherit'>
                    <ArrowBackIcon />
                </IconButton>
                <Typography variant='h6'>{ game.name }</Typography>
                <GameInfo>
                    <Typography variant='subtitle2'>{ game.factionName } [{ game.factionNumber }]</Typography>
                </GameInfo>
                <GameInfo>
                    <Typography variant='subtitle2'>Turn: { game.turn.number }</Typography>
                </GameInfo>
                <Tabs value={url}>
                    <Tab label='Map' component={Link} value={url} to={url} />
                    <Tab label='Statistics' component={Link} value={`${url}/stats`} to={`${url}/stats`} />
                </Tabs>
                <Box flex={1} />
                <Button color='inherit' onClick={game.getOrders}>Download Orders</Button>
            </Toolbar>
        </AppBar>
        <Switch>
            <Route path={`${path}/stats`}>
                <StatsPage />
            </Route>
            <Route path={path}>
                <MapTab />
            </Route>
        </Switch>
    </GameContainer>
})

export function GamePage() {
    const { gameId } = useParams<GameRouteParams>()
    const { game } = useStore()

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

