interface Skill {
    code: string
    level?: number
    days?: number
    title?: string
}

interface SkillDef {
    code: string
    title: string
    deps: Skill[]
}

const SKILL_TREE: SkillDef[] = [
    { code: 'OBSE', title: "observation",deps: []},
    { code: 'STEA', title: "stealth",deps: []},
    { code: 'FORC', title: "force",deps: []},
    { code: 'PATT', title: "pattern",deps: []},
    { code: 'SPIR', title: "spirit",deps: []},
    { code: 'FIRE', title: "fire",deps: [{ code: "FORC", level: 1 }]},
    { code: 'EQUA', title: "earthquake",deps: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }]},
    { code: 'FSHI', title: "force shield",deps: [{ code: "FORC", level: 1 }]},
    { code: 'ESHI', title: "energy shield",deps: [{ code: "FORC", level: 1 }]},
    { code: 'SSHI', title: "spirit shield",deps: [{ code: "SPIR", level: 1 }, { code: "FORC", level: 1 }]},
    { code: 'MHEA', title: "magical healing",deps: [{ code: "PATT", level: 1 }]},
    { code: 'GATE', title: "gate lore",deps: [{ code: "PATT", level: 1 }, { code: "SPIR", level: 1 }]},
    { code: 'FARS', title: "farsight",deps: [{ code: "PATT", level: 1 }, { code: "SPIR", level: 1 }]},
    { code: 'TELE', title: "teleportation",deps: [{ code: "GATE", level: 1 }, { code: "FARS", level: 2 }]},
    { code: 'PORT', title: "portal lore",deps: [{ code: "GATE", level: 2 }, { code: "FARS", level: 1 }]},
    { code: 'MIND', title: "mind reading",deps: [{ code: "PATT", level: 1 }]},
    { code: 'WEAT', title: "weather lore",deps: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }]},
    { code: 'SWIN', title: "summon wind",deps: [{ code: "WEAT", level: 1 }]},
    { code: 'SSTO', title: "summon storm",deps: [{ code: "WEAT", level: 1 }]},
    { code: 'STOR', title: "summon tornado",deps: [{ code: "WEAT", level: 3 }]},
    { code: 'CALL', title: "call lightning",deps: [{ code: "WEAT", level: 5 }]},
    { code: 'CLEA', title: "clear skies",deps: [{ code: "WEAT", level: 1 }]},
    { code: 'EART', title: "earth lore",deps: [{ code: "PATT", level: 1 }, { code: "FORC", level: 1 }]},
    { code: 'WOLF', title: "wolf lore",deps: [{ code: "EART", level: 1 }]},
    { code: 'BIRD', title: "bird lore",deps: [{ code: "EART", level: 1 }]},
    { code: 'DRAG', title: "dragon lore",deps: [{ code: "BIRD", level: 3 }, { code: "WOLF", level: 3 }]},
    { code: 'NECR', title: "necromancy",deps: [{ code: "FORC", level: 1 }, { code: "SPIR", level: 1 }]},
    { code: 'SUSK', title: "summon skeletons",deps: [{ code: "NECR", level: 1 }]},
    { code: 'RAIS', title: "raise undead",deps: [{ code: "SUSK", level: 3 }]},
    { code: 'SULI', title: "summon lich",deps: [{ code: "RAIS", level: 3 }]},
    { code: 'FEAR', title: "create aura of fear",deps: [{ code: "NECR", level: 1 }]},
    { code: 'SBLA', title: "summon black wind",deps: [{ code: "NECR", level: 5 }]},
    { code: 'BUND', title: "banish undead",deps: [{ code: "NECR", level: 1 }]},
    { code: 'DEMO', title: "demon lore",deps: [{ code: "FORC", level: 1 }, { code: "SPIR", level: 1 }]},
    { code: 'SUIM', title: "summon imps",deps: [{ code: "DEMO", level: 1 }]},
    { code: 'SUDE', title: "summon demon",deps: [{ code: "SUIM", level: 3 }]},
    { code: 'SUBA', title: "summon balrog",deps: [{ code: "SUDE", level: 3 }]},
    { code: 'BDEM', title: "banish demons",deps: [{ code: "DEMO", level: 1 }]},
    { code: 'ILLU', title: "illusion",deps: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }]},
    { code: 'PHEN', title: "phantasmal entertainment",deps: [{ code: "ILLU", level: 1 }]},
    { code: 'PHBE', title: "create phantasmal beasts",deps: [{ code: "ILLU", level: 1 }]},
    { code: 'PHUN', title: "create phantasmal undead",deps: [{ code: "ILLU", level: 1 }]},
    { code: 'PHDE', title: "create phantasmal demons",deps: [{ code: "ILLU", level: 1 }]},
    { code: 'INVI', title: "invisibility",deps: [{ code: "ILLU", level: 3 }]},
    { code: 'TRUE', title: "true seeing",deps: [{ code: "ILLU", level: 3 }]},
    { code: 'DISP', title: "dispel illusions",deps: [{ code: "ILLU", level: 1 }]},
    { code: 'ARTI', title: "artifact lore",deps: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }, { code: "SPIR", level: 1 }]},
    { code: 'CRRI', title: "create ring of invisibility",deps: [{ code: "ARTI", level: 2 }, { code: "INVI", level: 3 }]},
    { code: 'CRCL', title: "create cloak of invulnerability",deps: [{ code: "ARTI", level: 3 }, { code: "FSHI", level: 4 }]},
    { code: 'CRSF', title: "create staff of fire",deps: [{ code: "ARTI", level: 2 }, { code: "FIRE", level: 3 }]},
    { code: 'CRTA', title: "create amulet of true seeing",deps: [{ code: "ARTI", level: 2 }, { code: "TRUE", level: 3 }]},
    { code: 'CRPA', title: "create amulet of protection",deps: [{ code: "ARTI", level: 1 }, { code: "SSHI", level: 3 }]},
    { code: 'CRRU', title: "create runesword",deps: [{ code: "ARTI", level: 2 }, { code: "FEAR", level: 3 }]},
    { code: 'CRSS', title: "create shieldstone",deps: [{ code: "ARTI", level: 1 }, { code: "ESHI", level: 3 }]},
    { code: 'CRMA', title: "create magic carpet",deps: [{ code: "ARTI", level: 1 }, { code: "WEAT", level: 3 }]},
    { code: 'ENGR', title: "engrave runes of warding",deps: [{ code: "ARTI", level: 2 }, { code: "ESHI", level: 3 }, { code: "SSHI", level: 3 }]},
    { code: 'CGAT', title: "construct gate",deps: [{ code: "ARTI", level: 2 }, { code: "GATE", level: 3 }]},
    { code: 'ESWO', title: "enchant swords",deps: [{ code: "ARTI", level: 2 }]},
    { code: 'EARM', title: "enchant armor",deps: [{ code: "ARTI", level: 2 }]},
    { code: 'ESHD', title: "enchant shields",deps: [{ code: "ARTI", level: 1 }]},
    { code: 'CPOR', title: "construct portal",deps: [{ code: "ARTI", level: 1 }, { code: "PORT", level: 2 }]},
    { code: 'CFSW', title: "create flaming sword",deps: [{ code: "ARTI", level: 2 }, { code: "FIRE", level: 3 }]},
    { code: 'CRAG', title: "create aegis",deps: [{ code: "ARTI", level: 2 }, { code: "TRUE", level: 5 }]},
    { code: 'CRWC', title: "create windchime",deps: [{ code: "ARTI", level: 1 }, { code: "SWIN", level: 2 }]},
    { code: 'CRGC', title: "create gate crystal",deps: [{ code: "ARTI", level: 2 }, { code: "GATE", level: 3 }]},
    { code: 'CRSH', title: "create staff of healing",deps: [{ code: "ARTI", level: 2 }, { code: "MHEA", level: 3 }]},
    { code: 'CRSO', title: "create scrying orb",deps: [{ code: "ARTI", level: 2 }, { code: "FARS", level: 3 }]},
    { code: 'CRCO', title: "create cornucopia",deps: [{ code: "ARTI", level: 1 }, { code: "EART", level: 2 }]},
    { code: 'CRBX', title: "create book of exorcism",deps: [{ code: "ARTI", level: 2 }, { code: "BDEM", level: 3 }]},
    { code: 'CRHS', title: "create holy symbol",deps: [{ code: "ARTI", level: 2 }, { code: "BUND", level: 3 }]},
    { code: 'CRCN', title: "create censer of protection",deps: [{ code: "ARTI", level: 2 }, { code: "FSHI", level: 3 }]},
    { code: 'TRNS', title: "transmutation",deps: [{ code: "ARTI", level: 1 }]}
]

