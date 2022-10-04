import { GameDetailsStore } from './game-details-store'
import { GameEnginesStore } from './game-engines-store'
import { GameLoadingStore, GameStore } from './game-store'
import { HomeStore } from './home-store'
import { NewGameStore } from './new-game-store'
// import { UniversityStore } from './university-store'
import { StatsStore } from './stats-store'

export class MainStore {
    readonly home = new HomeStore()
    readonly gameDetails = new GameDetailsStore()
    readonly newGame = new NewGameStore(this.home)
    readonly game = new GameStore()
    readonly stats = new StatsStore(this.game)
    readonly gameEngines = new GameEnginesStore()
    readonly loading = new GameLoadingStore()
}

