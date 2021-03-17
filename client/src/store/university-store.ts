import * as React from 'react'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { GetUniversity, GetUniversityQuery, GetUniversityQueryVariables, SettlementSize, StudyPlanFragment, UniversityMemberFragment,  } from '../schema'
import { OpenUniversity, OpenUniversityMutation, OpenUniversityMutationVariables } from '../schema'
import { GetUniversityClass, GetUniversityClassQuery, GetUniversityClassQueryVariables } from '../schema'
import { SetStudPlanyTarget, SetStudPlanyTargetMutation, SetStudPlanyTargetMutationVariables } from '../schema'
import { SetStudPlanyStudy, SetStudPlanyStudyMutation, SetStudPlanyStudyMutationVariables } from '../schema'
import { SetStudPlanyTeach, SetStudPlanyTeachMutation, SetStudPlanyTeachMutationVariables } from '../schema'
import { ClassSummaryFragment, UniveristyMemberRole } from '../schema'

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

export interface ISkill {
    code: string
    level?: number
    days?: number
}

export class Skill implements ISkill {
    constructor(data?: ISkill) {
        makeObservable(this)

        if (data) {
            Object.assign(this, data)
        }
    }

    @observable code  = ''
    @observable level?: number = null
    @observable days?: number = null

    @computed get canStudy() {
        return !this.days || this.days < 450
    }
}

export interface SkillGroup {
    skills: Skill[]
}

export class Student {
    constructor(public readonly id: string) {
        makeObservable(this)
    }

    @observable name = ''
    @observable number = 0
    @observable factionName = ''
    @observable factionNumber = 0

    @observable target: Skill = null

    readonly skillsGroups: IObservableArray<SkillGroup> = observable([])
    @computed get skills() {
        const index: { [code: string]: Skill } = { }
        for (const g of this.skillsGroups) {
            for (const s of g.skills) {
                index[s.code] = s
            }
        }

        return index
    }

    readonly teach: IObservableArray<number> = observable([])
    @observable study = ''

    @observable mode: '' | 'target-selection' | 'study' = ''

    @action beginTargetSelection = () => {
        if (this.mode !== '') {
            this.mode = ''
            return
        }

        this.mode = 'target-selection'
    }

    @action beginStudy = () => {
        if (this.mode !== '') {
            this.mode = ''
            return
        }

        this.mode = 'study'
    }

    @action skillClick = (skill: string) => {
        if (this.mode === 'target-selection') {
            this.setTarget(skill, (this.skills[skill].level || 0) + 1)
        }
        else if (this.mode === 'study') {
            this.setStudy(skill)
        }

        this.mode = ''
    }

    @computed get ordersShort() {
        return this.study
            ? this.study
            : this.teach.length
                ? 'TEACH'
                : ''
    }

    @computed get ordersFull() {
        return this.study
            ? `STUDY ${this.study}`
            : this.teach.length
                ? `TEACH ${this.teach.join(' ')}`
                : ''
    }

    setTarget = async (skill: string, level: number) => {
        const result = await CLIENT.mutate<SetStudPlanyTargetMutation, SetStudPlanyTargetMutationVariables>({
            mutation: SetStudPlanyTarget,
            variables: {
                studyPlanId: this.id,
                skill,
                level
            }
        })

        this.setPlan(result.data.setStudyPlanTarget)
    }

    incTargetLevel = (e: React.MouseEvent) => {
        e.preventDefault()
        e.stopPropagation()

        if (!this.target) return

        const currentLevel = this.skills[this.target.code].level || 0

        const nextLevel = Math.max(currentLevel + 1, (this.target.level + 1) % 6)
        this.setTarget(this.target.code, nextLevel)
    }

    setStudy = async (skill: string) => {
        const result = await CLIENT.mutate<SetStudPlanyStudyMutation, SetStudPlanyStudyMutationVariables>({
            mutation: SetStudPlanyStudy,
            variables: {
                studyPlanId: this.id,
                skill
            }
        })

        this.setPlan(result.data.setStudPlanyStudy)
    }

    setTeach = async (units: number[]) => {
        const result = await CLIENT.mutate<SetStudPlanyTeachMutation, SetStudPlanyTeachMutationVariables>({
            mutation: SetStudPlanyTeach,
            variables: {
                studyPlanId: this.id,
                units
            }
        })

        this.setPlan(result.data.setStudyPlanTeach)
    }

    canTeach = (student: Student) => {
        const techerSkil = this.skills[student.study]
        const studentSkill = student.skills[student.study]

        return (techerSkil.level || 0) > (studentSkill.level || 0)
    }

    isTargetSkill(skill: string) {
        return skill === this.target?.code
    }

