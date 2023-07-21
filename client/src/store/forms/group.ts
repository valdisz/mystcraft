import { computed, makeObservable } from 'mobx';
import { FormField } from './form-field'

export class Group implements FormField {
    constructor(private readonly fields: FormField[]) {
        makeObservable(this, {
            dirty: computed,
            touched: computed,
            valid: computed
        })
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

export function makeGroup<T>(target: T): T & FormField {
    const props = Object.getOwnPropertyNames(target)
        .map(name => ({ name, value: target[name] }))

    const group = new Group(props.map(p => p.value))
    props.forEach(p => Object.defineProperty(group, p.name, {
        value: p.value,
        writable: false
    }))

    return group as any
}
