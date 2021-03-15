import * as React from 'react'
import { makeObservable, observable, IObservableArray, runInAction, action } from 'mobx'
import { CLIENT } from '../client'
import { GetUniversity, GetUniversityQuery, GetUniversityQueryVariables, SettlementSize,  } from '../schema'
import { OpenUniversity, OpenUniversityMutation, OpenUniversityMutationVariables } from '../schema'
import { GetUniversityClass, GetUniversityClassQuery, GetUniversityClassQueryVariables } from '../schema'
import { ClassSummaryFragment, UniversityClassFragment, UniveristyMemberRole } from '../schema'
import { NamedTypeNode } from 'graphql'

class NewUniversity {
    constructor(private readonly parent: UniversityStore) {
        makeObservable(this)
    }

    @observable name = ''
    @action setName = (event: React.ChangeEvent<HTMLInputElement>) => {
        this.name = event.target.value
    }

    @action clear = () => {
        this.name = ''
    }

    open = async () => {
        this.parent.setLoading(true)

        const response = await CLIENT.mutate<OpenUniversityMutation, OpenUniversityMutationVariables>({
            mutation: OpenUniversity,
            variables: {
                name: this.name,
                playerId: this.parent.playerId
            }
        })

        this.parent.load(this.parent.gameId)
    }
}

export interface Skill {
    code: string
    level?: number
    days?: number
}

export interface SkillGroup {
    skills: Skill[]
}

export interface Student {
    id: string

    name: string
    number: number
    factionName: string
    factionNumber: number

    skills: SkillGroup[]

    orders: string | ''
    target: Skill | null
}

export interface StudyLocation {
    id: string
    x: number
    y: number
    z: number
    label: string
    terrain: string
    province: string
    settlement?: string
    settlementSize?: SettlementSize

    students: Student[]
}


function getSkillGroups() {
    return [
        // 'bases'
        {
            skills: [
                { code: 'FORC' },
                { code: 'PATT' },
                { code: 'SPIR' },
            ]
        },
        // 'sruvival'
        {
            skills: [
                { code: 'OBSE' },
                { code: 'STEA' },
            ]
        },
        // 'dragon'
        {
            skills: [
                { code: 'EART' },
                { code: 'BIRD' },
                { code: 'WOLF' },
                { code: 'DRAG' },
            ]
        },
        // 'artifacts'
        {
            skills: [
                { code: 'ARTI' },
                { code: 'CRCL' },
                { code: 'CRRI' },
                { code: 'CRSF' },
                { code: 'CFSW' },
                { code: 'CRWC' },
            ]
        },
        // 'demons'
        {
            skills: [
                { code: 'DEMO' },
                { code: 'SUIM' },
                { code: 'SUDE' },
                { code: 'SUBA' },
            ]
        },
        // 'fire'
        {
            skills: [
                { code: 'FIRE' },
                { code: 'FSHI' },
                { code: 'SSHI' },
            ]
        },
        // 'illusions'
        {
            skills: [
                { code: 'ILLU' },
                { code: 'PHEN' },
                { code: 'INVI' },
                { code: 'PHDDE' },
                { code: 'TRUE' },
            ]
        },
        // 'necromancy'
        {
            skills: [
                { code: 'NECR' },
                { code: 'SBLA' },
            ]
        },
        // 'other'
        {
            skills: [
                { code: 'WEAT' },
                { code: 'CLEA' },
                { code: 'CALL' },
                { code: 'SWIN' },
                { code: 'MHEA' },
                { code: 'FARS' },
            ]
        },
    ]
}

export class UniversityStore {
    constructor() {
        makeObservable(this)
    }

    readonly newUniversity = new NewUniversity(this)

    gameId: string = null
    playerId: string = null

    @observable loading = true
    @action setLoading = (isLoading: boolean) => this.loading = isLoading

    @observable role: UniveristyMemberRole = UniveristyMemberRole.Member
    @observable id: string = null
    @observable name: string = null

    readonly classes: IObservableArray<ClassSummaryFragment> = observable([])

    @observable selectedClassId: string = null

    @observable locations: IObservableArray<StudyLocation> = observable([])

    readonly skills = getSkillGroups()

    async load(gameId: string) {
        this.gameId = gameId

        runInAction(() => this.loading = true)

        const response = await CLIENT.query<GetUniversityQuery, GetUniversityQueryVariables>({
            query: GetUniversity,
            variables: {
                gameId
            }
        })

        const player = response.data.node?.myPlayer
        const university = player?.university
        const classes = university?.university?.classes ?? []

        if (classes.length) {
            classes.sort((a, b) => a.turnNumber - b.turnNumber)
        }

        if (player) this.playerId = player.id

        runInAction(() => {
            if (player.university) {
                const { role, university: { id, name } } = university

                this.role = role
                this.id = id
                this.name = name
                this.classes.replace(classes)

                if (classes.length) {
                    this.selectClass(classes[classes.length - 1].id)
                    return
                }
            }

            this.loading = false
        })
    }

    selectClass = async (classId: string) => {
        runInAction(() => this.loading = true)

        const response = await CLIENT.query<GetUniversityClassQuery, GetUniversityClassQueryVariables>({
            query: GetUniversityClass,
            variables: {
                classId
            }
        })

        const locations: { [ id: string]: StudyLocation } = { }
        const students: { [ id: string ]: Student } = { }

        for (const item of response.data.node.students) {
            const unit = item.unit
            const region = unit.region

            const orders = item.study
                ? `STUDY ${item.study}`
                : item.teach?.length
                    ? `TEACH ${item.teach.join(' ')}`
                    : ''

            const student: Student = {
                id: item.id,
                name: unit.name,
                number: unit.number,
                factionName: unit.faction.name,
                factionNumber: unit.faction.number,
                skills: getSkillGroups(),
                target: item.target
                    ? { code: item.target.code, level: item.target.level }
                    : null,
                orders
            }
            students[item.id] = student

            const skillIndex: { [ code: string ]: Skill } = {}
            for (const g of student.skills) {
                for (const s of g.skills) {
                    skillIndex[s.code] = s
                }
            }

            for (const { code, level, days } of unit.skills) {
                const s = skillIndex[code]
                if (!s) continue

                s.level = level
                s.days = days
            }

            const locationId = `${region.x} ${region.y} ${region.z}`

            if (!locations[locationId]) {
                locations[locationId] = {
                    id: locationId,
                    terrain: region.terrain,
                    x: region.x,
                    y: region.y,
                    z: region.z,
                    label: region.label,
                    province: region.province,
                    settlement: region.settlement?.name,
                    settlementSize: region.settlement?.size,
                    students: []
                }
            }

            const loc = locations[locationId]
            loc.students.push(student)
        }

        for (const loc in locations) {
            locations[loc].students.sort((a, b) => {
                if (a.factionNumber === b.factionNumber) return a.number - b.number

                return a.factionNumber - b.factionNumber
            })
        }

        runInAction(() => {
            this.selectedClassId = classId
            this.locations.replace(Object.values(locations))
            this.loading = false
        })
    }
}
