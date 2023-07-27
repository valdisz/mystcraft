import { action, computed, makeObservable, observable } from 'mobx'
import { GameEngineFragment } from '../schema'
import { Seq } from './connection'
import { text, file, int, group, list, custom, rule } from './forms'
import cronstrue from 'cronstrue'

function dividesWithEight(value: number): string | null {
    if (value === 1) {
        return null
    }

    if (value % 8) {
        return 'Must be dividable with 8'
    }

    return null
}

function newMapLevelItem(label: string = '', width: number = null, height: number = null) {
    return group({
        label: text(rule('max:128'), { trim: true, initialValue: label, isRequired: true }),
        width: int([rule('min:1'), dividesWithEight], { initialValue: width, isRequired: true }),
        height: int([rule('min:1'), dividesWithEight], { initialValue: height, isRequired: true }),
    })
}

export type MapLevelItem = ReturnType<typeof newMapLevelItem>

function newForm() {
    const engine = custom<GameEngineFragment>(
        {
            sanitize: value => ({ value: value ? value : null }),
            format: value => value
        },
        null,
        { isRequired: true }
    )

    const schedule = text(rule('max:128'), { trim: true })

    return group({
        name: text(rule('max:128'), { isRequired: true, trim: true }),
        engine,
        schedule,
        timeZone: text(rule('max:128'), {
            isRequired: () => schedule.isNotEmpty,
            trim: true
        }),
        levels: list<MapLevelItem>(rule('min:1'), { isRequired: true }),
        files: group(
            {
                game: file(null, { isRequired: true }),
                players: file(null, { isRequired: true }),
            },
            {
                isDisabled: () => engine.value?.remote ?? true
            }
        )
    })
}

export class NewGameStore {
    constructor(public readonly engines: Seq<GameEngineFragment>) {
        makeObservable(this)
    }

    readonly form = newForm()

    @computed get scheduleHelpText() {
        const errorText = this.form.schedule.error
        const expression = this.form.schedule.isNotEmpty
            ? cronstrue.toString(this.form.schedule.value)
            : ''

        return errorText || expression || ''
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

        console.log('Form is vaid: ', this.form.isValid)
        console.log('Form: ', this.form.value)
    }

    @action addLevel = () => {
        this.form.levels.push(newMapLevelItem())
        this.form.levels.touch(false)
    }

    @action removeLevel = (item: MapLevelItem, index: number) => {
        this.form.levels.splice(index, 1)
    }
}
