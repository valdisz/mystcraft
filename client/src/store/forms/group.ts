import { action, computed, makeObservable, observable } from 'mobx';
import { FieldView } from './form-field'

export class Group implements FieldView {
    constructor(private readonly fields: FieldView[]) {
        makeObservable(this, {
            dirty: computed,
            touched: computed,
            valid: computed,
            disabled: computed,
            enable: action,
            disable: action,
            touch: action,
            validate: action,
            reset: action,
        })
    }

    private readonly _inner = observable.object({
        disabled: false
    })

    get disabled() {
        return this._inner.disabled && this.fields.every(f => f.disabled)
    }

    get dirty(): boolean {
        return this.fields.some(f => f.dirty)
    }

    get touched(): boolean {
        return this.fields.some(f => f.touched)
    }

    get valid(): boolean {
        return this.fields.every(f => f.valid)
    }

    disable(): void {
        this._inner.disabled = true
        this.fields.forEach(f => f.disable())
    }

    enable(): void {
        this._inner.disabled = false
        this.fields.forEach(f => f.enable())
    }

    touch(): void {
        this.fields.forEach(f => f.touch())
    }

    validate(): boolean {
        return this.fields.reduce((acc, f) => f.validate() && acc, true)
    }

    reset(): void {
        this.fields.forEach(f => f.reset())
    }
}

export function makeGroup<T>(target: T): T & FieldView {
    const props = Object.getOwnPropertyNames(target)
        .map(name => ({ name, value: target[name] }))

    const group = new Group(props.map(p => p.value))
    props.forEach(p => Object.defineProperty(group, p.name, {
        value: p.value,
        writable: false
    }))

    return group as any
}
