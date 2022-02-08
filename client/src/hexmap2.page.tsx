import * as React from 'react'
import { styled } from '@mui/material/styles'
import { useCallbackRef } from './lib'
import { HexMap2 } from './map'

const MapCanvas = styled('canvas')`
    width: 100%;
    height: 100%;
`

export function Hexmap2Page() {
    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const gameMapRef = React.useRef<HexMap2>(null)
    const observerRef = React.useRef<ResizeObserver>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        if (!gameMapRef.current) {
            gameMapRef.current = new HexMap2(canvasRef)
        }

        if (!observerRef.current) {
            observerRef.current = new ResizeObserver(items => {
                const rect = items[0].contentRect
                gameMapRef.current.resize(rect.width, rect.height)
            })
            observerRef.current.observe(canvasRef)
        }

        const gameMap = gameMapRef.current
        gameMap.load().then(() => gameMap.render())


        return (() => {
            observerRef.current.unobserve(canvasRef)
            observerRef.current = null

            gameMap.destroy()
            gameMapRef.current = null
        })
    }, [ canvasRef ])

    return <MapCanvas ref={setCanvasRef} />
}
