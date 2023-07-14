import { action, makeObservable, observable, runInAction } from 'mobx'
import { GameEngineCreateResult, GameEngineDeleteResult, GameEngineFragment } from '../schema'
import { GetGameEngines, GetGameEnginesQuery } from '../schema'
import { GameEngineCreate, GameEngineCreateMutation, GameEngineCreateMutationVariables } from '../schema'
import { GameEngineDelete, GameEngineDeleteMutation, GameEngineDeleteMutationVariables } from '../schema'
import { seq, mutate } from './connection'
import { FileViewModel } from '../components'

export class OperationViewModel {
    constructor(private readonly onConfirm: () => Promise<any>) {
        makeObservable(this)
    }

    @observable isLoading = false
    @observable isError = false
    @observable error = ''
}

export class GameEnginesStore {
    readonly engines = seq<GetGameEnginesQuery, GameEngineFragment>(GetGameEngines, data => data.gameEngines.items || [], null, 'engines')

    readonly opGameEngineAdd = mutate<GameEngineCreateMutation, GameEngineCreateMutationVariables, GameEngineCreateResult, GameEngineFragment>({
        name: 'opAddEngine',
        document: GameEngineCreate,
        pick: data => data.gameEngineCreate,
        map: data => data.engine,
        onSuccess: engine => {
            runInAction(() => {
                this.engines.insert(0, engine)
                this.newEngine.close()
            })
        }
    })

    readonly opGameEngineDelete = mutate<GameEngineDeleteMutation, GameEngineDeleteMutationVariables, GameEngineDeleteResult, void>({
        name: 'opDeleteEngine',
        document: GameEngineDelete,
        pick: data => data.gameEngineDelete,
        onSuccess: (_, variables) => {
            this.engines.remove(e => e.id === variables.gameEngineId)
        }
    })

    readonly newEngine = new NewGameEngineStore((name, content, ruleset) => this.opGameEngineAdd.run({ name, content, ruleset }))

    readonly delete = (gameEngineId: string) => this.opGameEngineDelete.run({ gameEngineId })
}

export interface NewGameEngineConfirmationCallback {
    (name: string, content: File, ruleset: File): Promise<any>
}

export class NewGameEngineStore {
    constructor(private readonly onConfirm: NewGameEngineConfirmationCallback) {
        makeObservable(this)
    }

    @observable name = ''
    @observable isOpen = false
    @observable inProgress = false
    @observable error = ''

    readonly content = new FileViewModel()
    readonly ruleset = new FileViewModel()

    @action readonly setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }

    @action readonly open = () => {
        this.isOpen = true

        this.name = ''
        this.inProgress = false
        this.error = ''
        this.content.clear()
        this.ruleset.clear()
    }

    @action readonly close = () => {
        this.isOpen = false
    }

    readonly autoClose = () => {
        if (this.inProgress) {
            return
        }

        this.close()
    }

    @action readonly begin = () => {
        this.inProgress = true
    }

    @action readonly end = () => {
        this.inProgress = false
    }

    @action readonly setError = (message: string) => this.error = message

    readonly confirm = async () => {
        await this.onConfirm(this.name, this.content.file, this.ruleset.file)
    }
}
