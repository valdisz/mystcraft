import { IAtom, createAtom, computed, makeObservable, action, observable, runInAction } from 'mobx'
import { FieldView } from './form-field'
import { FormControlProps, FormHelperTextProps, InputLabelProps } from '@mui/material'
import { Validator, combine, pass, required } from './validation'

export class FieldList<T extends FieldView> implements FieldView {
    constructor(private readonly _items: T[] = [], private readonly isRequired: boolean = false, ...validators: Validator<T[]>[]) {
        this._atom = createAtom('items')

        this._validator = isRequired
            ? combine(required(), ...validators, pass)
            : combine(...validators, pass)

        makeObservable(this, {
            dirty: computed,
            touched: computed,
            valid: computed,
            showError: computed,
            errorText: computed,
            touch: action,
            validate: action,
            reset: action,
            disabled: observable,
            disable: action,
            enable: action,
        })
    }

    private readonly _atom: IAtom
    private readonly _validator: Validator<T[]>
    private readonly _inner = observable.object({
        dirty: false,
        touched: false,
        valid: true,
        error: ''
    })

    private valueObserved() {
        this._atom.reportObserved()
    }

    private valueChanged() {
        this._atom.reportChanged()
        runInAction(() => {
            this._inner.dirty = true
            this.validate(false)
        })
    }

    private shouldShowError() {
        return (this.dirty || this.touched) && !this.valid
    }

    *[Symbol.iterator](): IterableIterator<T> {
        this.valueObserved()
        for (let i = 0; i < this._items.length; ++i) {
            yield this._items[i]
        }
    }

    get forFormControl(): FormControlProps {
        return {
            required: this.isRequired,
            error: this.showError
        }
    }

    get forInputLabel(): InputLabelProps {
        return {
            required: this.isRequired,
        }
    }

    get forFormHelperText(): FormHelperTextProps {
        return {
            error: this.showError,
            children: this.errorText,
        }
    }

    get showError() {
        return this.shouldShowError()
    }

    get errorText() {
        return this.shouldShowError() ? this._inner.error : ''
    }

    get items(): T[] {
        this.valueObserved()
        return this._items
    }

    get length(): number {
        return this.items.length
    }

    get dirty(): boolean {
        return this._inner.dirty || this.items.some(f => f.dirty)
    }

    get touched(): boolean {
        return this._inner.touched || this.items.some(f => f.touched)
    }

    get valid(): boolean {
        return this._inner.valid && this.items.every(f => f.valid)
    }

    disabled = false

    disable(): void {
        this.disabled = true
        this.items.forEach(f => f.disable())
    }

    enable(): void {
        this.disabled = false
        this.items.forEach(f => f.enable())
    }

    touch(everyone: boolean = true): void {
        if (this.disabled) {
            return
        }

        if (everyone) {
            this.items.forEach(f => f.touch())
        }

        if (!this._inner.touched) {
            this._inner.error = this._validator(this._items)
            this._inner.valid = !this._inner.error
        }

        this._inner.touched = true
    }

    validate(everyone: boolean = true): boolean {
        this._inner.error = this._validator(this._items)
        this._inner.valid = !this._inner.error

        if (everyone) {
            const outerValid = this._items.reduce((acc, f) => f.validate() && acc, true)
            return this._inner.valid && outerValid
        }

        return this._inner.valid
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
        this._inner.valid = true
        this._inner.error = ''
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
