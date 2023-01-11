const fs = require('fs')

const s = fs.readFileSync('./battle.json')
const battle = JSON.parse(s)

const SKILLS = {
    'combat': 'COMB',
    'riding': 'RIDI',
    'tactics': 'TACT'
}

console.log('[')
for (const { name, number, skills, items, flags } of battle.attackers) {
    const bu = {
        name: `${name} [${number}]`,
        items: items.map(({ code, amount }) => ({ abbr: code, amount })),
        skills: skills.map(({ name, level }) => ({ abbr: SKILLS[name], level })),
        flags
    }

    console.log(JSON.stringify(bu))
    console.log(',')
}
console.log(']')