function getSkillDeps(skill: string): Skill[] {
    return SKILL_TREE.find(s => s.code === skill)?.deps || []
}

// deps must be known first, and deps of deps, etc.
// to reach level, deps must be known at level or higher
function getLearningPath(skills: { code: string, level: number }[]): Skill[] {
    const path: Skill[] = []

    function skillPath(code: string, level: number) {
        const deps = getSkillDeps(code)
        for (const dep of deps) {
            const lvl = Math.max(dep.level || 0, level)
            for (let i = 1; i <= lvl; i++) {
                skillPath(dep.code, i)
            }
        }

        for (let i = path.length - 1; i >= 0; i--) {
            const skill = path[i]
            if (skill.code === code && skill.level! >= level) {
                return
            }
        }

        path.push({ code, level })
    }

    for (const s of skills) {
        for (let i = 1; i <= s.level; i++) {
            skillPath(s.code, i)
        }
    }

    return path
}

function levelTodays(level: number) {
    if (level < 1) {
        return 0
    }

    return 15 * level * (level + 1)
}

function daysToLevel(days: number) {
    if (days < 30) return 0
    if (days < 90) return 1
    if (days < 180) return 2
    if (days < 300) return 3
    if (days < 450) return 4
    if (days < 630) return 5

    return Math.trunc((Math.sqrt(60 * days + 225) - 15) / 30)
}

