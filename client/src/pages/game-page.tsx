import * as React from 'react'
import styled, { createGlobalStyle } from 'styled-components'
import { CLIENT } from '../client'
import { GetMapQuery, GetMap, GetMapQueryVariables, Region as RegionData, GetLastTurnMapQuery, GetLastTurnMap, GetLastTurnMapQueryVariables } from '../schema'
import PIXI, { Container, Renderer, Graphics, Polygon, autoDetectRenderer } from 'pixi.js'
import gql from 'graphql-tag'
import throttle from 'lodash.throttle'
import { useParams } from 'react-router-dom'
import { useCallbackRef } from '../lib'
import { Card, CardContent } from '@material-ui/core'

class Region extends Container {
    constructor(public readonly pX: number, public readonly pY: number, public readonly terrain: string, scale: number = 4) {
        super()

        const w = 14 * scale
        const h = 12 * scale

        this.sizeW = w
        this.sizeH = h

        const hex = new Polygon([
            w,          h / 2,
            w / 4 * 3,  h,
            w / 4,      h,
            0,          h / 2,
            w / 4,      0,
            w / 4 * 3,  0
        ])

        const terrainMap = {
            'plain':       0xa2a552,
            'forest':      0x2a603b,
            'hill':        0xc9c832,
            'mountain':    0xf7f9fb,
            'lake':        0x86aba5,
            'swamp':       0x52593b,
            'tundra':      0x8c9e5e,
            'jungle':      0x7a942e,
            'desert':      0xedc9af,
            'wasteland':   0x63424b,

            'ocean':       0x2654a1,
            'shallows':    0x54a5d5,
            'deeps':       0x243274,

            'cavern':      0xf0f0f0,
            'chasm':       0xf0f0f0,
            'deepforest':  0xf0f0f0,
            'grotto':      0xf0f0f0,
            'mystforest':  0xf0f0f0,
            'tunnels':     0xf0f0f0,
            'underforest': 0xf0f0f0,
        }
        const color = terrainMap[terrain] || 0xf0f0f0;

        const g = new Graphics()
        g.beginFill(color)
        g.drawPolygon(hex)
        g.endFill()

        this.addChild(g)

        const dX = 0
        const dY = 0

        this.x = (pX - 1) * (this.width / 4 * 3) - dX * pX
        this.y = (pY - 1) * (this.height / 2) - dY * pY
        this.zIndex = pY
    }

    readonly sizeW
    readonly sizeH
}

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
declare const ResizeObserver: any

interface IViewport {
    readonly width: number
    readonly height: number
    readonly offsetX: number
    readonly offsetY: number
    readonly zoom: number
}

class Viewport implements IViewport {
    constructor(private element: HTMLElement) {
        this.observer = new ResizeObserver(this.raiseOnUpdate)
        this.observer.observe(element)

        element.addEventListener('pointerdown', this.onPanStart)

        element.addEventListener('pointermove', this.onPan)

        element.addEventListener('pointerup', this.onPanEnd)
        element.addEventListener('pointercancel', this.onPanEnd)
        element.addEventListener('pointerleave', this.onPanEnd)
        element.addEventListener('pointerout', this.onPanEnd)

        element.addEventListener('wheel', this.onZoom)
    }

    private _offsetX = 0
    private _offsetY = 0

    private _zoom = 100

    private observer

    private paning = false
    private panOffsetX = 0
    private panOffsetY = 0
    private panX = 0
    private panY = 0

    private onPanStart = (e: MouseEvent) => {
        this.panOffsetX = this._offsetX
        this.panOffsetY = this._offsetY

        this.panX = e.x
        this.panY = e.y

        this.paning = true
    }

    private onPan = throttle((e: MouseEvent) => {
        if (!this.paning) return

        const deltaX = e.x - this.panX
        const deltaY = e.y - this.panY

        this._offsetX = Math.floor(this.panOffsetX + deltaX)
        this._offsetY = Math.floor(this.panOffsetY + deltaY)

        window.requestAnimationFrame(() => this.raiseOnUpdate())
    }, 30)

    private onPanEnd = (e: MouseEvent) => {
        if (!this.paning) return

        this.paning = false
        this.raiseOnUpdate()
    }

