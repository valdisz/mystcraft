import * as React from 'react'
import { styled } from '@mui/material/styles'
import { useCallbackRef } from './lib'
import { HexMap2 } from './map'
import { Region } from './game/region'

const MapCanvas = styled('canvas')`
    width: 100%;
    height: 100%;
`

export function Hexmap2Page() {
    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const gameMapRef = React.useRef<HexMap2>(null)
    const observerRef = React.useRef<ResizeObserver>(null)
    const [ region, setRegion ] = React.useState<Region>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        if (!gameMapRef.current) {
            gameMapRef.current = new HexMap2(canvasRef, 64, 64, {
                onClick: (reg) => {
                    setRegion(reg)
                },
                onDblClick: (reg) => {
                    gameMapRef.current.centerAt(reg.coords)
                }
            })
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

    React.useEffect(() => {
        const map = gameMapRef.current
        if (!map) {
            return
        }

        map.select(region?.coords)

    }, [ gameMapRef.current, region ])

    return <MapCanvas ref={setCanvasRef} />
}
