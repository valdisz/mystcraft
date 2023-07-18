import { runInAction } from 'mobx'

import { seq, mutate } from './connection'

import { GetGameEngines, GetGameEnginesQuery, GameEngineFragment } from '../schema'
import { GetGames, GetGamesQuery, GameHeaderFragment } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables, GameEngineCreateResult } from '../schema'
import { GameEngineDelete, GameEngineDeleteMutation, GameEngineDeleteMutationVariables, GameEngineDeleteResult } from '../schema'

import { GameDetailsStore } from './game-details-store'
import { NewGameEngineViewModel } from './game-engines-store'
import { GameLoadingStore, GameStore } from './game-store'
import { HomeStore } from './home-store'
import { NewGameStore } from './new-game-store'
// import { UniversityStore } from './university-store'
import { StatsStore } from './stats-store'

export class MainStore {
    readonly engines = seq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [], null, 'engines')

    readonly opGameEngineAdd = mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables, GameEngineCreateResult, GameEngineFragment>({
        name: 'opAddEngine',
        document: GameEngineCreate,
        pick: data => data.gameEngineCreate,
        map: data => data.engine,
        onSuccess: [
            engine => runInAction(() => {
                this.engines.insert(0, engine)
                this.enginesNew.close()
            })
        ]
    })

    readonly opGameEngineDelete = mutate<GameEngineDeleteMutation, GameEngineDeleteMutationVariables, GameEngineDeleteResult, void>({
        name: 'opDeleteEngine',
        document: GameEngineDelete,
        pick: data => data.gameEngineDelete,
        onSuccess: [
            (_, variables) => {
                this.engines.remove(e => e.id === variables.gameEngineId)
            }
        ]
    })

    readonly enginesAdd = (name: string, content: File, ruleset: File) => this.opGameEngineAdd.run({ name, content, ruleset })
    readonly enginesDelete = (gameEngineId: string) => this.opGameEngineDelete.run({ gameEngineId })

    readonly enginesNew = new NewGameEngineViewModel(this.enginesAdd)


    readonly games = seq<GetGamesQuery, GameHeaderFragment>(GetGames, data => data.games?.items || [], null, 'games')

    readonly home = new HomeStore()
    readonly gameDetails = new GameDetailsStore()
    readonly newGame = new NewGameStore(this.home)
    readonly game = new GameStore()
    readonly stats = new StatsStore(this.game)
    readonly loading = new GameLoadingStore()
}