    @action setPlan(plan: StudyPlanFragment) {
        const unit = plan.unit

        this.name = unit.name
        this.number = unit.number,
        this.factionName = unit.faction.name,
        this.factionNumber = unit.faction.number,
        this.target = plan.target
            ? new Skill({ code: plan.target.code, level: plan.target.level })
            : null
        this.study = plan.study

        const skills = getSkillGroups()
        const skillIndex: { [ code: string ]: Skill } = {}

        for (const g of skills) {
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

        this.skillsGroups.replace(skills)
        this.teach.replace(plan.teach || [])
    }
}

export class StudyLocation {
    constructor(public readonly id: string) {
        makeObservable(this)
    }

    @observable x = 0
    @observable y = 0
    @observable z = 0
    @observable label = ''
    @observable terrain = ''
    @observable province = ''
    @observable settlement?: string = null
    @observable settlementSize?: SettlementSize = null

    readonly students: IObservableArray<Student> = observable([])

    @action setStudents = (list: Student[]) => {
        this.students.replace(list)
    }

    @action addStudent = (student: Student) => {
        this.students.push(student)
    }
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
    readonly members: IObservableArray<UniversityMemberFragment> = observable([])

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

        const members = university?.university.members ?? []
        members.sort((a, b) => a.player.factionNumber - b.player.factionNumber)

        if (player) this.playerId = player.id

        runInAction(() => {
            if (player.university) {
                const { role, university: { id, name } } = university

                this.role = role
                this.id = id
                this.name = name
                this.classes.replace(classes)
                this.members.replace(members)

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

        for (const item of response.data.node.students) {
            const student = new Student(item.id)
            student.setPlan(item)

            const { terrain, x, y, z, label, province, settlement } = item.unit.region
            const locationId = `${x} ${y} ${z}`

            let loc: StudyLocation = locations[locationId]
            if (!loc) {
                loc = new StudyLocation(locationId);
                loc.terrain = terrain
                loc.x = x
                loc.y = y
                loc.z = z
                loc.label = label
                loc.province = province
                loc.settlement = settlement?.name
                loc.settlementSize = settlement?.size

                locations[locationId] = loc
            }

            loc.addStudent(student)
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



function getSkillGroups() {
    return [
        // 'bases'
        {
            skills: [
                new Skill({ code: 'FORC' }),
                new Skill({ code: 'PATT' }),
                new Skill({ code: 'SPIR' }),
            ]
        },
        // 'sruvival'
        {
            skills: [
                new Skill({ code: 'OBSE' }),
                new Skill({ code: 'STEA' }),
            ]
        },
        // 'dragon'
        {
            skills: [
                new Skill({ code: 'EART' }),
                new Skill({ code: 'BIRD' }),
                new Skill({ code: 'WOLF' }),
                new Skill({ code: 'DRAG' }),
            ]
        },
        // 'artifacts'
        {
            skills: [
                new Skill({ code: 'ARTI' }),
                new Skill({ code: 'CRCL' }),
                new Skill({ code: 'CRRI' }),
                new Skill({ code: 'CRTA' }),
                new Skill({ code: 'CRRU' }),
                new Skill({ code: 'CFSW' }),
                // new Skill({ code: 'CRSF' }),
                // new Skill({ code: 'CRWC' }),
            ]
        },
        // enchant
        {
            skills: [
                new Skill({ code: 'ESWO' }),
                new Skill({ code: 'EARM' }),
            ]
        },
        // 'demons'
        {
            skills: [
                new Skill({ code: 'DEMO' }),
                new Skill({ code: 'SUIM' }),
                new Skill({ code: 'SUDE' }),
                new Skill({ code: 'SUBA' }),
            ]
        },
        // 'fire'
        {
            skills: [
                new Skill({ code: 'FIRE' }),
                new Skill({ code: 'FSHI' }),
                new Skill({ code: 'SSHI' }),
                new Skill({ code: 'ESHI' }),
            ]
        },
        // 'illusions'
        {
            skills: [
                new Skill({ code: 'ILLU' }),
                new Skill({ code: 'PHEN' }),
                new Skill({ code: 'INVI' }),
                new Skill({ code: 'PHDDE' }),
                new Skill({ code: 'TRUE' }),
            ]
        },
        // 'necromancy'
        {
            skills: [
                new Skill({ code: 'NECR' }),
                new Skill({ code: 'SBLA' }),
            ]
        },
        // 'other'
        {
            skills: [
                new Skill({ code: 'WEAT' }),
                new Skill({ code: 'CLEA' }),
                new Skill({ code: 'CALL' }),
                new Skill({ code: 'SWIN' }),
                new Skill({ code: 'MHEA' }),
                new Skill({ code: 'FARS' }),
            ]
        },
    ]
}
