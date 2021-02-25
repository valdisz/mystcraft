import { GameStore } from './game-store'
import { HomeStore } from './home-store'

export class MainStore {
    readonly home = new HomeStore()
    readonly game = new GameStore()
}

