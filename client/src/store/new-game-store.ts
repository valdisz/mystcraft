import React from 'react'
import { action, makeObservable, observable } from 'mobx'
import { HomeStore } from './home-store'
import { GameCreate, GameCreateMutation, GameCreateMutationVariables, GameEngineFragment } from '../schema'
import { SelectChangeEvent } from '@mui/material'
import { mutate } from './connection'
import { Field, INT, STRING, maxLen, min, FieldList, FormField, makeGroup, maxCount, Converter } from './forms'

const FILE: Converter<File> = {
    sanitzie: (value: File) => ({ value }),
    format: value => value
}

function dividesWithEight(value: number): string | null {
    if (value === 1) {
        return null
    }

    if (value % 8) {
        return 'Must be dividable with 8'
    }

    return null
}

export interface MapLevelItem extends FormField {
    label: Field<string>
    width: Field<number>
    height: Field<number>
}

function newMapLevelItem(label: string = '', width: number = null, height: number = null): MapLevelItem {
    return makeGroup({
        label: new Field<string>(STRING(true), label, true, maxLen(128)),
        width: new Field<number>(INT, width, true, min(1), dividesWithEight),
        height: new Field<number>(INT, height, true, min(1), dividesWithEight)
    })
}

export interface NewGameForm extends FormField {
    name: Field<string>
    engine: Field<string>
    timeZone: Field<string>
    schedule: Field<string>
    levels: FieldList<MapLevelItem>
    gameFile: Field<File>
    playersFile: Field<File>
}

export class NewGameStore {
    constructor() {
        makeObservable(this)
    }

    readonly form = makeGroup({
        name: new Field<string>(STRING(true), '', true, maxLen(128)),
        engine: new Field<string>(STRING(), '', true),
        timeZone: new Field<string>(STRING(true), Intl.DateTimeFormat().resolvedOptions().timeZone, true, maxLen(128)),
        schedule: new Field<string>(STRING(true), '', false, maxLen(128)),
        levels: new FieldList<MapLevelItem>([], true, maxCount(4)),
        gameFile: new Field<File>(FILE, null, true),
        playersFile: new Field<File>(FILE, null, true),
    })


    @observable isOpen = false
    @observable remote = false

    readonly timeZones = Intl.supportedValuesOf('timeZone')

    @action open = () => {
        this.isOpen = true

        this.form.reset()
        this.form.levels.push(newMapLevelItem('nexus', 1, 1))
    }

    @action remoteChange = (ev: React.ChangeEvent<HTMLInputElement>, checked: boolean) => {
        this.remote = checked
    }

    @action cancel = () => {
        this.isOpen = false;
    }

    @action confirm = () => {
        this.form.validate()
        this.form.touch()

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
            // const variables: GameCreateMutationVariables = {
            //     name: this.form.name.value,
            //     // options: JSON.parse(this.options),
            //     gameEngineId: this.engine,
            //     map: [],
            //     schedule: '',
            //     timeZone: ''
            //     // playerData: this.playersFile,
            //     // gameData: this.gameFile
            // }

            // FIXME
            // mutate(GameCreate, variables, {
            //     refetch: [ this.store.games ]
            // })
            // .then(this.cancel, console.error)
        }
    }

    @action addLevel = () => {
        this.form.levels.push(newMapLevelItem())
        this.form.levels.touch(false)
    }

    @action removeLevel = (item: MapLevelItem, index: number) => {
        this.form.levels.splice(index, 1)
    }
}
