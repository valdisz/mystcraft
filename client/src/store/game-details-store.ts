import { action, makeObservable, observable } from 'mobx'
import { query } from './connection'
import { GetGameDetails, GetGameDetailsQuery, GetGameDetailsQueryVariables } from '../schema'

export interface Game {
    id: string
    name: string
    players: Player[]
}

export interface Player {
    id?: string
    number: number
    name: string
    orders: boolean
    times: boolean
}

export class GameDetailsStore {
    constructor() {
        makeObservable(this)
    }

    @observable gameId: string = null

    readonly source = query<GetGameDetailsQuery, Game | null, GetGameDetailsQueryVariables>(
        GetGameDetails,
        res => this.mapReponse(res),
        () => ({ gameId: this.gameId }),
        'Game Details'
    )

    get name() {
        return this.source.value?.name
    }

    get players() {
        return this.source.value?.players || []
    }

    get isLoading() {
        return this.source.isLoading
    }

    get isReady() {
        return this.source.isReady
    }

    get error() {
        return this.source.error
    }

    @action setGameId = (id: string) => {
        this.gameId = id
    }

    private mapReponse({ node }: GetGameDetailsQuery): Game | null {
        if (node.__typename !== 'Game') {
            return null
        }

        const players: Player[] = []

        for (const { id, name, number } of node.players.items) {
            players.push({ id, name, number, orders: false, times: false })
        }

        for (const { name, number, orders, times } of node.remotePlayers) {
            const p = players.find(x => x.number === number)
            if (p) {
                p.orders = orders
                p.times = times
            }
            else {
                players.push({ name, number, orders, times })
            }
        }

        players.sort((a, b) => a.number - b.number)

        return {
            id: node.id,
            name: node.name,
            players
        }
    }
}
