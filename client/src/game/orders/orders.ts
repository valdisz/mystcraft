import { Direction } from "../../schema";
import { Link } from "../link";
import { Unit } from "../unit";

export abstract class BaseOrder {
    protected ensure(predicate: () => boolean, errorMessage: string) {
        if (!this.isValid) return

        if (!predicate()) {
            this.isValid = false
            this.error = errorMessage
        }
    }

    isValid: boolean
    error: string

    abstract getOrder(): number
    abstract toString(): string
    abstract execute(): Partial<Unit>
}

function toDirection(dir: Direction) {
    switch (dir) {
        case Direction.North: return 'n'
        case Direction.Northeast: return 'ne'
        case Direction.Northwest: return 'nw'
        case Direction.South: return 's'
        case Direction.Southeast: return 'se'
        case Direction.Southwest: return 'sw'
        default: return dir as string
    }
}

export class Move extends BaseOrder {
    constructor (public unit: Unit, public path: Link[]) {
        super()

        this.ensure(() => !!path.length, 'Move path is not set')
    }

    getOrder(): number {
        return 1
    }

    execute(): Partial<Unit> {
        return {
            path: this.path
        }
    }

    toString(): string {
        return this.isValid
            ? `move ${this.path.map(x => toDirection(x.direction)).join(' ')}`
            : `move; ${this.error}`
    }
}
