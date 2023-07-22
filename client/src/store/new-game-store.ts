import React from 'react'
import { action, computed, makeObservable, observable } from 'mobx'
import { HomeStore } from './home-store'
import { GameCreate, GameCreateMutation, GameCreateMutationVariables, GameEngineFragment } from '../schema'
import { SelectChangeEvent } from '@mui/material'
import { Seq, mutate } from './connection'
import { Field, INT, STRING, maxLen, min, FieldList, FormField, makeGroup, maxCount, Converter } from './forms'
import cronstrue from 'cronstrue'

const FILE: Converter<File> = {
    sanitzie: (value: File) => ({ value }),
    format: value => value
}

const GAME_ENGINE: Converter<GameEngineFragment> = {
    sanitzie: (value: GameEngineFragment) => ({ value: value ? value : null }),
    format: value => value ? value : ''
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

function cronExpression(value: string): string | null {
    if (!value) {
        return null
    }

    try {
        cronstrue.toString(value)
    }
    catch (e) {
        if (typeof e === 'string') {
            return e
        }

        return (e as Error)?.message || 'Invalid cron expression'
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
    engine: Field<GameEngineFragment>
    timeZone: Field<string>
    schedule: Field<string>
    levels: FieldList<MapLevelItem>
    gameFile: Field<File>
    playersFile: Field<File>
}

export class NewGameStore {
    constructor(public readonly engines: Seq<GameEngineFragment>) {
        makeObservable(this)
    }

    readonly form = makeGroup({
        name: new Field<string>(STRING(true), '', true, maxLen(128)),
        engine: new Field<GameEngineFragment>(GAME_ENGINE, null, true),
        timeZone: new Field<string>(STRING(true), Intl.DateTimeFormat().resolvedOptions().timeZone, true, maxLen(128)),
        schedule: new Field<string>(STRING(true), '0 0 12 * * MON,TUE,FRI *', false, maxLen(128), cronExpression),
        levels: new FieldList<MapLevelItem>([], true, maxCount(4)),
        gameFile: new Field<File>(FILE, null, true),
        playersFile: new Field<File>(FILE, null, true),
    })

    @computed get scheduleHelpText() {
        const errorText = this.form.schedule.errorText
        const expression = this.form.schedule.value

        if (errorText) {
            return errorText
        }

        if (expression) {
            return cronstrue.toString(this.form.schedule.value)
        }

        return ''
    }

    @computed get requireUpload(): boolean {
        return !(this.form.engine.value?.remote ?? true)
    }

    @observable isOpen = false

    readonly timeZones = Intl.supportedValuesOf('timeZone')

    @action open = () => {
        this.isOpen = true

        this.form.reset()
        this.form.levels.reset(newMapLevelItem('nexus', 1, 1))
        this.form.schedule.reset('0 12 * * MON,WED,FRI')
    }

    @action cancel = () => {
        this.isOpen = false;
    }

    @action confirm = () => {
        // this.form.validate()
        this.form.touch()

        // if (this.remote) {
            // FIXME
            // const variables: GameCreateRemoteMutationVariables = {
            //     name: this.name,
            //     options: JSON.parse(this.options)
            // }

            // mutate(GameCreateRemote, variables, {
            //     refetch: [ this.store.games ]
            // })
            // .then(this.cancel, console.error)
        // }
        // else {
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
        // }
    }

    @action addLevel = () => {
        this.form.levels.push(newMapLevelItem())
        this.form.levels.touch(false)
    }

    @action removeLevel = (item: MapLevelItem, index: number) => {
        this.form.levels.splice(index, 1)
    }
}
