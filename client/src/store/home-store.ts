import { action, makeObservable, observable, runInAction, transaction } from 'mobx'
import { CLIENT } from '../client'
import { GameListItemFragment, GetGamesQuery, GetGames } from '../schema'
import { CreateGame, CreateGameMutation, CreateGameMutationVariables } from '../schema'
import { JoinGame, JoinGameMutation, JoinGameMutationVariables } from '../schema'
import { NewGameStore } from './new-game-store'

export class HomeStore {
    constructor() {
        makeObservable(this)
    }

    readonly games = observable<GameListItemFragment>([]);

    load = () => {
        CLIENT.query<GetGamesQuery>({
            query: GetGames
        }).then(response => {
            runInAction(() => {
                this.games.replace(response.data.games);
            });
        });
    };

    readonly newGame = new NewGameStore()

    confirmNewGame = async () => {
        const response = await CLIENT.mutate<CreateGameMutation, CreateGameMutationVariables>({
            mutation: CreateGame,
            variables: {
                name: this.newGame.name
            }
        });

        transaction(() => {
            this.games.push(response.data.createGame);
            this.newGame.cancel();
        });
    };

    private fileUpload: HTMLInputElement = null
    @action setFileUpload = (ref: HTMLInputElement) => {
        this.fileUpload = ref
    }

    @observable uploading = false

    uploadGameId: string
    triggerUploadReport = (gameId: string) => {
        this.uploadGameId = gameId
        this.fileUpload?.click()
    }

    @action uploadReport = async (event: React.ChangeEvent<HTMLInputElement>) => {
        this.uploading = true

        const reports = new FormData()
        for (const f of Array.from(event.target.files)) {
            reports.append('report', f)
        }

        await fetch(`/report/${this.uploadGameId}`, {
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
}
