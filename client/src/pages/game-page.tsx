import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { AppBar, Typography, Toolbar, IconButton, Divider, Box } from '@material-ui/core'
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

const MapContainer = styled.div`
    display: flex;
    flex: 1;
    background-color: #000;
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
}

function GameMapComponent({ regions }: GameMapProps) {
    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const gameMapRef = React.useRef<GameMap>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        if (!gameMapRef.current) {
            gameMapRef.current = new GameMap(canvasRef, {
                width: 56,
                height: 56
             }, (x, y) => regions.find(r => r.x === x && r.y === y && r.z === 1))
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
        <GameMapComponent regions={game.regions} />
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

