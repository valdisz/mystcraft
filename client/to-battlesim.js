const fs = require('fs')

const s = fs.readFileSync('battle.json')
const json = JSON.parse(s)

const index = new Map()
const stats = {
    fontline: {
        men: 0,
        items: {},
        skills: {},
        item_classes: {}
    },
    backline: {
        men: 0,
        items: {},
        skills: {},
        item_classes: {}
    },
    total: {
        men: 0,
        items: {},
        skills: {},
        item_classes: {}
    },
    factions: {

    }
}

const sim = {
    attackers: {
        units: []
    },
    defenders: {
        units: []
    }
}

const SKILLS = {
    'combat': 'COMB',
    'riding': 'RIDI',
    'longbow': 'LBOW',
    'tactics': 'TACT',
    'crossbow': 'XBOW'
}

const MEN = [
    'LEAD',
    'WELF',
    'ORC',
    'GNOL',
    'GBLN',
    'GNOM',
    'HELF',
    'IDWA',
    'LIZA',
    'CTAU',
    'HUMN'
]

const CLASS = {
    'LEAD': 'men',
    'ASWR': 'weapon',
    'AARM': 'armor',
    'WING': 'mount',
    'MSHD': 'shield',
    'RING': 'magic item',
    'STAF': 'magic weapon',
    'WELF': 'men',
    'LBOW': 'ranged weapon',
    'STED': 'fmi',
    'MSWO': 'weapon',
    'MARM': 'armor',
    'ISHD': 'shield',
    'HORS': 'mount',
    'CLOA': 'armor',
    'ORC': 'men',
    'CAME': 'mount',
    'EAGL': 'monster',
    'LARM': 'armor',
    'DRAG': 'monster',
    'GNOL': 'men',
    'SWOR': 'weapon',
    'TURT': 'mount',
    'PARM': 'armor',
    'GBLN': 'men',
    'MXBO': 'ranged weapon',
    'DBOW': 'ranged weapon',
    'GNOM': 'men',
    'HELF': 'men',
    'ARNG': 'armor',
    'XBOW': 'ranged weapon',
    'WSHD': 'shield',
    'JAVE': 'ranged weapon',
    'IDWA': 'men',
    'FSWO': 'weapon',
    'CATP': 'fmi',
    'SPEA': 'weapon',
    'LIZA': 'men',
    'BALR': 'monster',
    'CTAU': 'men',
    'HLYS': 'magic item',
    'SHST': 'magic item',
    'CNSR': 'magic item',
    'DEMO': 'monster',
    'LICH': 'monster',
    'BKEX': 'magic item',
    'AMPR': 'armor',
    'HAMM': 'tool',
    'PICK': 'tool',
    'SKEL': 'monster',
    'UNDE': 'monster',
    'HUMN': 'men',
    'CARM': 'armor'
}

function applyStats(s, u) {
    let men = 0
    for (const item of u.items) {
        if (MEN.includes(item.code)) {
            men += item.amount
        }

        if (s.items[item.code]) {
            s.items[item.code] += item.amount
        }
        else {
            s.items[item.code] = item.amount
        }

        const c = CLASS[item.code]
        if (s.item_classes[c]) {
            s.item_classes[c] += item.amount
        }
        else {
            s.item_classes[c] = item.amount
        }
    }

    s.men += men

    for (const skill of u.skills) {
        const code = SKILLS[skill.name]

        let sk = s.skills[code]
        if (!sk) {
            sk = {}
            s.skills[code] = sk
        }

        if (sk[skill.level]) {
            sk[skill.level] += men
        }
        else {
            sk[skill.level] = men
        }
    }
}

for (const battle of json.battles) {
    if (battle.location.province === 'Swerthenchi' || battle.location.province === 'Osdhan') {
        for (const unit of battle.attackers) {
            if (!index.has(unit.number)) {
                index.set(unit.number, unit)

                const faction = unit.faction
                    ? `${unit.faction.name} (${unit.faction.number})`
                    : `Unknown`

                if (!stats.factions[faction]) {
                    stats.factions[faction] = {
                        men: 0,
                        items: {},
                        skills: {},
                        item_classes: {}
                    }
                }

                applyStats(stats.total, unit)
                applyStats(unit.flags.includes('behind') ? stats.backline : stats.fontline, unit)
                applyStats(stats.factions[faction], unit)
            }
        }
    }
}

for (const unit of index.values()) {
    sim.attackers.units.push({
        name: `[${unit.number}] ${unit.name}`,
        items: unit.items.map(i => ({ abbr:  i.code, amount: i.amount })),
        skills: unit.skills.map(s => ({ abbr: SKILLS[s.name], level: s.level })),
        flags: unit.flags
    })
}

fs.writeFileSync('local-battles.json', JSON.stringify(sim, null, 4));
fs.writeFileSync('stats.json', JSON.stringify(stats, null, 4));
