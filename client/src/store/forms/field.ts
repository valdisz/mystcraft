import { IAtom, IObservableValue, action, computed, createAtom, makeObservable, observable, runInAction } from 'mobx'
import { FormControlProps, FormHelperTextProps, InputLabelProps, TextFieldProps } from '@mui/material'
import { Converter, ValueOrError } from './converters'
import { Validator, combine, pass } from './validation'
import { FileFieldProps, SelectFieldProps, SwitchFieldProps } from '../../components'

export type Option<T> = T | null | undefined

export interface FieldView {
    readonly isDisabled: boolean
    readonly isEnabled: boolean
    readonly isReadonly: boolean
    readonly isWritable: boolean
    readonly isRequired: boolean
    readonly isOptional: boolean
    readonly isDirty: boolean
    readonly isPristine: boolean
    readonly isTouched: boolean
    readonly isUntouched: boolean
    readonly isActive: boolean
    readonly isInactive: boolean
    readonly isEmpty: boolean
    readonly isNotEmpty: boolean
    readonly isValid: boolean
    readonly isInvalid: boolean
    readonly isErrorVisible: boolean
    readonly error: string

    reset(): void
    touch(): void
    activate(): void
    deactivate(): void
    setParent(parent: FieldView): void
}


export interface FieldOptions<TRaw, T> {
    converter: Converter<TRaw, T>
    initialValue?: T
    validator?: Validator<T>
    isRequired?: boolean | (() => boolean)
    isReadonly?: boolean | (() => boolean)
    isDisabled?: boolean | (() => boolean)
}

function _yes() {
    return true
}

function _no() {
    return false
}

/**
 * Converts a boolean or a function that returns a boolean into a function that returns a boolean.
 */
function _asCallable(value: boolean | (() => boolean)) {
    return typeof value === 'function'
        ? value
        : (
            value ? _yes : _no
        )
}

/**
 * So that MobX change detection captures boths sides of the OR operation, we need to wrap it into a function.
 * This will evaluate both sides of the OR operation, and then return the result.
 */
function _or(a: boolean, b: boolean) {
    return a || b
}

function _and(a: boolean, b: boolean) {
    return a && b
}

function _some<T>(items: T[], predicate: (item: T) => boolean) {
    return items.reduce((acc, item) => acc || predicate(item), false)
}

function _every<T>(items: T[], predicate: (item: T) => boolean) {
    return items.reduce((acc, item) => acc && predicate(item), true)
}

function isEmpty<T>(value: T): boolean {
    if (value === null || value === undefined || value === '' || (Array.isArray(value) && value.length === 0)) {
        return true
    }

    return false
}


export function forFormControl(f: FieldView): FormControlProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        error: f.isErrorVisible
    }
}

export function forInputLabel(f: FieldView): InputLabelProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        error: f.isErrorVisible,
    }
}

export function forFormHelperText(f: FieldView): FormHelperTextProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        error: f.isErrorVisible,
        children: f.error,
    }
}

export function forTextField<T>(f: Field<string, T | null>): TextFieldProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        value: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: e => f.handleChange(e.target.value),
        onBlur: f.handleTouch,
    }
}

export function forFileField(f: Field<File | null, File | null>): FileFieldProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        value: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: f.handleChange,
        onBlur: f.handleTouch,
    }
}

export function forSelectField<T>(f: Field<T | null, T | null>): Partial<SelectFieldProps<T | null>> {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        value: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: f.handleChange,
        onBlur: f.handleTouch,
    }
}

export function forSwitchField(f: Field<boolean | null | undefined, boolean | null>): SwitchFieldProps {
    return {
        required: f.isRequired,
        disabled: _or(f.isDisabled, f.isReadonly),
        checked: f.rawValue,
        error: f.isErrorVisible,
        helperText: f.error,
        onChange: (event) => f.handleChange(event.target.checked),
        onBlur: f.handleTouch,
    }
}

