import { IAtom, IObservableValue, action, computed, createAtom, makeObservable, observable, runInAction } from 'mobx'
import { FieldView } from './field'
import { _asCallable, _or, _and, _every, _some, _isEmpty } from './utils'
import { OneOrManyValidators, Validator, makeValidator } from './validation'

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

    get value() {
        this.valueObserved()
        return this._items.map(f => f.value)
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
        return _isEmpty(this.items)
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
        validator: makeValidator(validators),
        ...options
    })
}
