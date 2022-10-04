import { Loader } from 'pixi.js'
import * as React from 'react'
import { Level, Region, Troops, Coords } from '../game'
import { HexMap2 } from './hex-map'
import { Resources } from './resources'

export class MapContext {
    constructor() {
        this.resources = new Resources(new Loader())
    }

    readonly resources: Resources
    map: HexMap2

    load() {
        return this.resources.load()
    }

    initialize(canvas: HTMLCanvasElement, level: Level, onRegionSelected: (reg: Region) => void) {
        this.map = new HexMap2(canvas, this.resources, level.width, level.height, {
            onClick: onRegionSelected
        })

        const regions = level.toArray()
        this.map.setRegions(regions)

        return (() => {
            this.map.destroy()
            this.map = null
        })
    }

    findCoordsToCenterAt(troops: Troops, region?: Region) {
        let coords: Coords = region?.coords

        if (!coords) {
            const lastLocation = window.localStorage.getItem('coords')
            if (lastLocation) {
                coords = JSON.parse(lastLocation)
                if (coords?.x == null || coords?.y == null) {
                    coords = null
                }
            }
        }

        if (!coords) {
            const unit = troops.first()
            if (unit) {
                coords = unit.region.coords
            }
        }

        return coords
    }
}

const mapContext = React.createContext<MapContext>(null)

export interface MapProviderProps {
    children: React.ReactNode
}

export function MapProvider({ children }: MapProviderProps) {
    const [ map ] = React.useState(() => new MapContext())

    return <mapContext.Provider value={map}>
        {children}
    </mapContext.Provider>
}

export function useMapContext() {
    return React.useContext(mapContext)
}
