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
import { getSkillDeps, getSkillRequirements, ISkill, SKILL_TREE } from './skill-tree'

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

export class Skill implements ISkill {
    constructor(data?: ISkill) {
        makeObservable(this)

        if (data) {
            Object.assign(this, data)
        }

        if (data.code) {
            this.title = SKILL_TREE[data.code]?.title ?? ''
        }
    }

    @observable code  = ''
    @observable level?: number = null
    @observable days?: number = null
    @observable title? = ''

    @computed get canStudy() {
        return !this.days || this.days < 450
    }
}

export interface SkillGroup {
    skills: Skill[]
}

function* getSkillGroups() {
    // 'bases'
    yield {
        skills: [
            new Skill({ code: 'FORC' }),
            new Skill({ code: 'PATT' }),
            new Skill({ code: 'SPIR' }),
        ]
    }

    // 'sruvival'
    yield {
        skills: [
            new Skill({ code: 'OBSE' }),
            new Skill({ code: 'STEA' }),
        ]
    }

    // 'dragon'
    yield {
        skills: [
            new Skill({ code: 'EART' }),
            new Skill({ code: 'BIRD' }),
            new Skill({ code: 'WOLF' }),
            new Skill({ code: 'DRAG' }),
        ]
    }

    // 'artifacts'
    yield {
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
    }

    // enchant
    yield {
        skills: [
            new Skill({ code: 'ESWO' }),
            new Skill({ code: 'EARM' }),
        ]
    }

    // 'demons'
    yield {
        skills: [
            new Skill({ code: 'DEMO' }),
            new Skill({ code: 'SUIM' }),
            new Skill({ code: 'SUDE' }),
            new Skill({ code: 'SUBA' }),
        ]
    }

    // 'fire'
    yield {
        skills: [
            new Skill({ code: 'FIRE' }),
            new Skill({ code: 'FSHI' }),
            new Skill({ code: 'SSHI' }),
            new Skill({ code: 'ESHI' }),
        ]
    }

    // 'illusions'
    yield {
        skills: [
            new Skill({ code: 'ILLU' }),
            new Skill({ code: 'TRUE' }),
            new Skill({ code: 'PHEN' }),
            new Skill({ code: 'INVI' }),
            new Skill({ code: 'PHDE' }),
        ]
    }

    // 'necromancy'
    yield {
        skills: [
            new Skill({ code: 'NECR' }),
            new Skill({ code: 'SBLA' }),
        ]
    }

    // 'other'
    yield {
        skills: [
            new Skill({ code: 'WEAT' }),
            new Skill({ code: 'CLEA' }),
            new Skill({ code: 'CALL' }),
            new Skill({ code: 'SWIN' }),
            new Skill({ code: 'MHEA' }),
            new Skill({ code: 'FARS' }),
        ]
    }
}

function levelTodays(level: number) {
    if (level == 1) return 30;

    return levelTodays(level - 1) + 30 * level;
}

export interface StudyTarget extends ISkill {
    effort: number
    isTarget: boolean
}

export type StudentMode = '' | 'target-selection' | 'study' | 'teaching'

export class Student {
    constructor(public readonly id: string, private readonly location: StudyLocation) {
        makeObservable(this)
    }

    @observable name = ''
    @observable number = 0
    @observable factionName = ''
    @observable factionNumber = 0

    @observable target: Skill = null

    @computed get criticalMessage() {
        if (!this.study && !this.teach.length) return 'No orders'

        return null
    }

    @computed get warningMessage() {
        if (this.criticalMessage) return null

        if (!this.target) return 'Target not selected'

        if (this.study) {
            if (!this.depSkills[this.study]) return 'Studying out of target'
        }

        return null
    }

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

    @computed get depSkills() {
        if (!this.target) return { }

        let missing = getSkillRequirements(this.target.code, this.target.level)
        missing.push(this.target)
        missing = missing.filter(x => (this.skills[x.code]?.level ?? 0) < x.level)

        const value = { }
        const level = this.target.level
        for (const skill of missing) {
            const depLevel = Math.max(level, skill.level)
            const studentLevel = this.skills[skill.code]?.level ?? 0
            const delta = depLevel - studentLevel

            if (delta > 0) {
                value[skill.code] = delta
            }
        }

        return value
    }