type MageSkills = { code: string, days: number }[]

interface Mage {
    num: number
    name: string
    skills: MageSkills
}

interface State {
    mages: Mage[]
}

interface ActionStudy {
    type: 'study'
    num: number
    skill: string
}

interface ActionTeach {
    type: 'teach'
    num: number
    units: number[]
}

interface ActionNoop {
    type: 'noop'
    num: number
}

type Action = ActionStudy | ActionTeach | ActionNoop

function reduce(state: State, actions: Action[]): State {
    const newState = {
        mages: state.mages.map(mage => ({ ...mage, skills: [...mage.skills.map(s => ({...s}))] }))
    }

    for (const action of actions) {
        switch (action.type) {
            case 'study': {
                const mage = newState.mages.find(m => m.num === action.num)!
                const skill = mage.skills.find(s => s.code === action.skill)

                if (skill) {
                    skill.days += 30
                } else {
                    mage.skills.push({ code: action.skill, days: 30 })
                }

                break
            }

            case 'teach': {
                const mage = newState.mages.find(m => m.num === action.num)!
                for (const unitNum of action.units) {
                    const student = newState.mages.find(m => m.num === unitNum)!
                    const study = actions.find(x => x.num === unitNum && x.type === 'study' && canTeach(mage.skills, student.skills, x.skill)) as ActionStudy

                    if (study) {
                        const skill = student.skills.find(s => s.code === study.skill)

                        if (skill) {
                            skill.days += 30
                        } else {
                            student.skills.push({ code: study.skill, days: 30 })
                        }
                    }
                }

                break
            }

            case 'noop':
            default: {
                break
            }
        }
    }

    return newState
}


interface TargetSkill {
    code: string
    level: number
    value: number
}

interface Target {
    num: number
    skills: TargetSkill[]
}

function canStudy(skills: MageSkills): string[] {
    const list: string[] = []

    for (const skill of SKILL_TREE) {
        const skillLevel = daysToLevel(skills[skill.code] || 0)
        if (skillLevel >= 5) {
            // mage knows this skill at max level
            continue
        }

        const nextLevel = skillLevel + 1

        // if all deps are known at level >= nextLevel or minimum dep level, then this skill can be studied
        if (skill.deps.every(dep => daysToLevel(skills[dep.code] || 0) >= Math.max(dep.level || 0, nextLevel))) {
            list.push(skill.code)
        }
    }

    return list
}

