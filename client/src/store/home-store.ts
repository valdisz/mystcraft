import { observable, runInAction, transaction } from 'mobx';
import { CLIENT } from '../client';
import { GameListItemFragment, GetGamesQuery, GetGames, NewGame, NewGameMutation, NewGameMutationVariables } from '../schema';
import { NewGameStore } from "./new-game-store";

export class HomeStore {
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
}