    private onZoom = (e: WheelEvent) => {
        const z = Math.floor(e.deltaY * 0.1) + this._zoom
        this._zoom = Math.min(Math.max(z, 10), 400)

        this.raiseOnUpdate()
    }

    private raiseOnUpdate = () => this.onUpdate && this.onUpdate(this)

    get width() { return this.element.clientWidth }
    get height() { return this.element.clientHeight }

    get offsetX() { return this._offsetX }
    get offsetY() { return this._offsetY }

    get zoom() { return this._zoom / 100 }

    onUpdate?: (event: IViewport) => void
}

class Scene extends Container {
    constructor(canvas: HTMLCanvasElement) {
        super()

        this.viewport = new Viewport(canvas)

        this.renderer = autoDetectRenderer({
            width: this.viewport.width,
            height: this.viewport.height,
            view: canvas,
            antialias: true,
            resolution: window.devicePixelRatio || 1
        })

        this.viewport.onUpdate = ({ width, height, offsetX, offsetY, zoom }) => {
            this.renderer.resize(width, height)
            this.x = offsetX
            this.y = offsetY
            this.scale.set(zoom)
            this.update()
        }
    }

    readonly renderer: Renderer
    readonly viewport: Viewport

    update() {
        this.renderer.render(this)
    }
}

class GameMap {
    constructor(canvas: HTMLCanvasElement) {
        this.scene = new Scene(canvas)
    }

    private scene: Scene

    addRegion(x: number, y: number, terrain: string) {
        this.scene.addChild(new Region(x, y, terrain, 4))
    }

    finish() {
        this.scene.sortChildren()
        this.scene.update()
    }
}

const GlobalStyles = createGlobalStyle`
    html, body, #app-host {
        width: 100%;
        height: 100%;
        margin: 0;
        padding: 0;
    }
`

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

async function* loadMap(gameId: string) {
    const { data: { node: turnsNode } } = await CLIENT.query<GetLastTurnMapQuery, GetLastTurnMapQueryVariables>({
        query: GetLastTurnMap,
        variables: { gameId }
    })

    if (!turnsNode?.turns?.nodes?.length) return

    const { data: { node: regionsNode } } = await CLIENT.query<GetMapQuery, GetMapQueryVariables>({
        query: GetMap,
        variables: {
            turnId: turnsNode.turns.nodes[0].id
        }
    })

    if (!regionsNode?.regions?.nodes?.length) return

    for (const { x, y, z, terrain } of regionsNode.regions.nodes) {
        if (z !== 1) continue

        yield { x, y, terrain }
    }
}

const GameContainer = styled.div`
    width: 100%;
    height: 100%;
    position: relative;
`

const Widget = styled.div`
    position: absolute;
`

const TopLeft = styled(Widget)`
    top: 1rem;
    left: 1rem;
`

const TopRight = styled(Widget)`
    top: 1rem;
    right: 1rem;
`

const BottomLeft = styled(Widget)`
    bottom: 1rem;
    left: 1rem;
`

const BottomRight = styled(Widget)`
    bottom: 1rem;
    right: 1rem;
`

const GameActions = styled.div`
    width: 200px;
`


export function GamePage() {
    const { gameId } = useParams<GamePageRouteParams>()
    const [ canvasRef, setCanvasRef ] = useCallbackRef<HTMLCanvasElement>()
    const map = React.useRef<GameMap>(null)

    React.useEffect(() => {
        if (!canvasRef) return

        const loadMapAsync = async () => {
            map.current = new GameMap(canvasRef)
            for await (const {x, y, terrain} of loadMap(gameId)) {
                map.current.addRegion(x, y, terrain)
            }
            map.current.finish()
        }

        loadMapAsync()
    }, [ canvasRef ])

    return <GameContainer>
        <GlobalStyles />
        <MapContainer>
            <MapCanvas ref={setCanvasRef} />
        </MapContainer>
        <TopLeft>
            <GameActions>
                <Card>
                    <CardContent>
                        Test
                    </CardContent>
                </Card>
            </GameActions>
        </TopLeft>
    </GameContainer>
}

