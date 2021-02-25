import * as React from 'react'
import styled from 'styled-components'
import { Link, useParams } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { AppBar, Typography, Toolbar, IconButton, Divider, Box } from '@material-ui/core'
import ArrowBackIcon from '@material-ui/icons/ArrowBack';
import { useStore } from '../store'
import { Observer, observer } from 'mobx-react-lite'

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
export declare const ResizeObserver: any

const MapContainer = styled.div`
    width: 100%;
    height: 100%;
    background-color: #000;
`

const MapCanvas = styled.canvas`
    width: 100%;
    height: 100%;
`

interface GamePageRouteParams {
    gameId: string
}

// async function* loadMap(gameId: string) {
//     const { data: { node: turnsNode } } = await CLIENT.query<GetLastTurnMapQuery, GetLastTurnMapQueryVariables>({
//         query: GetLastTurnMap,
//         variables: { gameId }
//     })

//     if (!turnsNode?.turns?.nodes?.length) return

//     const { data: { node: regionsNode } } = await CLIENT.query<GetMapQuery, GetMapQueryVariables>({
//         query: GetMap,
//         variables: {
//             turnId: turnsNode.turns.nodes[0].id
//         }
//     })

//     if (!regionsNode?.regions?.nodes?.length) return

//     for (const { x, y, z, terrain } of regionsNode.regions.nodes) {
//         if (z !== 1) continue

//         yield { x, y, terrain }
//     }
// }

const GameContainer = styled.div`
    width: 100%;
    height: 100%;
    position: relative;
`

const Widget = styled.div`
    position: absolute;
`

const GameInfo = styled.div`
    margin-left: 1rem;
`

const GameComponent = observer(() => {
    const { game } = useStore()

    // const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    // const map = React.useRef<GameMap>(null)

    // React.useEffect(() => {
    //     if (!canvasRef) return

    //     const loadMapAsync = async () => {
    //         map.current = new GameMap(canvasRef)
    //         for await (const {x, y, terrain} of loadMap(gameId)) {
    //             map.current.addRegion(x, y, terrain)
    //         }
    //         map.current.finish()
    //     }

    //     loadMapAsync()
    // }, [ canvasRef ])

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
                    <Typography variant='subtitle2'>Turn: { game.lastTurnNumber }</Typography>
                </GameInfo>
            </Toolbar>
        </AppBar>
        {/* <MapContainer>
            <MapCanvas ref={setCanvasRef} />
        </MapContainer> */}
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

