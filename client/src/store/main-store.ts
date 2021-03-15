import { GameStore } from './game-store'
import { HomeStore } from './home-store'
import { UniversityStore } from './university-store'

export class MainStore {
    readonly home = new HomeStore()
    readonly game = new GameStore()
    readonly university = new UniversityStore()
}

