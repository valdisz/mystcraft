import * as React from 'react'
import styled, { createGlobalStyle } from 'styled-components'
import { List, ListSubheader, ListItem, ListItemText, Divider } from '@material-ui/core'
import { IObservableArray, runInAction, observable } from 'mobx'
import { useObserver, useLocalStore } from 'mobx-react-lite'
import { CLIENT } from '../client'
import { GameListItem, GetGamesQuery, GetGames, GetMapQuery, GetMap, GetMapQueryVariables } from '../schema'
import { Link, useParams } from 'react-router-dom'
import { Container, Renderer, Graphics, Polygon, Point } from 'pixi.js'
import gql from 'graphql-tag'

class Region extends Container {
    constructor(terrain: string, scale: number = 4) {
        super()

        const w = 16 * scale
        const h = 14 * scale
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

        this.addChild(g)
    }
}

// till Typescript adds official declarations for this API (https://github.com/microsoft/TypeScript/issues/37861)
declare const ResizeObserver: any

class Viewport {
    constructor(private element: HTMLElement) {
        this.observer = new ResizeObserver(() => {
            if (this.onResize) {
                this.onResize({ width: this.width, height: this.height })
            }
        })
        this.observer.observe(element)
    }

    private observer

    get width() { return this.element.clientWidth }
    get height() { return this.element.clientHeight }

    onResize?: (event: { width: number, height: number }) => void
}

class Scene extends Container {
    constructor(canvas: HTMLCanvasElement) {
        super()

        this.viewport = new Viewport(canvas)

        this.renderer = new Renderer({
            width: this.viewport.width,
            height: this.viewport.height,
            view: canvas
        })

        this.viewport.onResize = ({ width, height}) => {
            this.renderer.resize(width, height)
            this.update()
        }
    }

    private renderer: Renderer
    private viewport: Viewport

    update() {
        this.renderer.render(this)
    }
}

class GameMap {
    constructor(canvas: HTMLCanvasElement) {
        this.scene = new Scene(canvas)

        this.scene.update()
    }

    private scene: Scene

    addRegion(x: number, y: number, terrain: string) {
        const region = new Region(terrain, 2)
        region.x = (x - 1) * (region.width / 4 * 3)
        region.y = (y - 1) * (region.height / 2)
        this.scene.addChild(region)
    }

    finish() {
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
    background-color: red;
`

const MapCanvas = styled.canvas`
    width: 100%;
    height: 100%;
`

export function GamePage() {
    const { gameId } = useParams()
    const canvasRef = React.useRef()
    const map = React.useRef<GameMap>(null)

    React.useEffect(() => {
        CLIENT.query({
            query: gql`
            query GetLastTurn($gameId: ID!) {
                node(id: $gameId) {
                  ... on Game {
                    turns(last: 1) {
                      nodes {
                        id
                      }
                    }
                  }
                }
              }
            `,
            variables: { gameId }
        })
        .then(res => res.data.node.turns.nodes[0].id)
        .then(turnId => CLIENT.query<GetMapQuery, GetMapQueryVariables>({
            query: GetMap,
            variables: { turnId }
        }))
        .then(res => {
            map.current = new GameMap(canvasRef.current)

            for (const { x, y, z, terrain } of res.data.node.regions.nodes) {
                if (z !== 1) continue

                map.current.addRegion(x, y, terrain)
            }

            map.current.finish()
        })
    }, [ canvasRef.current ])

    return <>
        <GlobalStyles />
        <MapContainer>
            <MapCanvas ref={canvasRef} />
        </MapContainer>
    </>
}