type OneOrManyValidators<T> = Validator<T> | Validator<T>[]

function _makeValidator<T>(validators?: OneOrManyValidators<T>): Validator<T> {
    if (!validators) {
        return pass
    }

    if (Array.isArray(validators)) {
        if (validators.length === 0) {
            return pass
        }

        if (validators.length === 1) {
            return validators[0]
        }

        return combine(...validators)
    }

    return validators
}

const STRING: Converter<string, string> = {
    sanitize: value => ({ value: value ? value : null }),
    format: value => value ?? ''
}

const STRING_TRIM: Converter<string, string> = {
    sanitize: value => ({ value: value ? value?.trim() : null || '' }),
    format: value => value ?? ''
}

const INT: Converter<Option<string>, Option<number>> = {
    sanitize: s => {
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

const REAL: Converter<Option<string>, Option<number>> = {
    sanitize: s => {
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

const FILE: Converter<Option<File>, Option<File>> = {
    sanitize: (value: File) => ({ value: value === undefined ? null : value }),
    format: value => value
}

const BOOLEAN: Converter<Option<boolean | string>, Option<boolean>> = {
    sanitize: value => {
        if (value === null || value === undefined) {
            return { value: null }
        }

        if (typeof value === 'boolean') {
            return { value }
        }

        switch (value.toLowerCase().trim()) {
            case 'true':
            case 'yes':
            case 'on':
            case '1':
                return { value: true }

            case 'false':
            case 'off':
            case 'no':
            case '0':
              return { value: false }

            default: {
                try {
                    const parsed = JSON.parse(value)
                    return { value: !!parsed }
                }
                catch (e) {
                    return { error: 'Value is not a boolean' }
                }

            }
        }
    },
    format: value => value
}


export interface BaseFieldFactoryOptions<TRaw, T> extends Omit<
    FieldOptions<TRaw, T>,
    'converter' | 'validator'
> { }

export interface StringFieldOptions extends BaseFieldFactoryOptions<Option<string>, Option<string>> {
    trim?: boolean
}

export function text(validators?: OneOrManyValidators<Option<string>>, options?: StringFieldOptions): Field<Option<string>, Option<string>>{
    const trim = options?.trim ?? false
    return new Field({
        converter: trim ? STRING_TRIM : STRING,
        validator: _makeValidator(validators),
        ...options
    })
}

export interface IntFieldOptions extends BaseFieldFactoryOptions<Option<string>, Option<number>> {
}

export function int(validators?: OneOrManyValidators<Option<number>>, options?: IntFieldOptions): Field<Option<string>, Option<number>>{
    return new Field({
        converter: INT,
        validator: _makeValidator(validators),
        ...options
    })
}

export interface RealFieldOptions extends BaseFieldFactoryOptions<Option<string>, Option<number>> {
}

export function real(validators?: OneOrManyValidators<Option<number>>, options?: IntFieldOptions): Field<Option<string>, Option<number>> {
    return new Field({
        converter: REAL,
        validator: _makeValidator(validators),
        ...options
    })
}

export interface FileFieldOptions extends BaseFieldFactoryOptions<Option<File>, Option<File>> {
}

export function file(validators?: OneOrManyValidators<Option<File>>, options?: FileFieldOptions): Field<Option<File>, Option<File>> {
    return new Field({
        converter: FILE,
        validator: _makeValidator(validators),
        ...options
    })
}

export interface BoolFieldOptions extends BaseFieldFactoryOptions<Option<string | boolean>, Option<boolean>> {
}

export function bool(validators?: OneOrManyValidators<Option<boolean>>, options?: BoolFieldOptions): Field<Option<string | boolean>, Option<boolean>> {
    return new Field({
        converter: BOOLEAN,
        validator: _makeValidator(validators),
        ...options
    })
}

interface FieldInnerState<TRaw> {
    value: TRaw
    dirty: boolean
    touched: boolean
    active: boolean
}

export class Field<TRaw, T> implements FieldView {
    constructor({ converter, initialValue, validator, isReadonly, isRequired, isDisabled }: FieldOptions<TRaw, T>) {
        this._converter = converter
        this._inner = observable.object({
            dirty: false,
            touched: false,
            active: false,
            value: converter.format(initialValue),
        })

        this._parent = observable.box(null, { deep: false })

        this._isReadonly = _asCallable(isReadonly)
        this._isRequired = _asCallable(isRequired)
        this._isDisabled = _asCallable(isDisabled)
        this._validator = validator

        makeObservable(this, {
            setParent: action,
            state: computed,
            value: computed,
            isDisabled: computed,
            isEnabled: computed,
            isReadonly: computed,
            isWritable: computed,
            isRequired: computed,
            isOptional: computed,
            isDirty: computed,
            isPristine: computed,
            isTouched: computed,
            isUntouched: computed,
            isActive: computed,
            isInactive: computed,
            isEmpty: computed,
            isNotEmpty: computed,
            isValid: computed,
            isInvalid: computed,
            isErrorVisible: computed,
            error: computed,
            reset: action,
            setValue: action,
            touch: action,
            activate: action,
            deactivate: action,
            handleChange: action,
        })
    }

    private readonly _converter: Converter<TRaw, T>
    private readonly _validator: Validator<T>
    private readonly _isReadonly: () => boolean
    private readonly _isRequired: () => boolean
    private readonly _isDisabled: () => boolean
    private readonly _inner: FieldInnerState<TRaw>

    private _parent: IObservableValue<FieldView>

    setParent(parent: FieldView) {
        this._parent.set(parent)
    }

    get state(): ValueOrError<T> {
        const { value, error } = this._converter.sanitize(this._inner.value)
        const required = this._isRequired()

        if (error) {
            return { error }
        }

        if (required && isEmpty(value)) {
            return {
                value,
                error: 'Required'
            }
        }

        return {
            value,
            error: this._validator(value)
        }
    }

    get value() {
        const { value, error} = this.state
        return error ? null : value
    }

    get rawValue() {
        return this._inner.value
    }

    get isDisabled() {
        return _or(this._parent.get()?.isDisabled ?? false, this._isDisabled())
    }

    get isEnabled() {
        return !this.isDisabled
    }

    get isReadonly() {
        return _or(this._parent.get()?.isReadonly ?? false, _or(this.isDisabled, this._isReadonly()))
    }

    get isWritable() {
        return !this.isReadonly
    }

    get isRequired() {
        return _or(this._parent.get()?.isRequired ?? false, _and(!this.isDisabled, this._isRequired()))
    }

    get isOptional() {
        return !this.isRequired
    }

    get isDirty(): boolean {
        return _and(!this.isDisabled, this._inner.dirty)
    }

    get isPristine(): boolean {
        return !this.isDirty
    }

    get isTouched(): boolean {
        return _and(!this.isDisabled, this._inner.touched)
    }

    get isUntouched(): boolean {
        return !this.isTouched
    }

    get isActive(): boolean {
        return _and(!this.isDisabled, this._inner.active)
    }

    get isInactive(): boolean {
        return !this.isActive
    }

    get isEmpty(): boolean {
        return isEmpty(this.value)
    }

    get isNotEmpty(): boolean {
        return !this.isEmpty
    }

    get isValid(): boolean {
        return _or(this.isDisabled, this.state.error === null)
    }

    get isInvalid(): boolean {
        return !this.isValid
    }

    get isErrorVisible() {
        const enabled = this.isEnabled
        const touched = this.isTouched
        const valid = this.isValid

        if (!enabled) {
            return false
        }

        if (valid) {
            return false
        }

        if (!touched) {
            return false
        }

        return true
    }

    get error() {
        const err = this.state.error
        return this.isErrorVisible ? err : ''
    }

    /**
     * Handles a event when the user interacted (touched) with the field.
     */
    readonly handleTouch = () => this.touch()

    /**
     * Handles a change event from the input.
     * This will trigger validation, update the dirty state, and change the touched state.
     */
    readonly handleChange = (value: any) => {
        this._inner.touched = true
        this.setValue(value)
    }

    /**
     * Sets the value of the field programmatically.
     * This will trigger validation, and update the dirty state, but will not change touched state.
     */
    setValue(input: TRaw) {
        const changed = this._inner.value !== input
        if (!changed) {
            return
        }

        this._inner.value = input
        this._inner.dirty = true
    }

    /**
     * Resets the field to its initial state.
     * Optionally, a new value can be provided.
     */
    reset(value?: T) {
        this._inner.value = this._converter.format(value)
        this._inner.dirty = false
        this._inner.touched = false
        this._inner.active = false
    }

    touch() {
        if (this.isDisabled) {
            return
        }

        this._inner.touched = true
    }

    activate() {
        if (this.isDisabled) {
            return
        }

        this._inner.active = true
    }

    deactivate() {
        if (this.isDisabled) {
            return
        }

        this._inner.active = false
    }
}

interface GroupOptions {
    isRequired?: boolean | (() => boolean)
    isReadonly?: boolean | (() => boolean)
    isDisabled?: boolean | (() => boolean)
}

class Group implements FieldView {
    constructor(private readonly _fields: FieldView[], options?: GroupOptions) {
        this._isReadonly = _asCallable(options?.isReadonly ?? false)
        this._isRequired = _asCallable(options?.isRequired ?? false)
        this._isDisabled = _asCallable(options?.isDisabled ?? false)

        this._parent = observable.box(null, { deep: false })
        for (const f of this._fields) {
            f.setParent(this)
        }

        makeObservable(this, {
            setParent: action,
            isDisabled: computed,
            isEnabled: computed,
            isReadonly: computed,
            isWritable: computed,
            isRequired: computed,
            isOptional: computed,
            isDirty: computed,
            isPristine: computed,
            isTouched: computed,
            isUntouched: computed,
            isActive: computed,
            isInactive: computed,
            isEmpty: computed,
            isNotEmpty: computed,
            isValid: computed,
            isInvalid: computed,
            isErrorVisible: computed,
            error: computed,
            reset: action,
            touch: action,
            activate: action,
            deactivate: action,
        })
    }

    private readonly _isReadonly: () => boolean
    private readonly _isRequired: () => boolean
    private readonly _isDisabled: () => boolean

    private _parent: IObservableValue<FieldView>

    setParent(parent: FieldView) {
        this._parent.set(parent)
    }

    get isDisabled() {
        return _or(this._parent.get()?.isDisabled ?? false, this._isDisabled())
    }

    get isEnabled() {
        return !this.isDisabled
    }

    get isReadonly() {
        return _or(this._parent.get()?.isReadonly ?? false, _or(this.isDisabled, this._isReadonly()))
    }

    get isWritable() {
        return !this.isReadonly
    }

    get isRequired() {
        return _or(this._parent.get()?.isRequired ?? false, _and(!this.isDisabled, this._isRequired()))
    }

    get isOptional() {
        return !this.isRequired
    }

    get isDirty(): boolean {
        return _some(this._fields, f => f.isDirty)
    }

    get isPristine(): boolean {
        return !this.isDirty
    }

    get isTouched(): boolean {
        return _some(this._fields, f => f.isTouched)
    }

    get isUntouched(): boolean {
        return !this.isTouched
    }

    get isActive(): boolean {
        return _some(this._fields, f => f.isActive)
    }

    get isInactive(): boolean {
        return !this.isActive
    }

    get isEmpty(): boolean {
        return _every(this._fields, f => f.isEmpty)
    }

    get isNotEmpty(): boolean {
        return !this.isEmpty
    }

    get isValid(): boolean {
        return _every(this._fields, f => f.isValid)
    }

    get isInvalid(): boolean {
        return !this.isValid
    }

    get isErrorVisible() {
        return _some(this._fields, f => f.isErrorVisible)
    }

    get error() {
        return this._fields.map(f => f.error).filter(e => !!e).join(', ')
    }

    reset() {
        this._fields.forEach(f => f.reset())
    }

    touch() {
        this._fields.forEach(f => f.touch())
    }

    activate() {
        this._fields.forEach(f => f.activate())
    }

    deactivate() {
        this._fields.forEach(f => f.deactivate())
    }
}

export function group<T>(target: T, options?: GroupOptions): T & FieldView {
    const props = Object.getOwnPropertyNames(target)
        .map(name => ({ name, value: target[name] }))

    const group = new Group(props.map(p => p.value), options)

    props.forEach(p => Object.defineProperty(group, p.name, {
        value: p.value,
        writable: false
    }))

    return group as any
}

export interface ListOptions<T extends FieldView> {
    initialValue?: T[]
    validator?: Validator<T[]>
    isRequired?: boolean | (() => boolean)
    isReadonly?: boolean | (() => boolean)
    isDisabled?: boolean | (() => boolean)
}

interface ListInnerState {
    dirty: boolean
    touched: boolean
    active: boolean
}

export class List<T extends FieldView> implements FieldView {
    constructor({ initialValue, validator, isReadonly, isRequired, isDisabled }: ListOptions<T>) {
        this._atom = createAtom('list')
        this._items = initialValue ?? []

        this._inner = observable.object({
            dirty: false,
            touched: false,
            active: false
        })

        this._parent = observable.box(null, { deep: false })

        this._isReadonly = _asCallable(isReadonly)
        this._isRequired = _asCallable(isRequired)
        this._isDisabled = _asCallable(isDisabled)
        this._validator = validator

        makeObservable(this, {
            setParent: action,
            isDisabled: computed,
            isEnabled: computed,
            isReadonly: computed,
            isWritable: computed,
            isRequired: computed,
            isOptional: computed,
            isDirty: computed,
            isPristine: computed,
            isTouched: computed,
            isUntouched: computed,
            isActive: computed,
            isInactive: computed,
            isEmpty: computed,
            isNotEmpty: computed,
            isValid: computed,
            isInvalid: computed,
            isErrorVisible: computed,
            validationError: computed,
            error: computed,
            reset: action,
            touch: action,
            activate: action,
            deactivate: action,
        })
    }

    private readonly _validator: Validator<T[]>
    private readonly _isReadonly: () => boolean
    private readonly _isRequired: () => boolean
    private readonly _isDisabled: () => boolean
    private readonly _inner: ListInnerState

    private readonly _atom: IAtom
    private readonly _items: T[]

    private _parent: IObservableValue<FieldView>

    private valueObserved() {
        this._atom.reportObserved()
    }

    private valueChanged() {
        runInAction(() => {
            this._atom.reportChanged()
            this._inner.dirty = true
        })
    }

    *[Symbol.iterator](): IterableIterator<T> {
        this.valueObserved()
        for (let i = 0; i < this._items.length; ++i) {
            yield this._items[i]
        }
    }

    get items() {
        this.valueObserved()
        return this._items
    }

    setParent(parent: FieldView) {
        this._parent.set(parent)
    }

    get isDisabled() {
        return _or(this._parent.get()?.isDisabled ?? false, this._isDisabled())
    }

    get isEnabled() {
        return !this.isDisabled
    }

    get isReadonly() {
        return _or(this._parent.get()?.isReadonly ?? false, _or(this.isDisabled, this._isReadonly()))
    }

    get isWritable() {
        return !this.isReadonly
    }

    get isRequired() {
        return _or(this._parent.get()?.isRequired ?? false, _and(!this.isDisabled, this._isRequired()))
    }

    get isOptional() {
        return !this.isRequired
    }

    get isDirty(): boolean {
        return _and(!this.isDisabled, _or(this._inner.dirty, this.items.some(f => f.isDirty)))
    }

    get isPristine(): boolean {
        return !this.isDirty
    }

    get isTouched(): boolean {
        return _and(!this.isDisabled, this._inner.touched)
    }

    get isUntouched(): boolean {
        return !this.isTouched
    }

    get isActive(): boolean {
        return _some(this.items, f => f.isActive)
    }

    get isInactive(): boolean {
        return !this.isActive
    }

    get isEmpty(): boolean {
        return isEmpty(this.items)
    }

    get isNotEmpty(): boolean {
        return !this.isEmpty
    }

    get isValid(): boolean {
        return _and(_every(this.items, f => f.isValid), !this.validationError)
    }

    get isInvalid(): boolean {
        return !this.isValid
    }

    get isErrorVisible() {
        const enabled = this.isEnabled
        const touched = this.isTouched
        const valid = this.isValid

        if (!enabled) {
            return false
        }

        if (valid) {
            return false
        }

        if (!touched) {
            return false
        }

        return true
    }

    get validationError() {
        return this._validator(this.items)
    }

    get error() {
        const err = this.validationError
        return this.isErrorVisible ? err : ''
    }

    reset(...items: T[]): void {
        const newItems = items || []
        if (this._items.length || newItems.length) {
            this._items.length = 0
            this._items.push(...newItems)
            this.valueChanged()
        }

        this._inner.dirty = false
        this._inner.touched = false
    }

    touch(withItems: boolean = true) {
        this._inner.touched = true

        if (withItems) {
            this._items.forEach(f => f.touch())
        }
    }

    activate() {
        this._inner.active = true
    }

    deactivate() {
        this._inner.active = true
    }

    /////////////////////////
    ///// reading sequence

    map<U>(callbackfn: (value: T, index: number, array: T[]) => U, thisArg?: any): U[] {
        return this.items.map(callbackfn)
    }

    filter<S extends T>(predicate: (value: T, index: number, array: T[]) => value is S, thisArg?: any): S[] {
        return this.items.filter(predicate)
    }

    find(predicate: (value: T, index: number, obj: T[]) => boolean, thisArg?: any): T | undefined {
        return this.items.find(predicate)
    }

    /////////////////////////
    ///// writing sequence

    push(...items: T[]): number {
        const ret = this._items.push(...items)

        if (items?.length) {
            this.valueChanged()
        }

        return ret
    }

    pop(): T | undefined {
        const willChange = this._items.length > 0

        const ret = this._items.pop()

        if (willChange) {
            this.valueChanged()
        }

        return ret
    }

    splice(start: number, deleteCount: number, ...items: T[]): T[] {
        const ret = this._items.splice(start, deleteCount, ...items)

        if (ret.length || items?.length) {
            this.valueChanged()
        }

        return ret
    }

    insert(index: number, ...items: T[]): T[] {
        return this.splice(index, 0, ...items)
    }

    remove(predicate: (value: T, index: number, array: T[]) => boolean): T[] {
        const ret = []
        for (let i = this._items.length - 1; i >= 0; --i) {
            if (predicate(this._items[i], i, this._items)) {
                ret.push(...this._items.splice(i, 1))
            }
        }

        if (ret.length) {
            this.valueChanged()
        }

        return ret
    }
}

export function list<T extends FieldView>(validators?: OneOrManyValidators<T[]>, options?: Omit<ListOptions<T>, 'validator'>): List<T> {
    return new List<T>({
        validator: _makeValidator(validators),
        ...options
    })
}

export interface CustomFieldOptions<TRaw, T> extends BaseFieldFactoryOptions<TRaw, T> {
}

export function custom<TRaw, T = TRaw>(converter: Converter<TRaw, T>, validators?: OneOrManyValidators<T>, options?: CustomFieldOptions<TRaw, T>): Field<TRaw, T> {
    return new Field<TRaw, T>({
        converter,
        validator: _makeValidator(validators),
        ...options
    })
}
