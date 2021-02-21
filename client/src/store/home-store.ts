import { action, makeObservable, observable, runInAction, transaction } from 'mobx';
import { CLIENT } from '../client';
import { GameListItemFragment, GetGamesQuery, GetGames, NewGame, NewGameMutation, NewGameMutationVariables } from '../schema';
import { NewGameStore } from "./new-game-store";

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
                this.games.replace(response.data.games.nodes);
            });
        });
    };

    readonly newGame = new NewGameStore();

    confirmNewGame = async () => {
        const response = await CLIENT.mutate<NewGameMutation, NewGameMutationVariables>({
            mutation: NewGame,
            variables: {
                name: this.newGame.name
            }
        });

        transaction(() => {
            this.games.push(response.data.newGame);
            this.newGame.cancel();
        });
    };

    private fileUpload: HTMLInputElement = null
    @action setFileUpload = (ref: HTMLInputElement) => {
        this.fileUpload = ref
    }

    triggerUploadReport = (event) => {
        this.fileUpload?.click();
    }

    @action uploadReport = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const reports = new FormData()
        for (const f of Array.from(event.target.files)) {
            reports.append('report', f)
        }

        const gameId = 'R2FtZQpsMQ=='

        await fetch(`http://localhost:5000/report/${gameId}`, {
            method: 'POSt',
            body: reports
        })

        event.target.value = null;
    }
}
