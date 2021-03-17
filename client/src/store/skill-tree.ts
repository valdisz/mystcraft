export interface ISkill {
    code: string
    level?: number
    days?: number
}

export const SKILL_TREE: { [ code: string]: ISkill[] } = {
    FORC: [],
    PATT: [],
    SPIR: [],

    FIRE: [{ code: "FORC", level: 1 }],
    FSHI: [{ code: "FORC", level: 1 }],
    ESHI: [{ code: "FORC", level: 1 }],
    MHEA: [{ code: "PATT", level: 1 }],

    MIND: [{ code: "PATT", level: 1 }],

    EQUA: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }],
    SSHI: [{ code: "SPIR", level: 1 }, { code: "FORC", level: 1 }],
    GATE: [{ code: "PATT", level: 1 }, { code: "SPIR", level: 1 }],
    FARS: [{ code: "PATT", level: 1 }, { code: "SPIR", level: 1 }],
    TELE: [{ code: "GATE", level: 1 }, { code: "FARS", level: 2 }],
    PORT: [{ code: "GATE", level: 2 }, { code: "FARS", level: 1 }],

    WEAT: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }],
    SWIN: [{ code: "WEAT", level: 1 }],
    CLEA: [{ code: "WEAT", level: 1 }],
    SSTO: [{ code: "WEAT", level: 1 }],
    STOR: [{ code: "WEAT", level: 3 }],
    CALL: [{ code: "WEAT", level: 5 }],

    EART: [{ code: "PATT", level: 1 }, { code: "FORC", level: 1 }],
    WOLF: [{ code: "EART", level: 1 }],
    BIRD: [{ code: "EART", level: 1 }],
    DRAG: [{ code: "BIRD", level: 3 }, { code: "WOLF", level: 3 }],

    NECR: [{ code: "FORC", level: 1 }, { code: "SPIR", level: 1 }],
    BUND: [{ code: "NECR", level: 1 }],
    SUSK: [{ code: "NECR", level: 1 }],
    RAIS: [{ code: "SUSK", level: 3 }],
    SULI: [{ code: "RAIS", level: 3 }],
    FEAR: [{ code: "NECR", level: 1 }],
    SBLA: [{ code: "NECR", level: 5 }],

    DEMO: [{ code: "FORC", level: 1 }, { code: "SPIR", level: 1 }],
    BDEM: [{ code: "DEMO", level: 1 }],
    SUIM: [{ code: "DEMO", level: 1 }],
    SUDE: [{ code: "SUIM", level: 3 }],
    SUBA: [{ code: "SUDE", level: 3 }],

    ILLU: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }],
    DISP: [{ code: "ILLU", level: 1 }],
    PHEN: [{ code: "ILLU", level: 1 }],
    PHBE: [{ code: "ILLU", level: 1 }],
    PHUN: [{ code: "ILLU", level: 1 }],
    PHDE: [{ code: "ILLU", level: 1 }],
    INVI: [{ code: "ILLU", level: 3 }],
    TRUE: [{ code: "ILLU", level: 3 }],

    ARTI: [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }, { code: "SPIR", level: 1 }],
    ESHD: [{ code: "ARTI", level: 1 }],
    TRNS: [{ code: "ARTI", level: 1 }],
    ESWO: [{ code: "ARTI", level: 2 }],
    EARM: [{ code: "ARTI", level: 2 }],

    CRPA: [{ code: "ARTI", level: 1 }, { code: "SSHI", level: 3 }],
    CRSS: [{ code: "ARTI", level: 1 }, { code: "ESHI", level: 3 }],
    CRMA: [{ code: "ARTI", level: 1 }, { code: "WEAT", level: 3 }],
    CPOR: [{ code: "ARTI", level: 1 }, { code: "PORT", level: 2 }],
    CRWC: [{ code: "ARTI", level: 1 }, { code: "SWIN", level: 2 }],
    CRCO: [{ code: "ARTI", level: 1 }, { code: "EART", level: 2 }],

    CRRI: [{ code: "ARTI", level: 2 }, { code: "INVI", level: 3 }],
    CRSF: [{ code: "ARTI", level: 2 }, { code: "FIRE", level: 3 }],
    CRTA: [{ code: "ARTI", level: 2 }, { code: "TRUE", level: 3 }],
    CRRU: [{ code: "ARTI", level: 2 }, { code: "FEAR", level: 3 }],
    CGAT: [{ code: "ARTI", level: 2 }, { code: "GATE", level: 3 }],
    CFSW: [{ code: "ARTI", level: 2 }, { code: "FIRE", level: 3 }],
    CRGC: [{ code: "ARTI", level: 2 }, { code: "GATE", level: 3 }],
    CRSH: [{ code: "ARTI", level: 2 }, { code: "MHEA", level: 3 }],
    CRSO: [{ code: "ARTI", level: 2 }, { code: "FARS", level: 3 }],
    CRBX: [{ code: "ARTI", level: 2 }, { code: "BDEM", level: 3 }],
    CRHS: [{ code: "ARTI", level: 2 }, { code: "BUND", level: 3 }],
    CRCN: [{ code: "ARTI", level: 2 }, { code: "FSHI", level: 3 }],
    CRAG: [{ code: "ARTI", level: 2 }, { code: "TRUE", level: 5 }],
    ENGR: [{ code: "ARTI", level: 2 }, { code: "ESHI", level: 3 }, { code: "SSHI", level: 3 }],

    CRCL: [{ code: "ARTI", level: 3 }, { code: "FSHI", level: 4 }],

    BRTL: [{ code: "PATT", level: 4 }, { code: "FORC", level: 4 }, { code: "SPIR", level: 4 }]
}

export function getSkillRequirements(code: string): ISkill[] {
    const deps = { }
    const s = [ ]
    for (const dep of (SKILL_TREE[code] ?? [])) {
        s.push(dep.code)
        deps[dep.code] = dep.level
    }

    while (s.length) {
        const next = s.pop()

        for (const dep of (SKILL_TREE[next] ?? [])) {
            s.push(dep.code)
            deps[dep.code] = Math.max(dep.level, deps[dep.code] || 0, deps[next] || 0)
        }
    }

    return Object.keys(deps).map(code => ({ code, level: deps[code] }))
}
