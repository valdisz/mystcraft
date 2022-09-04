const fs = require('fs')

const s = fs.readFileSync('report.json')
const rep = JSON.parse(s)

const hist = {}

for (const reg of rep.regions) {
    if (!reg.settlement) {
        continue
    }

    if (!hist[reg.terrain]) {
        hist[reg.terrain] = {
            city: 0, town: 0, village: 0
        }
    }

    hist[reg.terrain][reg.settlement.size]++

    console.log(`${reg.terrain} ${reg.settlement.size} ${reg.settlement.name}`)
}

console.log(hist)
