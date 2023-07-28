import { seq, mutate, combine } from './connection'

import { GetGameEngines, GetGameEnginesQuery, GameEngineFragment, MutationResult } from '../schema'
import { GetGames, GetGamesQuery, GameHeaderFragment } from '../schema'

import { GameEngineCreateLocal, GameEngineCreateLocalMutation, GameEngineCreateLocalMutationVariables } from '../schema'
import { GameEngineCreateRemote, GameEngineCreateRemoteMutation, GameEngineCreateRemoteMutationVariables } from '../schema'
import { GameEngineDelete, GameEngineDeleteMutation, GameEngineDeleteMutationVariables, GameEngineDeleteResult } from '../schema'
import { GameCreateRemote, GameCreateRemoteMutation, GameCreateRemoteMutationVariables } from '../schema'
import { GameCreateLocal, GameCreateLocalMutation, GameCreateLocalMutationVariables } from '../schema'
import { GameDelete, GameDeleteMutation, GameDeleteMutationVariables, GameDeleteResult } from '../schema'


import { GameDetailsStore } from './game-details-store'
import { NewGameEngineViewModel } from './game-engines-store'
import { GameLoadingStore, GameStore } from './game-store'
import { HomeStore } from './home-store'
import { NewGameStore } from './new-game-store'
// import { UniversityStore } from './university-store'
import { StatsStore } from './stats-store'

export type Result<K extends keyof any, T> = {
    [P in K]?: T
}  & MutationResult

export class MainStore {
    readonly engines = seq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [], null, 'engines')

    readonly opGameEngineAddLocal = mutate<GameEngineCreateLocalMutation, GameEngineCreateLocalMutationVariables, Result<'engine', GameEngineFragment>, GameEngineFragment>({
        name: 'opGameEngineAddLocal',
        document: GameEngineCreateLocal,
        pick: data => data.gameEngineCreateLocal,
        map: data => data.engine,
        onSuccess: [
            engine => {
                this.engines.insert(0, engine)
                this.enginesNew.dialog.close()
            }
        ]
    })

    readonly opGameEngineAddRemote = mutate<GameEngineCreateRemoteMutation, GameEngineCreateRemoteMutationVariables, Result<'engine', GameEngineFragment>, GameEngineFragment>({
        name: 'opGameEngineAddRemote',
        document: GameEngineCreateRemote,
        pick: data => data.gameEngineCreateRemote,
        map: data => data.engine,
        onSuccess: [
            engine => {
                this.engines.insert(0, engine)
                this.enginesNew.dialog.close()
            }
        ]
    })

    readonly opGameEngineDelete = mutate<GameEngineDeleteMutation, GameEngineDeleteMutationVariables, GameEngineDeleteResult, void>({
        name: 'opGameEngineDelete',
        document: GameEngineDelete,
        pick: data => data.gameEngineDelete,
        onSuccess: [
            (_, variables) => {
                this.engines.remove(e => e.id === variables.gameEngineId)
            }
        ]
    })

    readonly enginesDelete = (gameEngineId: string) => this.opGameEngineDelete.run({ gameEngineId })

    readonly games = seq<GetGamesQuery, GameHeaderFragment>(GetGames, data => data.games?.items || [], null, 'games')

    readonly opGameAddRemote = mutate<GameCreateRemoteMutation, GameCreateRemoteMutationVariables, Result<'game', GameHeaderFragment>, GameHeaderFragment>({
        name: 'opGameAddRemote',
        document: GameCreateRemote,
        pick: data => data.gameCreateRemote,
        map: data => data.game,
        onSuccess: [
            game => {
                this.games.insert(0, game)
                this.gamesNew.dialog.close()
            }
        ]
    })

    readonly opGameAddLocal = mutate<GameCreateLocalMutation, GameCreateLocalMutationVariables, Result<'game', GameHeaderFragment>, GameHeaderFragment>({
        name: 'opGameAddLocal',
        document: GameCreateLocal,
        pick: data => data.gameCreateLocal,
        map: data => data.game,
        onSuccess: [
            game => {
                this.games.insert(0, game)
                this.gamesNew.dialog.close()
            }
        ]
    })

    readonly opGameDelete = mutate<GameDeleteMutation, GameDeleteMutationVariables, GameDeleteResult, void>({
        name: 'opGameDelete',
        document: GameDelete,
        pick: data => data.gameDelete,
        onSuccess: [
            (_, variables) => {
                this.games.remove(e => e.id === variables.gameId)
            }
        ]
    })

    readonly gamesDelete = (gameId: string) => this.opGameDelete.run({ gameId })

    readonly homePageOperation = combine(this.games, this.opGameDelete)
    readonly gameEnginesPageOperation = combine(this.engines, this.opGameEngineDelete)

    readonly home = new HomeStore()
    readonly enginesNew = new NewGameEngineViewModel(
        combine(this.opGameEngineAddLocal, this.opGameEngineAddRemote),
        this.opGameEngineAddLocal,
        this.opGameEngineAddRemote
    )
    readonly gamesNew = new NewGameStore(
        this.engines,
        combine(this.opGameAddLocal, this.opGameAddRemote),
        this.opGameAddLocal,
        this.opGameAddRemote
    )
    readonly gameDetails = new GameDetailsStore()
    readonly game = new GameStore()
    readonly stats = new StatsStore(this.game)
    readonly loading = new GameLoadingStore()
}

