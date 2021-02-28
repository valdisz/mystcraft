import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { AppBar, Typography, Toolbar, IconButton, TextField, Table, TableHead, TableRow, TableCell, TableBody } from '@material-ui/core'
import ArrowBackIcon from '@material-ui/icons/ArrowBack';
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react-lite'
import { RegionFragment } from '../schema'
import { GameMap } from '../map'

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
// export declare const ResizeObserver: any

interface GamePageRouteParams {
    gameId: string
}

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
    regions: RegionFragment[]
    onRegionSelected: (col: number, row: number) => void
}

function GameMapComponent({ regions, onRegionSelected }: GameMapProps) {
    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const gameMapRef = React.useRef<GameMap>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        if (!gameMapRef.current) {
            gameMapRef.current = new GameMap(canvasRef, {
                width: 56,
                height: 56
             }, (x, y) => regions.find(r => r.x === x && r.y === y && r.z === 1))

             gameMapRef.current.onRegionSelected = onRegionSelected
        }

        const gameMap = gameMapRef.current
        gameMap
            .load()
            .then(() => gameMap.update())
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
`

const UnitsComponent = observer(() => {
    const { game } = useStore()
    return <UnitsContainer>
        <UnitsTable size='small' stickyHeader>
            <TableHead>
                <TableRow>
                    <TableCell>Number</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Skills</TableCell>
                    <TableCell>Items</TableCell>
                </TableRow>
            </TableHead>
            <TableBody>
            {game.units.map((row) => (
                <TableRow key={row.id}>
                    <TableCell>{row.number}</TableCell>
                    <TableCell component="th">{row.name}</TableCell>
                    <TableCell>{row.skills.map(x => `${x.code} ${x.level} (${x.days})`).join(', ')}</TableCell>
                    <TableCell>{row.items.map(x => `${x.amount} ${x.code}`).join(', ')}</TableCell>
                </TableRow>
            ))}
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
            {game.structures.map((row) => (
                <StructureItem key={row.id}>
                    {row.name} [{row.number}]: {row.type}
                </StructureItem>
            ))}
        </StructuresBody>
    </StructuresContainer>
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
                    <Typography variant='subtitle2'>{ game.playerFactionName } [{ game.playerFactionNumber }]</Typography>
                </GameInfo>
                <GameInfo>
                    <Typography variant='subtitle2'>Turn: { game.turn.number }</Typography>
                </GameInfo>
            </Toolbar>
        </AppBar>
        <GameGrid>
            <GameMapComponent regions={game.regions} onRegionSelected={game.selectRegion} />
            <UnitsComponent />
            <StructuresComponent />
            <Orders />
        </GameGrid>
    </GameContainer>
})

export function GamePage() {
    const { gameId } = useParams<GamePageRouteParams>()
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