function canTeach(teacherSkills: MageSkills, studentSkills: MageSkills, skill?: string): boolean {
    if (skill) {
        return daysToLevel(teacherSkills.find(s => s.code === skill)?.days || 0) > daysToLevel(studentSkills.find(s => s.code === skill)?.days || 0)
    }

    for (const teacherSkill of teacherSkills) {
        const teacherLevel = daysToLevel(teacherSkill.days)
        const studentLevel = daysToLevel(studentSkills.find(s => s.code === teacherSkill.code)?.days || 0)

        if (teacherLevel > studentLevel) {
            return true
        }
    }

    return false
}

function cartesian(...sets) {
    return sets.reduce((acc, set) => acc.flatMap((x) => set.map((y) => [...x, y])), [[]])
}

function plan(state: State, depth: number, scorer: (s: State) => number, path: Action[][]) {
    if (depth > 0) {
        const { mages } = state
        // possible actions of each mage
        const moves = mages.map(mage => [
            { type: 'noop', num: mage.num },
            ...canStudy(mage.skills).map(skill => ({ type: 'study', num: mage.num, skill } as ActionStudy)),
            ...(
                [
                    {
                        type: 'teach',
                        num: mage.num,
                        units: mages.filter(m => m.num !== mage.num && canTeach(mage.skills, m.skills)).map(m => m.num)
                    } as ActionTeach
                ].filter(action => action.units.length > 0)
            )
        ] as Action[])

        // determine potential next states
        const S: { actions: Action[], value: number, state: State, path?: Action[][] }[] = []

        for (const actions of cartesian(...moves)) {
            const nextState = reduce(state, actions)
            const value = scorer(nextState)

            if (value > 0) {
                S.push({ actions, value, state: nextState })
            }
        }

        S.sort((a, b) => b.value - a.value)
        S.length = Math.trunc(S.length / 2)

        for (const s of S) {
            const p = [...path]
            s.value = plan(s.state, depth - 1, scorer, p)
            s.path = p
        }

        S.sort((a, b) => b.value - a.value)
        path.push(S[0].actions)
    }

    return scorer(state)
}

function totalTurns(path: Skill[]) {
    const turns = path.reduce((accum, skill) => accum + (skill.level || 0), 0)
    console.log(`Needs ${turns} turns`)

    return path
}

const type1 = totalTurns(getLearningPath([
    { code: 'FIRE', level: 1 },
    { code: 'PHEN', level: 1 },
    { code: 'CFSW', level: 2 },
    { code: 'SBLA', level: 3 },
]))
console.log('CFSW', type1)

const type2 = totalTurns(getLearningPath([
    { code: 'FIRE', level: 1 },
    { code: 'PHEN', level: 1 },
    // { code: 'OBSE', level: 5 },
    { code: 'TRUE', level: 3 },
    { code: 'SBLA', level: 3 },
]))
console.log('TRUE', type2)

const type3 = totalTurns(getLearningPath([
    { code: 'FIRE', level: 1 },
    { code: 'PHEN', level: 1 },
    { code: 'GATE', level: 3 },
    { code: 'SBLA', level: 3 },
]))
console.log('GATE', type3)

const targetSkills = new Set<string>([type1, type2, type3].flat().map(s => s.code))

function score(state: State): number {
    const totalDays = state.mages.map(x => x.skills).flat().reduce((accum, skill) => accum + skill.days, 0)
    const totalLevels = state.mages.map(x => x.skills).flat().reduce((accum, skill) => accum + daysToLevel(skill.days), 0) * 10
    const totalTargetValue = state.mages.map(mage => mage.skills.filter(s => targetSkills.has(s.code)).map(s => s.days * 10)).flat().reduce((accum, skill) => accum + skill, 0)

    return totalTargetValue + totalDays + totalLevels
}

// const studyPlan = []
// plan({
//     mages: [
//         { name: 'A', num: 1, skills: [] },
//         { name: 'B', num: 2, skills: [] },
//         { name: 'C', num: 3, skills: [] },
//         // { name: 'D', num: 4, skills: [] },
//     ]
// }, 3, score, studyPlan)

// console.log('Plan', studyPlan)
