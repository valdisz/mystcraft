import { action, makeObservable, observable, runInAction } from 'mobx'
import { mutate, query } from './connection'
import { GetGameDetails, GetGameDetailsQuery, GetGameDetailsQueryVariables } from '../schema'
import { FactionClaim, FactionClaimMutation, FactionClaimMutationVariables } from '../schema'
import React from 'react'

export interface Game {
    id: string
    name: string
    lastTurnNumber: number
    joined: boolean
    players: Player[]
}

export interface Player {
    id?: string
    number: number
    name: string
    orders: boolean
    times: boolean
    isClaimed: boolean
    isOwn: boolean
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

    readonly claimFaction = new ClaimFactionStore(this)

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

    get canPlay() {
        return this.isReady && this.source.value?.joined
    }

    get canJoin() {
        return !(this.source.value?.joined || false)
    }

    get turnNumber() {
        return this.source.value?.lastTurnNumber
    }

    get playerCount() {
        return this.players.length
    }

    get locaPlayerCount() {
        let count = 0
        for (const player of this.players) {
            if (player.isClaimed) {
                count++
            }
        }

        return count
    }

    @action setGameId = (id: string) => {
        this.gameId = id
    }

    readonly claim = (player: Player) => {
        if (!player.isClaimed && this.canJoin) {
            this.claimFaction.open(this.gameId, player.id, player.name, player.number)
        }
    }

    private mapReponse({ node }: GetGameDetailsQuery): Game | null {
        if (node.__typename !== 'Game') {
            return null
        }

        let joined = false
        const players: Player[] = []
        for (const { id, name, number, nextTurn, isClaimed } of node.players.items) {
            const isOwn = id === node.me?.id
            joined = joined || isOwn
            players.push({
                id,
                name,
                number,
                isClaimed,
                isOwn,
                orders: nextTurn?.isOrdersSubmitted,
                times: nextTurn?.isTimesSubmitted
            })
        }

        players.sort((a, b) => a.number - b.number)

        return {
            id: node.id,
            name: node.name,
            lastTurnNumber: node.lastTurnNumber,
            joined,
            players
        }
    }
}

export class ClaimFactionStore {
    constructor(private readonly gameDetails: GameDetailsStore) {
        makeObservable(this)
    }

    private gameId: string
    private playerId: string

    @observable isLoading = false
    @observable isOpen = false
    @observable error = ''

    @observable factionName = ''
    @observable factionNumber = 0
    @observable password = ''

    @action open = (gameId: string, playerId: string, factionName: string, factionNumber: number) => {
        this.gameId = gameId
        this.playerId = playerId

        this.isOpen = true;
        this.error = ''
        this.factionName = factionName
        this.factionNumber = factionNumber
    }

    readonly claim = async () => {
        runInAction(() => this.isLoading = true)

        let error = ''
        try {
            const result = await mutate<FactionClaimMutation, FactionClaimMutationVariables>(FactionClaim, {
                gameId: this.gameId,
                playerId: this.playerId,
                password: this.password
            }, {
                refetch: [ this.gameDetails.source ]
            })

            if (result.error) {
                error = result.error.message
                return
            }
            else if (!result.data?.gameJoinRemote?.isSuccess) {
                error = result.data?.gameJoinRemote?.error
            }
        }
        finally {
            runInAction(() => {
                this.isLoading = false
                this.isOpen = !!error
                this.error = error
            })
        }
    }

    @action close = () => {
        if (this.isLoading) {
            return
        }

        this.isOpen = false
    }

    @action onPasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.password = e.target.value
    }
}
