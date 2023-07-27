import { IObservableValue, action, computed, makeObservable, observable } from 'mobx'
import { FieldView } from './field'
import { _asCallable, _or, _and, _every, _some } from './utils'

interface GroupOptions {
    isRequired?: boolean | (() => boolean)
    isReadonly?: boolean | (() => boolean)
    isDisabled?: boolean | (() => boolean)
}

class Group implements FieldView {
    constructor(private readonly _fields: { name: string, value: FieldView }[], options?: GroupOptions) {
        this._isReadonly = _asCallable(options?.isReadonly ?? false)
        this._isRequired = _asCallable(options?.isRequired ?? false)
        this._isDisabled = _asCallable(options?.isDisabled ?? false)

        this._parent = observable.box(null, { deep: false })

        for (const { name, value } of _fields) {
            Object.defineProperty(this, name, {
                value,
                writable: false
            })
            value.setParent(this)
        }

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

    private get _values() {
        return this._fields.map(f => f.value)
    }

    setParent(parent: FieldView) {
        this._parent.set(parent)
    }

    get value() {
        const value = this._fields.reduce((acc, f) => {
            acc[f.name] = f.value.value
            return acc
        }
        , {} as any)

        return this.isEnabled ? value : null
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
        return _and(this.isEnabled, _some(this._values, f => f.isDirty))
    }

    get isPristine(): boolean {
        return !this.isDirty
    }

    get isTouched(): boolean {
        return _and(this.isEnabled, _some(this._values, f => f.isTouched))
    }

    get isUntouched(): boolean {
        return !this.isTouched
    }

    get isActive(): boolean {
        return _and(this.isEnabled, _some(this._values, f => f.isActive))
    }

    get isInactive(): boolean {
        return !this.isActive
    }

    get isEmpty(): boolean {
        return _every(this._values, f => f.isEmpty)
    }

    get isNotEmpty(): boolean {
        return !this.isEmpty
    }

    get isValid(): boolean {
        return _or(this.isDisabled, _every(this._values, f => f.isValid))
    }

    get isInvalid(): boolean {
        return !this.isValid
    }

    get isErrorVisible() {
        return _some(this._values, f => f.isErrorVisible)
    }

    get error() {
        return this._values.map(f => f.error).filter(e => !!e).join(', ')
    }

    reset() {
        this._values.forEach(f => f.reset())
    }

    touch() {
        this._values.forEach(f => f.touch())
    }

    activate() {
        this._values.forEach(f => f.activate())
    }

    deactivate() {
        this._values.forEach(f => f.deactivate())
    }
}

export function group<T>(target: T, options?: GroupOptions): T & FieldView {
    const props = Object.getOwnPropertyNames(target)
        .map(name => ({ name, value: target[name] }))

    const group = new Group(props, options)

    return group as any
}
