import { action, computed, makeObservable } from 'mobx'
import { GameStore } from '../game-store'

export interface InterfaceCommand {
    readonly title: string
    readonly tooltip?: string
    readonly error: string
    readonly canExecute: boolean
    readonly visible: boolean
    execute()
}

export class MoveCommand implements InterfaceCommand {
    constructor (private game: GameStore) {
        makeObservable(this)
    }

    readonly title = 'Move'
    readonly tooltip = 'Order unit to move into another region or structure'

    @computed get error() {
        if (this.game.unit.isOverweight) return 'Cannot move'

        return ''
    }

    @computed get canExecute() {
        return this.visible && !this.error
    }

    @computed get visible() {
        return this.game?.unit?.isPlayer ?? false
    }

    @action execute() {

    }
}
