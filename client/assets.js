// required ImageMagic installed globaly
const os = require('os')
os.tmpDir = os.tmpdir

const fs = require('fs')

const spritesheet = require('spritesheet-js')

const options = {
    format: 'pixi.js',
    path: 'static',
}

const optionsCss = {
    format: 'css',
    path: 'static'
}

spritesheet('./src/assets/terrain/*.png', { ...options, name: 'terrain' }, function (err) {
    if (err) throw err;

    console.log('Terrain spritesheet... DONE');
})

spritesheet('./src/assets/items/*.png', { ...options, name: 'items' }, function (err) {
    if (err) throw err;

    console.log('Items spritesheet... DONE');
})

spritesheet('./src/assets/objects/*.png', { ...options, name: 'objects' }, function (err) {
    if (err) throw err;

    console.log('Objects spritesheet... DONE');
})

spritesheet('./src/assets/items/*.png', { ...optionsCss, prefix: 'item-', name: 'items-css' }, function (err) {
    if (err) throw err;

    console.log('Items CSS spritesheet... DONE');
})

spritesheet('./src/assets/objects/*.png', { ...optionsCss, prefix: 'object-', name: 'objects-css' }, function (err) {
    if (err) throw err;

    const fname = 'static/objects-css.css'

    const f = fs.readFileSync(fname, 'UTF-8')
    const lines = f.split(/\n/)

    const outf = fs.openSync(fname, 'w+')
    for (const l of lines) {
        if (l.startsWith('.')) {
            const rule = l.match(/^(.+) {$/i)[1]
            if (rule) {
                const fixedRule = rule.replace(/(\s|')+/gi, '-').toLowerCase()
                console.log(`${rule} -> ${fixedRule}`)

                fs.writeFileSync(outf, `${fixedRule} {\n`)
                continue
            }
        }

        fs.writeFileSync(outf, `${l}\n`)
    }
    fs.closeSync(outf)


    console.log('Objects CSS spritesheet... DONE');
})