    getEffort(code: string, level: number) {
        let totalDays = 0

        const deps = getSkillRequirements(code, level)
        deps.push({ code, level, days: levelTodays(level) })

        for (const dep of deps) {
            const currentDays = this.skills[dep.code]?.days ?? 0
            const targetDays = levelTodays(dep.level)

            totalDays += Math.max(0, targetDays - currentDays)
        }

        return Math.ceil(totalDays / 30)
    }

    getMissingLevel(skill: string) {
        return this.depSkills[skill] ?? 0
    }


    isTargetSkill(skill: string) {
        return skill === this.target?.code
    }

    @computed get studyTarget() {
        const targets: { [code: string]: StudyTarget } = {}

        for (const code in this.skills) {
            const skill = this.skills[code]

            const isTarget = this.isTargetSkill(code)
            let level: number
            if (isTarget) {
                level = this.target.level
            }
            else {
                const missingLevel = this.getMissingLevel(code) || 1
                level = Math.min(5, (skill.level ?? 0) + missingLevel)
            }

            const effort = this.getEffort(code, level)

            targets[code] = {
                code,
                level,
                days: levelTodays(level),
                isTarget,
                effort,
                title: skill.title
            }
        }

        return targets
    }

    readonly teach: IObservableArray<Student> = observable([])

    @observable teacher: Student = null
    @action setTeacher = (teacher: Student) => this.teacher = teacher

    addStudent = async (student: Student) => {
        const units = this.teach.map(x => x.number)
        if (units.includes(student.number)) return

        if (student.teacher) {
            await student.teacher.removeStudent(student)
        }

        units.push(student.number)

        const result = await CLIENT.mutate<SetStudPlanyTeachMutation, SetStudPlanyTeachMutationVariables>({
            mutation: SetStudPlanyTeach,
            variables: {
                studyPlanId: this.id,
                units
            }
        })

        this.setPlan(result.data.setStudyPlanTeach)
        this.setStudents(result.data.setStudyPlanTeach)

        if (this.teach.length == 10) {
            this.resetMode()
        }
    }

    removeStudent = async (student: Student) => {
        if (!this.teach.includes(student)) return

        const result = await CLIENT.mutate<SetStudPlanyTeachMutation, SetStudPlanyTeachMutationVariables>({
            mutation: SetStudPlanyTeach,
            variables: {
                studyPlanId: this.id,
                units: this.teach.filter(x => x !== student).map(x => x.number)
            }
        })

        this.setPlan(result.data.setStudyPlanTeach)
        this.setStudents(result.data.setStudyPlanTeach)
    }

    clearStudents = async () => {
        const result = await CLIENT.mutate<SetStudPlanyTeachMutation, SetStudPlanyTeachMutationVariables>({
            mutation: SetStudPlanyTeach,
            variables: {
                studyPlanId: this.id,
                units: []
            }
        })

        this.setPlan(result.data.setStudyPlanTeach)
        this.setStudents(result.data.setStudyPlanTeach)
    }

    canTeach = (student: Student, skill: string) => {
        const techerLevel = this.skills[skill]?.level ?? 0
        const targetLevel = (student.skills[skill]?.level ?? 0) + 1

        if (targetLevel > techerLevel) return false

        const deps = getSkillDeps(skill)
        for (const dep of deps) {
            const depLevel = Math.max(dep.level, targetLevel)
            const studentDepLevel = student.skills[dep.code]?.level ?? 0

            if (studentDepLevel < depLevel) return false
        }

        return true
    }


    @observable study = ''

    @observable mode: StudentMode = ''

    @action beginTargetSelection = () => this.mode = 'target-selection'

    @action beginStudy = () => this.mode = 'study'

    @action beginTeaching = () => {
        if (this.location.teacher === this) {
            this.location.setTeacher(null)
        }
        else {
            this.location.setTeacher(this)
        }

        this.mode = 'teaching'
    }

    @action resetMode = () => {
        if (this.mode === 'teaching') {
            this.location.setTeacher(null)
        }

        this.mode = ''
    }

