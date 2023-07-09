import React from 'react'
import { IObservableArray, action, computed, makeObservable, observable } from 'mobx'
import { HomeStore } from './home-store'
import { GameCreate, GameCreateMutation, GameCreateMutationVariables } from '../schema'
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

function numberOrDefault(value: string | null | undefined, defaultValue: number) {
    if (value === null || value === undefined) {
        return defaultValue
    }

    if (!value) {
        return ''
    }

    const n = Number(value)
    if (isNaN(n)) {
        return defaultValue
    }

    return n
}

export class MapLevelItem {
    constructor(label: string = '', width: number = null, height: number = null) {
        this.label = label
        this.width = width
        this.height = height

        makeObservable(this)
    }

    @observable label: string = ''
    @observable width: number | '' = ''
    @observable height: number | '' = ''

    @action setLabel = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.label = e.target.value
    }

    @action setWidth = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.width = numberOrDefault(e.target.value, this.width || 0)
    }

    @action setHeight = (e: React.ChangeEvent<HTMLInputElement>) => {
        this.height = numberOrDefault(e.target.value, this.height || 0)
    }
}

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

    readonly mapLevels: IObservableArray<MapLevelItem> = observable<MapLevelItem>([])

    @action open = () => {
        this.isOpen = true

        this.gameFile = null
        this.playersFile = null
        this.name = ''
        this.gameFileName = ''
        this.playersFileName = ''
        this.engine = ''
        this.options = String(DEFAULT_OPTIONS)

        this.mapLevels.clear()
        this.mapLevels.push(new MapLevelItem('nexus', 1, 1))
    }

    @action remoteChange = (ev: React.ChangeEvent<HTMLInputElement>, checked: boolean) => {
        this.remote = checked
    }

    @action cancel = () => {
        this.isOpen = false;
    }

    confirm = () => {
        if (this.remote) {
            // FIXME
            // const variables: GameCreateRemoteMutationVariables = {
            //     name: this.name,
            //     options: JSON.parse(this.options)
            // }

            // mutate(GameCreateRemote, variables, {
            //     refetch: [ this.store.games ]
            // })
            // .then(this.cancel, console.error)
        }
        else {
            const variables: GameCreateMutationVariables = {
                name: this.name,
                // options: JSON.parse(this.options),
                gameEngineId: this.engine,
                map: [],
                schedule: '',
                timeZone: ''
                // playerData: this.playersFile,
                // gameData: this.gameFile
            }

            mutate(GameCreate, variables, {
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

    @action addLevel = () => {
        this.mapLevels.push(new MapLevelItem())
    }

    @computed get canRemoveLevel() {
        return this.mapLevels.length > 1
    }

    @action removeMapLevel = (index: number) => {
        if (this.canRemoveLevel) {
            this.mapLevels.splice(index, 1)
        }
    }
}
