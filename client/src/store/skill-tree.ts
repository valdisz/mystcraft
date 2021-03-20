export interface ISkill {
    code: string
    level?: number
    days?: number
    title?: string
}

export interface SkillDef {
    title: string
    deps: ISkill[]
}

export const SKILL_TREE: { [ code: string]: SkillDef } = {
    OBSE: {"title": "observation","deps": []},
    STEA: {"title": "stealth","deps": []},
    FORC: {"title": "force","deps": []},
    PATT: {"title": "pattern","deps": []},
    SPIR: {"title": "spirit","deps": []},
    FIRE: {"title": "fire","deps": [{ code: "FORC", level: 1 }]},
    EQUA: {"title": "earthquake","deps": [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }]},
    FSHI: {"title": "force shield","deps": [{ code: "FORC", level: 1 }]},
    ESHI: {"title": "energy shield","deps": [{ code: "FORC", level: 1 }]},
    SSHI: {"title": "spirit shield","deps": [{ code: "SPIR", level: 1 }, { code: "FORC", level: 1 }]},
    MHEA: {"title": "magical healing","deps": [{ code: "PATT", level: 1 }]},
    GATE: {"title": "gate lore","deps": [{ code: "PATT", level: 1 }, { code: "SPIR", level: 1 }]},
    FARS: {"title": "farsight","deps": [{ code: "PATT", level: 1 }, { code: "SPIR", level: 1 }]},
    TELE: {"title": "teleportation","deps": [{ code: "GATE", level: 1 }, { code: "FARS", level: 2 }]},
    PORT: {"title": "portal lore","deps": [{ code: "GATE", level: 2 }, { code: "FARS", level: 1 }]},
    MIND: {"title": "mind reading","deps": [{ code: "PATT", level: 1 }]},
    WEAT: {"title": "weather lore","deps": [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }]},
    SWIN: {"title": "summon wind","deps": [{ code: "WEAT", level: 1 }]},
    SSTO: {"title": "summon storm","deps": [{ code: "WEAT", level: 1 }]},
    STOR: {"title": "summon tornado","deps": [{ code: "WEAT", level: 3 }]},
    CALL: {"title": "call lightning","deps": [{ code: "WEAT", level: 5 }]},
    CLEA: {"title": "clear skies","deps": [{ code: "WEAT", level: 1 }]},
    EART: {"title": "earth lore","deps": [{ code: "PATT", level: 1 }, { code: "FORC", level: 1 }]},
    WOLF: {"title": "wolf lore","deps": [{ code: "EART", level: 1 }]},
    BIRD: {"title": "bird lore","deps": [{ code: "EART", level: 1 }]},
    DRAG: {"title": "dragon lore","deps": [{ code: "BIRD", level: 3 }, { code: "WOLF", level: 3 }]},
    NECR: {"title": "necromancy","deps": [{ code: "FORC", level: 1 }, { code: "SPIR", level: 1 }]},
    SUSK: {"title": "summon skeletons","deps": [{ code: "NECR", level: 1 }]},
    RAIS: {"title": "raise undead","deps": [{ code: "SUSK", level: 3 }]},
    SULI: {"title": "summon lich","deps": [{ code: "RAIS", level: 3 }]},
    FEAR: {"title": "create aura of fear","deps": [{ code: "NECR", level: 1 }]},
    SBLA: {"title": "summon black wind","deps": [{ code: "NECR", level: 5 }]},
    BUND: {"title": "banish undead","deps": [{ code: "NECR", level: 1 }]},
    DEMO: {"title": "demon lore","deps": [{ code: "FORC", level: 1 }, { code: "SPIR", level: 1 }]},
    SUIM: {"title": "summon imps","deps": [{ code: "DEMO", level: 1 }]},
    SUDE: {"title": "summon demon","deps": [{ code: "SUIM", level: 3 }]},
    SUBA: {"title": "summon balrog","deps": [{ code: "SUDE", level: 3 }]},
    BDEM: {"title": "banish demons","deps": [{ code: "DEMO", level: 1 }]},
    ILLU: {"title": "illusion","deps": [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }]},
    PHEN: {"title": "phantasmal entertainment","deps": [{ code: "ILLU", level: 1 }]},
    PHBE: {"title": "create phantasmal beasts","deps": [{ code: "ILLU", level: 1 }]},
    PHUN: {"title": "create phantasmal undead","deps": [{ code: "ILLU", level: 1 }]},
    PHDE: {"title": "create phantasmal demons","deps": [{ code: "ILLU", level: 1 }]},
    INVI: {"title": "invisibility","deps": [{ code: "ILLU", level: 3 }]},
    TRUE: {"title": "true seeing","deps": [{ code: "ILLU", level: 3 }]},
    DISP: {"title": "dispel illusions","deps": [{ code: "ILLU", level: 1 }]},
    ARTI: {"title": "artifact lore","deps": [{ code: "FORC", level: 1 }, { code: "PATT", level: 1 }, { code: "SPIR", level: 1 }]},
    CRRI: {"title": "create ring of invisibility","deps": [{ code: "ARTI", level: 2 }, { code: "INVI", level: 3 }]},
    CRCL: {"title": "create cloak of invulnerability","deps": [{ code: "ARTI", level: 3 }, { code: "FSHI", level: 4 }]},
    CRSF: {"title": "create staff of fire","deps": [{ code: "ARTI", level: 2 }, { code: "FIRE", level: 3 }]},
    CRTA: {"title": "create amulet of true seeing","deps": [{ code: "ARTI", level: 2 }, { code: "TRUE", level: 3 }]},
    CRPA: {"title": "create amulet of protection","deps": [{ code: "ARTI", level: 1 }, { code: "SSHI", level: 3 }]},
    CRRU: {"title": "create runesword","deps": [{ code: "ARTI", level: 2 }, { code: "FEAR", level: 3 }]},
    CRSS: {"title": "create shieldstone","deps": [{ code: "ARTI", level: 1 }, { code: "ESHI", level: 3 }]},
    CRMA: {"title": "create magic carpet","deps": [{ code: "ARTI", level: 1 }, { code: "WEAT", level: 3 }]},
    ENGR: {"title": "engrave runes of warding","deps": [{ code: "ARTI", level: 2 }, { code: "ESHI", level: 3 }, { code: "SSHI", level: 3 }]},
    CGAT: {"title": "construct gate","deps": [{ code: "ARTI", level: 2 }, { code: "GATE", level: 3 }]},
    ESWO: {"title": "enchant swords","deps": [{ code: "ARTI", level: 2 }]},
    EARM: {"title": "enchant armor","deps": [{ code: "ARTI", level: 2 }]},
    ESHD: {"title": "enchant shields","deps": [{ code: "ARTI", level: 1 }]},
    CPOR: {"title": "construct portal","deps": [{ code: "ARTI", level: 1 }, { code: "PORT", level: 2 }]},
    CFSW: {"title": "create flaming sword","deps": [{ code: "ARTI", level: 2 }, { code: "FIRE", level: 3 }]},
    CRAG: {"title": "create aegis","deps": [{ code: "ARTI", level: 2 }, { code: "TRUE", level: 5 }]},
    CRWC: {"title": "create windchime","deps": [{ code: "ARTI", level: 1 }, { code: "SWIN", level: 2 }]},
    CRGC: {"title": "create gate crystal","deps": [{ code: "ARTI", level: 2 }, { code: "GATE", level: 3 }]},
    CRSH: {"title": "create staff of healing","deps": [{ code: "ARTI", level: 2 }, { code: "MHEA", level: 3 }]},
    CRSO: {"title": "create scrying orb","deps": [{ code: "ARTI", level: 2 }, { code: "FARS", level: 3 }]},
    CRCO: {"title": "create cornucopia","deps": [{ code: "ARTI", level: 1 }, { code: "EART", level: 2 }]},
    CRBX: {"title": "create book of exorcism","deps": [{ code: "ARTI", level: 2 }, { code: "BDEM", level: 3 }]},
    CRHS: {"title": "create holy symbol","deps": [{ code: "ARTI", level: 2 }, { code: "BUND", level: 3 }]},
    CRCN: {"title": "create censer of protection","deps": [{ code: "ARTI", level: 2 }, { code: "FSHI", level: 3 }]},
    TRNS: {"title": "transmutation","deps": [{ code: "ARTI", level: 1 }]},
    BRTL: {"title": "blasphemous ritual","deps": [{ code: "PATT", level: 4 }, { code: "FORC", level: 4 }, { code: "SPIR", level: 4 }]}
}

export function getSkillDeps(skill: string): ISkill[] {
    return SKILL_TREE[skill]?.deps ?? []
}

export function getSkillRequirements(code: string, level: number): ISkill[] {
    const deps = { }
    const s = [ ]
    for (const dep of getSkillDeps(code)) {
        s.push(dep.code)
        deps[dep.code] = Math.max(level, dep.level)
    }

    while (s.length) {
        const next = s.pop()

        for (const dep of getSkillDeps(next)) {
            s.push(dep.code)
            deps[dep.code] = Math.max(dep.level, deps[dep.code] || 0, deps[next] || 0)
        }
    }

    return Object.keys(deps).map(code => ({ code, level: deps[code] }))
}
