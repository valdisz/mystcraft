import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { AppBar, Typography, Toolbar, IconButton, TextField, Table, TableHead, TableRow, TableCell, TableBody } from '@material-ui/core'
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react-lite'
import { HexMap } from '../map'
import { Region } from "../store/game/region"
import { GameRouteParams } from './game-route-params'
import ArrowBackIcon from '@material-ui/icons/ArrowBack'
import { List } from '../store/game/list'
import { Item } from '../store/game/item'

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
// export declare const ResizeObserver: any


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

    grid-template-columns: 1fr 4fr 1fr;
    grid-template-rows: 2fr 1fr;
    grid-template-areas:
        "structures map details"
        "units units orders";
`

const OrdersContainer = styled.div`
    grid-area: orders;
`

const OrdersBody = styled.div`
    margin: 1rem;

    textarea {
        width: 100%;
        height: 100%;
    }
`

const Orders = () => {
    return <OrdersContainer>
        <OrdersBody>
            <TextField variant='outlined' rows={10} multiline fullWidth />
        </OrdersBody>
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
                const { x, y } = game.world.levels[1].regions[0].coords
                gameMap.centerAt(x, y)
                gameMap.update()
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

    .faction {
        width: 1px;
    }

    .unit-nr {
        width: 1px;
        text-align: right;
        padding-right: 4px;
    }

    .unit-name {
        width: 1px;
        padding-left: 4px;
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
    const men = items.all.filter(x => x.isMan)
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
    return <UnitsContainer>
        <UnitsTable size='small' stickyHeader>
            <TableHead>
                <TableRow>
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
                    <TableRow>
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
                        <TableCell className={`items ${noBorder}`}>{unit.inventory.items.all.filter(x => !x.isMan && !x.isMoney && !x.isMount).map(x => `${x.amount} ${x.name}`).join(', ')}</TableCell>
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
            {/* {game.structures.map((row) => (
                <StructureItem key={row.id}>
                    {row.name} [{row.number}]: {row.type}
                </StructureItem>
            ))} */}
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
`

const RegionComponent = observer(() => {
    const { game: { region } } = useStore()

    return <RegionContainer>
        <RegionBody>
            <h4>{region.province.name}</h4>

            <div>
                {region.terrain.code} ({region.coords.x},{region.coords.y}{region.coords.z !== 1 ? `,${region.coords.z}` : ''})
                {' '}{region.population.amount} {region.population.race.getName(region.population.amount)}
            </div>

            <h4>Products</h4>
            { region.products.all.map(p => <div key={p.code}>
                {p.amount} {p.name}
            </div>)}

            <h4>Wanted</h4>
            { region.wanted.all.map(p => <div key={p.code}>
                {p.amount} {p.name} for ${p.price}
            </div>)}

            <h4>For sale</h4>
            { region.forSale.all.map(p => <div key={p.code}>
                {p.amount} {p.name} for ${p.price}
            </div>)}
        </RegionBody>
    </RegionContainer>
})

const GameComponent = observer(() => {
    const { game } = useStore()

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
            </Toolbar>
        </AppBar>
        <GameGrid>
            <GameMapComponent getRegion={(x, y) => game.world.getRegion(x, y, 1)} onRegionSelected={game.selectRegion} />
            <UnitsComponent />
            <StructuresComponent />
            { game.region && <RegionComponent /> }
            <Orders />
        </GameGrid>
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

