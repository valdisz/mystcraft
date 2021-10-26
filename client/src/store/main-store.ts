import { GameStore } from './game-store'
import { HomeStore } from './home-store'
import { UniversityStore } from './university-store'
import { StatsStore } from './stats-store'

export class MainStore {
    readonly home = new HomeStore()
    readonly game = new GameStore()
    // readonly university = new UniversityStore()
    readonly stats = new StatsStore(this.game)
}