    @action skillClick = (skill: string) => {
        switch (this.mode) {
            case 'study':
                this.setStudy(skill)
                this.resetMode()
                if (this.teacher) {
                    this.teacher.teach.remove(this)
                    this.setTeacher(null)
                }
                break

            case 'target-selection':
                this.setTarget(skill, (this.skills[skill].level || 0) + 1)
                this.resetMode()
                break

            case 'teaching':
                break

            case '':
                if (this.location.teacher) {
                    this.location.teacher
                        .addStudent(this)
                        .then(() => this.setStudy(skill))
                }
                break;
        }
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
                ? `TEACH ${this.teach.map(x => x.number).join(' ')}`
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
        this.setStudents(result.data.setStudyPlanTarget)
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
        this.setStudents(result.data.setStudPlanyStudy)
    }

    clearOrders = async () => {
        if (this.teach.length) {
            await this.clearStudents()
        }

        if (this.study !== '') {
            if (this.teacher) {
                await this.teacher.removeStudent(this)
            }

            await this.setStudy('')
        }
    }

    canStudy(code: string) {
        const skill = this.skills[code]
        if (!skill.canStudy) return false

        const level = skill.level + 1

        const skillDeps = getSkillDeps(code)
        if (!skillDeps) return true

        for (const dep of skillDeps) {
            const depLevel = Math.max(level, dep.level)
            const depSkill = this.skills[dep.code]

            if ((depSkill?.level || 0) < depLevel) return false;
        }

        return true
    }

    isSkillActive(skill: string) {
        const teacher = this.location.teacher
        const mode = this.mode

        if (mode === 'teaching') return false

        if (teacher && teacher !== this) {
            return teacher !== this.teacher && teacher.canTeach(this, skill)
        }
        else {
            if (!mode) return false

            if (mode === 'target-selection') {
                return this.skills[skill].canStudy
            }

            if (mode === 'study') {
                return this.canStudy(skill)
            }
        }
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

        const skills = Array.from(getSkillGroups())
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
    }

    @action setStudents = (plan: StudyPlanFragment) => {
        const students = (plan.teach || []).map(s => this.location.students.find(x => x.number === s))

        for (const student of this.teach) {
            if (students.includes(student)) continue

            student.setTeacher(null)
        }

        this.teach.replace(students.filter(s => !!s))
        for (const student of this.teach) {
            student.setTeacher(this)
        }
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

    @observable teacher: Student = null
    @action setTeacher = (teacher: Student) => this.teacher = teacher
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

    readonly skills = Array.from(getSkillGroups())

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
        // runInAction(() => this.loading = true)

        const response = await CLIENT.query<GetUniversityClassQuery, GetUniversityClassQueryVariables>({
            query: GetUniversityClass,
            variables: {
                classId
            }
        })

        const locations: { [ id: string]: {
            location: StudyLocation
            data: { [id: string]: StudyPlanFragment }
        } } = { }

        for (const item of response.data.node.students) {
            const { terrain, x, y, z, label, province, settlement } = item.unit.region
            const locationId = `${x} ${y} ${z}`

            let loc = locations[locationId]
            if (!loc) {
                const location = new StudyLocation(locationId);
                location.terrain = terrain
                location.x = x
                location.y = y
                location.z = z
                location.label = label
                location.province = province
                location.settlement = settlement?.name
                location.settlementSize = settlement?.size

                loc = { location, data: { } }
                locations[locationId] = loc
            }

            const student = new Student(item.id, loc.location)
            loc.location.addStudent(student)
            loc.data[item.id] = item
        }

        for (const key in locations) {
            const { location, data } = locations[key]

            for (const student of location.students) {
                student.setPlan(data[student.id])
            }

            for (const student of location.students) {
                student.setStudents(data[student.id])
            }

            location.students.sort((a, b) => {
                if (a.factionNumber === b.factionNumber) return a.number - b.number

                return a.factionNumber - b.factionNumber
            })
        }

        runInAction(() => {
            this.selectedClassId = classId
            this.locations.replace(Object.values(locations).map(x => x.location))
            this.loading = false
        })
    }
}
