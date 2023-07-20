import React from 'react'
import { IObservableArray, action, computed, makeObservable, observable } from 'mobx'
import { HomeStore } from './home-store'
import { GameCreate, GameCreateMutation, GameCreateMutationVariables } from '../schema'
import { FormControlProps, FormHelperTextProps, InputLabelProps, SelectChangeEvent, SelectProps, TextFieldProps } from '@mui/material'
import { mutate } from './connection'
import { FileViewModel } from '../components'

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

export interface ValueOrError<T> {
    error?: string
    value?: T
}

export interface Converter<T> {
    sanitzie(value: string): ValueOrError<T>
    format(value: T | null): string
}

export interface Validator<T> {
    (value: T): string | null
}

const STRING: Converter<string> = {
    sanitzie: value => ({ value }),
    format: value => value || ''
}

const STRING_TRIM: Converter<string> = {
    sanitzie: value => ({ value: value?.trim() || '' }),
    format: value => value || ''
}

const INT: Converter<number> = {
    sanitzie: s => {
        if (!s) {
            return { value: null }
        }

        const value = parseInt(s)
        if (isNaN(value)) {
            return { error: 'Value is not a whole number' }
        }

        if (!isFinite(value)) {
            return { error: 'Value is out of bounds' }
        }

        return { value }
    },
    format: value => value?.toString() || ''
}

const REAL: Converter<number> = {
    sanitzie: s => {
        if (!s) {
            return { value: null }
        }

        const value = parseFloat(s)
        if (isNaN(value)) {
            return { error: 'Value is not a fraction or whole number' }
        }

        if (!isFinite(value)) {
            return { error: 'Value is out of bounds' }
        }

        return { value }
    },
    format: value => value?.toString() || ''
}

function minLen(len: number, error?: string): Validator<string> {
    return (value: string) => !value || value.length < len ? error || `Must be at least ${len} symbols` : null
}

function maxLen(len: number, error?: string): Validator<string> {
    return (value: string) => !value || value.length > len ? error || `Must be no more tha ${len} symbols` : null
}

function min(minValue: number, error?: string): Validator<number> {
    return (value: number) => value < minValue ? error || `Must be greater or equal to ${minValue}` : null
}

function max(maxValue: number, error?: string): Validator<number> {
    return (value: number) => value > maxValue ? error || `Must be less or equal to ${maxValue}` : null
}

function required<T>(error?: string): Validator<T> {
    return (value: T) => {
        if (value === null || value === undefined || value === '') {
            return error || 'Required'
        }

        return null
    }
}

function noValidate<T>(value: T) {
    return null
}

function combine<T>(...validators: Validator<T>[]): Validator<T> {
    return (value: T) => {
        for (var v of validators) {
            const error = v(value)
            if (error) {
                return error
            }
        }

        return null
    }
}

export class Field<T> {
    constructor(private converter: Converter<T>, value: T | null, private isRequired?: boolean, validator?: Validator<T>) {
        this.value = value
        this.rawValue = converter.format(value)
        this.validator = isRequired
            ? combine(required(), validator || noValidate)
            : validator

        makeObservable(this)
    }

    private readonly validator: Validator<T>

    @observable value: T | null = null
    @observable rawValue: string = ''

    @observable dirty: boolean = false
    @observable touched: boolean = false
    @observable valid: boolean = true

    @observable error: string = ''

    @computed get isError() {
        return (this.dirty || this.touched) && !this.valid
    }

    @computed get errorText() {
        return this.isError ? this.error : ''
    }

    get forFormControl(): FormControlProps {
        return {
            required: this.isRequired,
            error: this.isError
        }
    }

    get forInputLabel(): InputLabelProps {
        return {
            required: this.isRequired,
        }
    }

    get forFormHelperText(): FormHelperTextProps {
        return {
            error: this.isError,
            children: this.errorText,
        }
    }

    get forTextField(): TextFieldProps {
        return {
            required: this.isRequired,
            value: this.rawValue,
            error: this.isError,
            helperText: this.errorText,
            onChange: e => this.onChange(e.target.value),
            onBlur: this.touch,
        }
    }

    get forSelect(): SelectProps {
        return {
            value: this.rawValue,
            error: this.isError,
            onChange: e => this.onChange(e.target.value as string),
            onBlur: this.touch,
        }
    }

    @action reset(value?: T | null) {
        this.value = value || null
        this.rawValue = this.converter.format(this.value)
        this.dirty = false
        this.touched = false
        this.valid = true
        this.error = ''
    }

    @action setValue(input: string) {
        this.rawValue = input

        const { error, value } = this.converter.sanitzie(input)

        if (error) {
            this.error = error
            this.value = null

            this.dirty = true
            this.valid = false

            return
        }

        this.dirty = this.dirty || this.value !== value
        this.value = value

        const validationError = this.validator?.(this.value)
        if (validationError) {
            this.error = validationError
            this.valid = false

            return
        }

        this.valid = true
    }

    @action readonly onChange = (value: string) => {
        this.touched = true
        this.setValue(value)
    }

    @action touch = () => {
        if (!this.touched) {
            this.setValue(this.rawValue)
        }

        this.touched = true
    }
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

export class MapLevelItem {
    constructor(label: string = '', width: number = null, height: number = null) {
        this.label = new Field<string>(STRING_TRIM, label, true, maxLen(128))
        this.width = new Field<number>(INT, width, true, combine(min(1), dividesWithEight))
        this.height = new Field<number>(INT, height, true, combine(min(1), dividesWithEight))
    }

    readonly label: Field<string>
    readonly width: Field<number>
    readonly height: Field<number>
}

const TIME_ZONES = Intl.supportedValuesOf('timeZone')

export class NewGameStore {
    constructor(private readonly store: HomeStore) {
        makeObservable(this)

        this.name = new Field<string>(STRING_TRIM, '', true, maxLen(128))
        this.timeZone = new Field<string>(STRING, Intl.DateTimeFormat().resolvedOptions().timeZone, true)
    }

    readonly gameFile = new FileViewModel()
    readonly playersFile = new FileViewModel()

    readonly name: Field<string>
    readonly timeZone: Field<string>

    @observable isOpen = false
    @observable remote = false
    @observable engine = ''

    readonly timeZones = TIME_ZONES

    readonly mapLevels: IObservableArray<MapLevelItem> = observable<MapLevelItem>([])

    @action open = () => {
        this.isOpen = true

        this.gameFile.clear()
        this.playersFile.clear()
        this.name.reset()
        this.engine = ''

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
                name: this.name.value,
                // options: JSON.parse(this.options),
                gameEngineId: this.engine,
                map: [],
                schedule: '',
                timeZone: ''
                // playerData: this.playersFile,
                // gameData: this.gameFile
            }

            // FIXME
            // mutate(GameCreate, variables, {
            //     refetch: [ this.store.games ]
            // })
            // .then(this.cancel, console.error)
        }
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
