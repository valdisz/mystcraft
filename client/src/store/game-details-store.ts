import { action, computed, makeObservable, observable, runInAction } from 'mobx'
import { mutate, query } from './connection'
import { GetGameDetails, GetGameDetailsQuery, GetGameDetailsQueryVariables, JobState } from '../schema'
import { FactionClaim, FactionClaimMutation, FactionClaimMutationVariables } from '../schema'
import { GameStart, GameStartMutation, GameStartMutationVariables } from '../schema'
import { GetJob, GetJobQuery, GetJobQueryVariables } from '../schema'
import { GameStatus } from '../schema'
import React from 'react'
import { identity } from 'lodash'

export interface Game {
    id: string
    name: string
    status: GameStatus
    lastTurnNumber: number
    joined: boolean
    players: Player[]
}

export interface TurnState {
    turnNumber: number
    orders: boolean
    times: boolean
}

export interface Player {
    id?: string
    number: number
    name: string
    isClaimed: boolean
    isOwn?: boolean
    stance: 'own' | 'ally' | 'friendly' | 'neutral' | 'unfriendly' | 'hostile'
    turns: TurnState[]
}

function delay(time: number) {
    return new Promise((resolve) => setTimeout(resolve, time))
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

    get game() {
        return this.source.value
    }

    get name() {
        return this.source.value?.name
    }

    get status() {
        return this.source.value?.status
    }

    get players() {
        return this.source.value?.players || []
    }

    @computed get ownPlayers() {
        return this.players.filter(x => x.isOwn)
    }

    @computed get showOwnPlayers() {
        return this.ownPlayers.length > 0
    }

    @computed get claimedPlayers() {
        return this.players.filter(x => !x.isOwn && x.isClaimed)
    }

    @computed get showClaimedPlayers() {
        return this.claimedPlayers.length > 0
    }

    @computed get remotePlayers() {
        return this.players.filter(x => !x.isClaimed)
    }

    @computed get showRemotePlayers() {
        return this.remotePlayers.length > 0
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
        return !(this.source.value?.joined || false) && this.status === GameStatus.Running
    }

    get turnNumber() {
        return this.source.value?.lastTurnNumber
    }

    @computed get playerCount() {
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

    readonly start = async () => {
        // FIXME
        // const result = await mutate<GameStartMutation, GameStartMutationVariables>(GameStart, { gameId: this.gameId })

        // const { jobId } = result.data?.gameStart

        // let state: JobState
        // const ds = query<GetJobQuery, GetJobQuery, GetJobQueryVariables>(GetJob, identity, { jobId })
        // do {
        //     await delay(1000)
        //     await ds.reload()
        //     state = ds.value.job.state
        // }
        // while (state !== JobState.Succeeded && state !== JobState.Failed)

        // ds.close()

        // this.source.reload()
    }

    private mapReponse({ node }: GetGameDetailsQuery): Game | null {
        if (node.__typename !== 'Game') {
            return null
        }

        let joined = false
        const players: Player[] = []
        for (const { id, name, number, turns, isClaimed } of node.players.items) {
            const isOwn = id === node.me?.id
            joined = joined || isOwn
            players.push({
                id,
                name,
                number,
                isClaimed,
                isOwn,
                // FIXME: dummy value, replace with real value
                stance: 'neutral',
                turns: turns.items.map(x => ({
                    turnNumber: x.turnNumber,
                    orders: x.isOrdersSubmitted,
                    times: x.isTimesSubmitted
                } as TurnState))
            })
        }

        players.sort((a, b) => a.number - b.number)

        return {
            id: node.id,
            name: node.name,
            lastTurnNumber: node.lastTurnNumber,
            status: node.status,
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

        // FIXME
        // let error = ''
        // try {
        //     const result = await mutate<FactionClaimMutation, FactionClaimMutationVariables>(FactionClaim, {
        //         gameId: this.gameId,
        //         playerId: this.playerId,
        //         password: this.password
        //     }, {
        //         refetch: [ this.gameDetails.source ]
        //     })

        //     if (result.error) {
        //         error = result.error.message
        //         return
        //     }
        //     else if (!result.data?.gameJoinRemote?.isSuccess) {
        //         error = result.data?.gameJoinRemote?.error
        //     }
        // }
        // finally {
        //     runInAction(() => {
        //         this.isLoading = false
        //         this.isOpen = !!error
        //         this.error = error
        //     })
        // }
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
