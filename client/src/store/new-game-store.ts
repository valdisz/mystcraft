import React from 'react'
import { action, computed, makeObservable, observable, reaction } from 'mobx'
import { GameEngineFragment } from '../schema'
import { Seq } from './connection'
import { Field, FILE, INT, STRING, FieldList, FieldView, makeGroup, isCron, Converter, rule } from './forms'
import cronstrue from 'cronstrue'

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

export interface MapLevelItem extends FieldView {
    label: Field<string>
    width: Field<number>
    height: Field<number>
}

function newMapLevelItem(label: string = '', width: number = null, height: number = null): MapLevelItem {
    return makeGroup({
        label: new Field<string>(STRING(true), label, true, rule('max:128')),
        width: new Field<number>(INT, width, true, rule('min:1'), dividesWithEight),
        height: new Field<number>(INT, height, true, rule('min:1'), dividesWithEight)
    })
}

export interface NewGameForm extends FieldView {
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
        name: new Field<string>(STRING(true), '', true, rule('max:128')),
        engine: new Field<GameEngineFragment>(GAME_ENGINE, null, true),
        timeZone: new Field<string>(STRING(true), Intl.DateTimeFormat().resolvedOptions().timeZone, true, rule('max:128')),
        schedule: new Field<string>(STRING(true), '0 0 12 * * MON,TUE,FRI *', false, rule('max:128'), isCron()),
        levels: new FieldList<MapLevelItem>([], true, rule('max:4')),
        gameFile: new Field<File>(FILE, null, true),
        playersFile: new Field<File>(FILE, null, true),
    })

    private readonly _whenRemote = reaction(
        () => this.form.engine.value,
        engine => {
            if (!engine) {
                return
            }

            if (engine.remote) {
                this.form.gameFile.disable()
                this.form.playersFile.disable()
            }
            else {
                this.form.gameFile.enable()
                this.form.playersFile.enable()
            }
        }
    )

    @computed get scheduleHelpText() {
        const errorText = this.form.schedule.errorText
        const expression = this.form.schedule.value

        return errorText ?? cronstrue.toString(expression) ?? ''
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
        this.form.timeZone.reset(Intl.DateTimeFormat().resolvedOptions().timeZone)
    }

    @action cancel = () => {
        this.isOpen = false;
    }

    @action confirm = () => {
        this.form.touch()

        console.log('Form is vaid: ', this.form.valid)
    }

    @action addLevel = () => {
        this.form.levels.push(newMapLevelItem())
        this.form.levels.touch(false)
    }

    @action removeLevel = (item: MapLevelItem, index: number) => {
        this.form.levels.splice(index, 1)
    }
}
