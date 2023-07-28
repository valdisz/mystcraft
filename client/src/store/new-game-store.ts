import { action, computed, makeObservable } from 'mobx'
import { GameCreateRemoteMutationVariables, GameCreateLocalMutationVariables, GameEngineFragment } from '../schema'
import { Operation, Runnable, Seq } from './connection'
import { text, file, int, group, list, custom, rule } from './forms'
import cronstrue from 'cronstrue'
import { DialogViewModel, newDialog } from './dialog'

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
        width: int([dividesWithEight], { initialValue: width, isRequired: true }),
        height: int([dividesWithEight], { initialValue: height, isRequired: true }),
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
        levels: list<MapLevelItem>(null, { isRequired: true }),
        files: group(
            {
                gameIn: file(null, { isRequired: true }),
                playersIn: file(null, { isRequired: true }),
            },
            {
                isDisabled: () => engine.value?.remote ?? true
            }
        )
    })
}

export class NewGameStore {
    constructor(
        public readonly engines: Seq<GameEngineFragment>,
        public readonly operation: Operation,
        private readonly addLocal: Runnable<GameCreateLocalMutationVariables>,
        private readonly addRemote: Runnable<GameCreateRemoteMutationVariables>,
    ) {
        this.dialog = newDialog({
            onOpen: () => {
                this.operation.reset()
                this.form.reset()
                this.form.levels.reset(newMapLevelItem('nexus', 1, 1))
                this.form.schedule.reset('0 12 * * MON,WED,FRI')
                this.form.timeZone.reset(Intl.DateTimeFormat().resolvedOptions().timeZone)
            },
            operation: this.operation
        })

        makeObservable(this)
    }

    readonly dialog: DialogViewModel
    readonly form = newForm()

    @computed get scheduleHelpText() {
        const errorText = this.form.schedule.error
        const expression = this.form.schedule.isNotEmpty
            ? cronstrue.toString(this.form.schedule.value)
            : ''

        return errorText || expression || ''
    }

    readonly timeZones = Intl.supportedValuesOf('timeZone')

    @action addLevel = () => {
        this.form.levels.push(newMapLevelItem())
        this.form.levels.touch(false)
    }

    @action removeLevel = (item: MapLevelItem, index: number) => {
        this.form.levels.splice(index, 1)
    }

    @action confirm = async () => {
        this.form.touch()
        if (this.form.isInvalid) {
            return
        }

        const { engine, files, levels, ...variables } = this.form.value
        const vars = { gameEngineId: engine.id, levels: levels.map((l, i) => ({ level: i, ...l })), ...variables }

        if (engine.remote) {
            await this.addRemote.run(vars)
        }
        else {
            const vars = { gameEngineId: engine.id, levels: levels.map((l, i) => ({ level: i, ...l})), ...variables }
            await this.addLocal.run({ ...files, ...vars })
        }
    }
}
