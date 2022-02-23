import * as React from 'react'
import { makeObservable, observable, IObservableArray, runInAction, action, computed } from 'mobx'
import { CLIENT } from '../client'
import { getSkillDeps, getSkillRequirements, ISkill, SKILL_TREE } from './skill-tree'
import { GetAllianceMages, GetAllianceMagesQuery, GetAllianceMagesQueryVariables } from '../schema'
import { StudyPlanTarget, StudyPlanTargetMutation, StudyPlanTargetMutationVariables } from '../schema'
import { StudyPlanTeach, StudyPlanTeachMutation, StudyPlanTeachMutationVariables } from '../schema'
import { StudyPlanStudy, StudyPlanStudyMutation, StudyPlanStudyMutationVariables } from '../schema'
import { MageFragment, SkillFragment, StudyPlanFragment } from '../schema'
import { World } from '../game'
import { Region } from '../game'

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

    // 'fire, shields and headling'
    yield {
        skills: [
            new Skill({ code: 'FIRE' }),
            new Skill({ code: 'FSHI' }),
            new Skill({ code: 'SSHI' }),
            new Skill({ code: 'ESHI' }),
            new Skill({ code: 'MHEA' }),
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
            new Skill({ code: 'PHBE' }),
            new Skill({ code: 'PHUN' }),
            new Skill({ code: 'DISP' }),
        ]
    }

    // 'artifacts'
    yield {
        skills: [
            new Skill({ code: 'ARTI' }),
            new Skill({ code: 'TRNS' }),
            new Skill({ code: 'ESWO' }),
            new Skill({ code: 'EARM' }),
            new Skill({ code: 'CRCL' }),
            new Skill({ code: 'CRRI' }),
            new Skill({ code: 'CRTA' }),
            new Skill({ code: 'CRRU' }),
            new Skill({ code: 'CFSW' }),
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

    // 'demons'
    yield {
        skills: [
            new Skill({ code: 'DEMO' }),
            new Skill({ code: 'SUIM' }),
            new Skill({ code: 'SUDE' }),
            new Skill({ code: 'SUBA' }),
            new Skill({ code: 'BDEM' }),
        ]
    }

    // 'necromancy'
    yield {
        skills: [
            new Skill({ code: 'NECR' }),
            new Skill({ code: 'SUSK' }),
            new Skill({ code: 'RAIS' }),
            new Skill({ code: 'SULI' }),
            new Skill({ code: 'FEAR' }),
            new Skill({ code: 'SBLA' }),
            new Skill({ code: 'BUND' }),
        ]
    }

    // 'dimensions'
    yield {
        skills: [
            new Skill({ code: 'GATE' }),
            new Skill({ code: 'FARS' }),
            new Skill({ code: 'TELE' }),
            new Skill({ code: 'PORT' }),
        ]
    }

    // 'weather'
    yield {
        skills: [
            new Skill({ code: 'WEAT' }),
            new Skill({ code: 'CLEA' }),
            new Skill({ code: 'SWIN' }),
            new Skill({ code: 'SSTO' }),
            new Skill({ code: 'STOR' }),
            new Skill({ code: 'CALL' }),
        ]
    }

    // 'other'
    yield {
        skills: [
            new Skill({ code: 'EQUA' }),
            new Skill({ code: 'MIND' }),
            new Skill({ code: 'CRSF' }),
            new Skill({ code: 'CRPA' }),
            new Skill({ code: 'CRSS' }),
            new Skill({ code: 'CRMA' }),
            new Skill({ code: 'ENGR' }),
            new Skill({ code: 'CGAT' }),
            new Skill({ code: 'ESHD' }),
            new Skill({ code: 'CPOR' }),
            new Skill({ code: 'CRAG' }),
            new Skill({ code: 'CRWC' }),
            new Skill({ code: 'CRGC' }),
            new Skill({ code: 'CRSH' }),
            new Skill({ code: 'CRSO' }),
            new Skill({ code: 'CRCO' }),
            new Skill({ code: 'CRBX' }),
            new Skill({ code: 'CRHS' }),
            new Skill({ code: 'CRCN' }),
            new Skill({ code: 'BRTL' }),
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

        const result = await CLIENT.mutate<StudyPlanTeachMutation, StudyPlanTeachMutationVariables>({
            mutation: StudyPlanTeach,
            variables: {
                unitId: this.id,
                units: units
            }
        })

        const plan = result.data.studyPlanTeach.studyPlan
        this.setPlan(plan)
        this.setStudents(plan)

        if (this.teach.length >= this.maxStudents) {
            this.resetMode()
        }
    }

    @computed get maxStudents() {
        return Math.min(10, Math.max(0,this.location.students.length - 1))
    }

    removeStudent = async (student: Student) => {
        if (!this.teach.includes(student)) return

        const result = await CLIENT.mutate<StudyPlanTeachMutation, StudyPlanTeachMutationVariables>({
            mutation: StudyPlanTeach,
            variables: {
                unitId: this.id,
                units: this.teach.filter(x => x !== student).map(x => x.number)
            }
        })

        const plan = result.data.studyPlanTeach.studyPlan
        this.setPlan(plan)
        this.setStudents(plan)
    }

    clearStudents = async () => {
        const result = await CLIENT.mutate<StudyPlanTeachMutation, StudyPlanTeachMutationVariables>({
            mutation: StudyPlanTeach,
            variables: {
                unitId: this.id,
                units: []
            }
        })

        const plan = result.data.studyPlanTeach.studyPlan
        this.setPlan(plan)
        this.setStudents(plan)
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

    @action skillClick = async (skill: string) => {
        switch (this.mode) {
            case 'study':
                await this.setStudy(skill)
                this.resetMode()
                if (this.teacher && !this.teacher.canTeach(this, skill)) await this.teacher.removeStudent(this)
                break

            case 'target-selection':
                await this.setTarget(skill, (this.skills[skill].level || 0) + 1)
                this.resetMode()
                break

            case 'teaching':
                break

            case '':
                if (this.location.teacher) {
                    await this.setStudy(skill)
                    await this.location.teacher.addStudent(this)
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
        const result = await CLIENT.mutate<StudyPlanTargetMutation, StudyPlanTargetMutationVariables>({
            mutation: StudyPlanTarget,
            variables: {
                unitId: this.id,
                skill,
                level
            }
        })

        const plan = result.data.studyPlanTarget.studyPlan
        this.setPlan(plan)
        this.setStudents(plan)
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
        const result = await CLIENT.mutate<StudyPlanStudyMutation, StudyPlanStudyMutationVariables>({
            mutation: StudyPlanStudy,
            variables: {
                unitId: this.id,
                skill
            }
        })

        const plan = result.data.studyPlanStudy.studyPlan
        this.setPlan(plan)
        this.setStudents(plan)
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

    setSkills(skills: SkillFragment[]) {
        const grous = Array.from(getSkillGroups())
        const skillIndex: { [ code: string ]: Skill } = {}

        for (const g of grous) {
            for (const s of g.skills) {
                skillIndex[s.code] = s
            }
        }

        for (const { code, level, days } of skills) {
            const s = skillIndex[code]
            if (!s) continue

            s.level = level
            s.days = days
        }

        this.skillsGroups.replace(grous)

    }

    @action setPlan(plan: StudyPlanFragment) {
        if (plan) {
            this.target = plan.target
            ? new Skill({ code: plan.target.code, level: plan.target.level })
            : null
            this.study = plan.study
        }
        else {
            this.target = null
            this.study = null
        }
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

export interface StudyRegion {
    x: number
    y: number
    z: number
    label: string
    terrain: string
    province: string
    settlement: string
    size: string
}

export class StudyLocation {
    constructor(public readonly region: StudyRegion) {
        makeObservable(this)
    }

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
    constructor(private readonly world: World) {
        makeObservable(this)
    }

    @observable loading = true
    @action setLoading = (isLoading: boolean) => this.loading = isLoading

    @observable id: string = null
    @observable name: string = null

    @observable locations: IObservableArray<StudyLocation> = observable([])

    readonly skills = Array.from(getSkillGroups())

    async load(gameId: string, turn: number) {
        runInAction(() => {
            this.loading = true
            this.id = null
            this.name = null
            this.locations.clear()
        })

        const response = await CLIENT.query<GetAllianceMagesQuery, GetAllianceMagesQueryVariables>({
            query: GetAllianceMages,
            variables: { gameId, turn }
        })

        if (response.data.node.__typename !== 'Game') {
            return
        }

        const alliance = response.data.node.me.alliance
        if (!alliance) {
            return
        }

        this.loadMages(alliance.members.flatMap(x => x.turn.units.items))

        runInAction(() => {
            this.loading = false
        })
    }

    loadMages(mages: MageFragment[]) {
        const locations = new Map<string, StudyLocation>()
        const students = new Map<number, Student>()

        const getLocation = (x: number, y: number, z: number) => {
            const region = this.world.getRegion(x, y, z)
            if (!locations.has(region.id)) {
                locations.set(region.id, new StudyLocation({
                    x, y, z,
                    label: region?.coords?.label,
                    terrain: region?.terrain?.name,
                    province: region?.province?.name,
                    settlement: region?.settlement?.name,
                    size: region?.settlement?.size?.toLowerCase()
                }))
            }

            return locations.get(region.id)
        }

        for (const { id, x, y, z, number, name, factionNumber, skills, studyPlan } of mages) {
            let loc = getLocation(x, y, z)

            const student = new Student(id, loc)
            student.name = name
            student.number = number
            student.factionNumber = factionNumber
            student.factionName = this.world.factions.get(factionNumber).name

            student.setSkills(skills)
            student.setPlan(studyPlan)
            loc.addStudent(student)

            students.set(number, student)
        }

        for (const loc of locations.values()) {
            loc.students.sort((a, b) => {
                if (a.factionNumber === b.factionNumber) return a.number - b.number

                return a.factionNumber - b.factionNumber
            })
        }

        for (const { number, studyPlan } of mages) {
            if (!studyPlan) {
                continue
            }

            students.get(number).setStudents(studyPlan)
        }

        runInAction(() => {
            this.locations.replace(Array.from(locations.values()))
        })
    }
}
