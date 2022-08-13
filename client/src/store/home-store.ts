import { action, makeObservable, observable, runInAction } from 'mobx'
import { CLIENT } from '../client'
import { GameHeaderFragment, GetGamesQuery, GetGames } from '../schema'
import { JoinGame, JoinGameMutation, JoinGameMutationVariables } from '../schema'
import { DeleteGame, DeleteGameMutation, DeleteGameMutationVariables } from '../schema'

export class HomeStore {
    constructor() {
        makeObservable(this)
    }

    readonly games = observable<GameHeaderFragment>([]);

    load = (games?: GameHeaderFragment[]) => {
        if (games) {
            runInAction(() => {
                this.games.replace(games);
            })
            return
        }

        CLIENT.query<GetGamesQuery>({
            query: GetGames
        }).then(response => {
            runInAction(() => {
                this.games.replace(response.data.games.items);
            });
        });
    };

    confirmNewGame = async () => {
        // const response = await CLIENT.mutate<CreateLocalGameMutation, CreateLocalGameMutationVariables>({
        //     mutation: CreateLocalGame,
        //     variables: {
        //         name: this.newGame.name,
        //         options: {
        //             schedule: '',
        //             map: [
        //                 { level: 0, label: 'nexus', width: 1, height: 1 },
        //                 { level: 1, label: 'surface', width: 32, height: 32 }
        //             ]
        //         },
        //         engine: this.newGame.engineFile,
        //         playerData: this.newGame.playersFile,
        //         gameData: this.newGame.gameFile
        //     }
        // });

        // runInAction(() => {
        //     this.games.push(response.data.createLocalGame);
        //     this.newGame.cancel();
        // });
    };

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

        this.load()
        runInAction(() => this.uploading = false)
    }

    joinGame = async (gameId: string) => {
        const response = await CLIENT.mutate<JoinGameMutation, JoinGameMutationVariables>({
            mutation: JoinGame,
            variables: { gameId }
        });

        this.load();
    }

    deleteGame = async (gameId: string) => {
        const response = await CLIENT.mutate<DeleteGameMutation, DeleteGameMutationVariables>({
            mutation: DeleteGame,
            variables: { gameId }
        });

        this.load(response.data.gameDelete);
    }
}
