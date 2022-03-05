import { Region } from '../game'

export interface MapState {
    zoom: number
}

export interface TileState {
    reg: Region
    isActive: boolean
    map: MapState
}
