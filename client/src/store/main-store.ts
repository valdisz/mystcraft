import { GameEnginesStore } from './game-engines-store'
import { GameStore } from './game-store'
import { HomeStore } from './home-store'
import { NewGameStore } from './new-game-store'
// import { UniversityStore } from './university-store'
import { StatsStore } from './stats-store'

export class MainStore {
    readonly home = new HomeStore()
    readonly newGame = new NewGameStore(this.home)
    readonly game = new GameStore()
    readonly stats = new StatsStore(this.game)
    readonly gameEngines = new GameEnginesStore()
}

