import React from 'react'
import { action, makeObservable, observable, runInAction } from 'mobx'
import { HomeStore } from './home-store'
import { CLIENT } from '../client'
import { GameCreateLocal, GameCreateLocalMutation, GameCreateLocalMutationVariables } from '../schema'
import { GameCreateRemote, GameCreateRemoteMutation, GameCreateRemoteMutationVariables } from '../schema'
import { SelectChangeEvent } from '@mui/material'
import { mutate } from './connection'

const DEFAULT_OPTIONS = `{
    "map": [
        {
            "level": 0,
            "label": "nexus",
            "width": 1,
            "height": 1
        },
        {
            "level": 1,
            "label": "surface",
            "width": 64,
            "height": 64
        }
    ],
    "schedule": "0 12 * * *",
    "timeZone": "Europe/Riga",
    "serverAddress": "http://atlantis-pbem.com"
}`

export class NewGameStore {
    constructor(private readonly store: HomeStore) {
        makeObservable(this)
    }

    gameFile: File | null = null
    playersFile: File | null = null

    @observable isOpen = false
    @observable remote = false
    @observable name = ''
    @observable gameFileName = ''
    @observable playersFileName = ''
    @observable engine = ''
    @observable options = String(DEFAULT_OPTIONS)

    @action open = () => {
        this.isOpen = true

        this.gameFile = null
        this.playersFile = null
        this.name = ''
        this.gameFileName = ''
        this.playersFileName = ''
        this.engine = ''
        this.options = String(DEFAULT_OPTIONS)
    }

    @action remoteChange = (ev: React.ChangeEvent<HTMLInputElement>, checked: boolean) => {
        this.remote = checked
    }

    @action cancel = () => {
        this.isOpen = false;
    }

    confirm = () => {
        if (this.remote) {
            const variables: GameCreateRemoteMutationVariables = {
                name: this.name,
                options: JSON.parse(this.options)
            }

            mutate(GameCreateRemote, variables, {
                refetch: [ this.store.games ]
            })
            .then(this.cancel, console.error)
        }
        else {
            const variables: GameCreateLocalMutationVariables = {
                name: this.name,
                options: JSON.parse(this.options),
                gameEngineId: this.engine,
                playerData: this.playersFile,
                gameData: this.gameFile
            }

            mutate(GameCreateLocal, variables, {
                refetch: [ this.store.games ]
            })
            .then(this.cancel, console.error)
        }
    }

    @action setName = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.name = e.target.value;
    }

    @action setFile = (type: 'players' | 'game', f: File) => {
        switch (type) {
            case 'players':
                this.playersFile = f
                this.playersFileName = f.name
                break

            case 'game':
                this.gameFile = f
                this.gameFileName = f.name
                break
        }
    }

    @action setOptions = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.options = e.target.value;
    }

    @action setEngine = (e: SelectChangeEvent) => {
        this.engine = e.target.value
    }
}
