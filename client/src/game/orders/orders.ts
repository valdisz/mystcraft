import { Direction } from '../../schema'
import { Region } from '../region'
import { Ruleset } from '../ruleset'
import { Structure } from '../structure'
import { Unit } from '../unit'

function directionToString(dir: Direction) {
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

export type MoveStep = Direction | 'in' | 'out' | number

export abstract class BaseOrder {
    protected ensure(predicate: () => boolean, errorMessage: string) {
        if (!this.isValid) return false

        if (!predicate()) {
            this.setError(errorMessage)
            return false
        }

        this.clearError()
        return true
    }

    isValid: boolean
    dirty: boolean
    executed: boolean
    error: string

    setError(message: string) {
        this.isValid = false
        this.error = message
    }

    clearError() {
        this.isValid = true
        this.error = null
    }

    abstract toString(): string
    abstract execute(): Partial<Unit>
}

export class Move extends BaseOrder {
    constructor (private ruleset: Ruleset, public unit: Unit, public path: MoveStep[]) {
        super()

        this.ensure(() => !!path.length, 'Move path is not set')
    }

    execute(): Partial<Unit> {
        let curStructure: Structure = this.unit.structure
        let curRegion: Region = this.unit.region

        let usedMp = 0
        const S = [...this.path].reverse()
        const P = []

        loop:
        while (S.length > 0) {
            const step = S.pop()

            if (typeof step === 'number') {
                if (curRegion.explored) {
                    const nextStructure = curRegion.structures.find(s => s.num == step)
                    if (!this.ensure(() => !!nextStructure, `Cannot find structure with number ${step} in the region ${curRegion}`)) {
                        break loop
                    }
                }

                P.push(step)
                continue
            }

            switch (step) {
                case 'in':
                    // todo: out of scope, IN moves through the shaft/static portal in nexus.
                    P.push(step)
                    break loop

                case 'out':
                    // if we are in the region then OUT will do nothing
                    if (curStructure) {
                        // otherwise we will end up in the region
                        curStructure = null
                    }

                    P.push(step)
                    break

                default:
                    const link = curRegion.neighbors.find(x => x.direction == step)
                    if (!this.ensure(() => !!link, `There is no exit in ${step} direction in the region ${curRegion}`)) {
                        break loop
                    }

                    const cost = curRegion.terrain.getMoveCost(this.unit.capacity)
            }
        }

        return {
            path: [ ]
        }
    }

    toString(): string {
        let str = `move ${this.path.join(' ')}`
        if (!this.isValid) {
            str += `; ${this.error}`
        }

        return str
    }
}
