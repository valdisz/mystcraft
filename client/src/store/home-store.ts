import { action, makeObservable, observable, runInAction } from 'mobx'
import { mutate, querySeq } from './connection'
import { GameHeaderFragment, GetGamesQuery, GetGames } from '../schema'
import { DeleteGame, DeleteGameMutation, DeleteGameMutationVariables } from '../schema'

export class HomeStore {
    constructor() {
        makeObservable(this)
    }

    readonly games = querySeq<GetGamesQuery, GameHeaderFragment>(GetGames, data => data.games?.items || [])

    private fileUpload: HTMLInputElement = null
    @action setFileUpload = (ref: HTMLInputElement) => {
        this.fileUpload = ref
    }

    @observable uploading = false

    uploadPlayerId: string
    uploadAction: 'report' | 'map' | 'ruleset' = 'report'

    triggerUploadReport = (playerId: string) => {
        this.uploadPlayerId = playerId
        this.uploadAction = 'report'

        this.fileUpload?.click()
    }

    triggerImportMap = (playerId: string) => {
        this.uploadPlayerId = playerId
        this.uploadAction = 'map'

        this.fileUpload?.click()
    }

    triggerRuleset = (gameId: string) => {
        this.uploadPlayerId = gameId
        this.uploadAction = 'ruleset'

        this.fileUpload?.click()
    }

    @action uploadFile = async (event: React.ChangeEvent<HTMLInputElement>) => {
        this.uploading = true

        const reports = new FormData()
        for (const f of Array.from(event.target.files)) {
            reports.append('report', f)
        }

        await fetch(`/api/${this.uploadPlayerId}/${this.uploadAction}`, {
            method: 'POSt',
            body: reports,
            credentials: 'include'
        })
        event.target.value = null;

        this.games.reload()
        runInAction(() => this.uploading = false)
    }

    deleteGame = (gameId: string) => mutate<DeleteGameMutation, DeleteGameMutationVariables>(
        DeleteGame,
        { gameId },
        { refetch: [
            this.games
        ]}
    )
}
