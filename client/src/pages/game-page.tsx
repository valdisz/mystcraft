import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams, Switch, Route, useRouteMatch } from 'react-router-dom'
import { useCallbackRef, useCopy } from '../lib'
import { Box, AppBar, Typography, Toolbar, IconButton, Table, TableHead, TableRow, TableCell, TableBody, Tabs, Tab, Paper, Button } from '@material-ui/core'
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react-lite'
import { HexMap } from '../map'
import { Region } from "../store/game/types"
import { GameRouteParams } from './game-route-params'
import ArrowBackIcon from '@material-ui/icons/ArrowBack'
import { List } from '../store/game/list'
import { Item } from '../store/game/item'
import { RegionSummary } from '../components/region-summary'
import { StatsPage } from './stats-page'
import Editor from 'react-simple-code-editor'
import Prism from 'prismjs'

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
    border-top: 1px solid silver;

    display: flex;
    align-items: stretch;

    @media (max-width: 640px) {
        display: none;
    }
`

const OrdersEditor = styled(Editor)`
    min-height: 100%;

    textarea {
        min-height: 100%;
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

Prism.languages.atlantis = language

function highlight(s: string) {
    const result = Prism.highlight(s, language, 'atlantis')

    return result
}

const Orders = () => {
    const { game } = useStore()

    return <OrdersContainer>
        <Box m={1} flex={1}>
            <Observer>
                {() => <OrdersEditor value={game.unit?.orders ?? ''} onValueChange={game.unit?.setOrders} highlight={highlight} />}
            </Observer>
        </Box>
    </OrdersContainer>
}

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

    .structure-nr, .unit-nr {
        width: 1px;
        text-align: right;
        padding-right: 4px;
    }

    .structure-name, .unit-name {
        width: 1px;
        padding-left: 4px;
    }

    .faction {
        width: 1px;
    }

    .men {
        width: 1px;
    }

    .money {
        width: 1px;
    }

    .mounts {
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

    .no-border {
        border-bottom-width: 0;
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

const UnitsComponent = observer(() => {
    const { game } = useStore()
    const copy = useCopy(false, {
        text: game.toBattleSim
    })

    return <UnitsContainer>
        <Paper>
            <Button ref={copy}>To Battle Sim</Button>
        </Paper>
        <UnitsTable size='small' stickyHeader>
            <TableHead>
                <TableRow>
                    <TableCell className='structure-nr'></TableCell>
                    <TableCell className='structure-name'>Structure</TableCell>
                    <TableCell className='faction'>Faction</TableCell>
                    <TableCell className='unit-nr'>Unit Nr.</TableCell>
                    <TableCell className='unit-name'>Name</TableCell>
                    <TableCell className='men'>Men</TableCell>
                    <TableCell className='money'>Money</TableCell>
                    <TableCell className='mounts'>Mounts</TableCell>
                    <TableCell className='items'>Items</TableCell>
                    <TableCell className='skills'>Skills</TableCell>
                </TableRow>
            </TableHead>
            <TableBody>
            {game.units.map((unit) => {
                const rows = unit.description ? 2 : 1
                const noBorder = rows > 1 ? 'no-border' : ''

                return <React.Fragment key={unit.id}>
                    <TableRow onClick={() => game.selectUnit(unit)} selected={unit.num === game.unit?.num}>
                        <TableCell rowSpan={rows} className='structure-nr'>{unit.structure?.num ?? null}</TableCell>
                        <TableCell rowSpan={rows} className='structure-name'>{unit.structure?.name ?? null}</TableCell>
                        <TableCell rowSpan={rows} className='faction'>{unit.faction ? `${unit.faction.name} (${unit.faction.num})` : null}</TableCell>
                        <TableCell className={`unit-nr ${noBorder}`}>{unit.num}</TableCell>
                        <TableCell component="th" className={`unit-name ${noBorder}`}>{unit.name}</TableCell>
                        <TableCell className={`men ${noBorder}`}>
                            <UnitMen items={unit.inventory.items} />
                        </TableCell>
                        <TableCell className={`money ${noBorder}`}>{unit.money ? unit.money : null}</TableCell>
                        <TableCell className={`mounts ${noBorder}`}>
                            <UnitMounts items={unit.inventory.items} />
                        </TableCell>
                        <TableCell className={`items ${noBorder}`}>{unit.inventory.items.all.filter(x => !x.isManLike && !x.isMoney && !x.isMount).map(x => `${x.amount} ${x.name}`).join(', ')}</TableCell>
                        <TableCell className={`skills ${noBorder}`}>{unit.skills.all.map(x => `${x.name} ${x.level} (${x.days})`).join(', ')}</TableCell>
                    </TableRow>
                    { rows > 1 && <TableRow>
                        <TableCell className='unit-nr'></TableCell>
                        <TableCell colSpan={6} className='description'>
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
    const { game: { region } } = useStore()

    return <RegionContainer>
        <RegionSummary region={region} />
    </RegionContainer>
})

const MapTab = observer(() => {
    const { game } = useStore()

    return <GameGrid>
        <GameMapComponent getRegion={(x, y) => game.world.getRegion(x, y, 1)} onRegionSelected={game.selectRegion} />
        <UnitsComponent />
        <StructuresComponent />
        { game.region && <RegionComponent /> }
        <Orders />
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
            ? <div>Loading...</div>
            : <GameComponent />
        }
    </Observer>
}

