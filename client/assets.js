
// this script will package sprite sheets

// required ImageMagic installed globaly
const os = require('os')
os.tmpDir = os.tmpdir

const fs = require('fs')
const path = require('path')
const glob = require('glob')

const spritesheet = require('../lib/spritesheet.js')

const options = {
    format: 'pixi.js',
    path: 'static',
    padding: 4
}

const optionsCss = {
    format: 'css',
    path: 'static'
}


const basePath = 'static'
const terrainPath = path.join(basePath, 'terrain')

function isFolder(p) {
    return fs.lstatSync(p).isDirectory()
}

function isFile(p) {
    return fs.lstatSync(p).isFile()
}

function listFiles(p) {
    return fs.readdirSync(p)
        .map(x => path.join(p, x))
        .filter(x => isFile(x))
}

function listFolders(p) {
    return fs.readdirSync(p)
        .map(x => path.join(p, x))
        .filter(x => isFolder(x))
}

const manifest = { }
let index = 0

function addTerrainTiles(sourcePath) {
    const sprites = []

    for (const tilePath of listFolders(sourcePath)) {
        const tileName = path.basename(tilePath)

        const frames = glob.sync('*.png', { cwd: path.join(tilePath, 'main'), absolute: true })
        manifest[tileName] = {
            main: frames.length
        }

        let i = 0
        for (const frame of frames) {
            sprites.push({
                index: index++,
                path: frame,
                name: `${tileName}-main-${i++}`,
                extension: ''
            })
        }
    }

    return sprites
}

function addTiles(prefix, sourcePath) {
    const sprites = []

    const frames = glob.sync('*.png', { cwd: sourcePath, absolute: true })
    for (const frame of frames) {
        sprites.push({
            index: index++,
            path: frame,
            name: `${prefix}-${path.basename(frame, path.extname(frame))}`,
            extension: ''
        })
    }

    return sprites
}

let sprites = [ ]
sprites = sprites.concat(addTerrainTiles('src/assets/terrain'))
sprites = sprites.concat(addTiles('map', 'src/assets/map'))
sprites = sprites.concat(addTiles('road', 'src/assets/road'))

spritesheet(sprites, {
    name: 'sprites',
    format: 'pixi.js',
    padding: 4,
    path: 'static',
    trim: false,
    manual: true
},
function (err) {
    if (err) throw err;

    const textureManifest = JSON.parse(fs.readFileSync(`static/sprites.json`))
    fs.writeFileSync(`static/sprites.json`, JSON.stringify({ ...textureManifest, tiles: manifest }, null, 4), { encoding: 'utf-8' })
})

// processTileset('road', { trim: false, padding: 10 }, 'src/assets/road', 'static')

// spritesheet('./src/assets/terrain/*.png', { ...options, trim: false, name: 'terrain' }, function (err) {
//     if (err) throw err;

//     console.log('Terrain spritesheet... DONE');
// })

// spritesheet('./src/assets/items/*.png', { ...options, name: 'items' }, function (err) {
//     if (err) throw err;

//     console.log('Items spritesheet... DONE');
// })

// spritesheet('./src/assets/objects/*.png', { ...options, name: 'objects' }, function (err) {
//     if (err) throw err;

//     console.log('Objects spritesheet... DONE');
// })

// spritesheet('./src/assets/map/*.png', { ...options, name: 'map' }, function (err) {
//     if (err) throw err;

//     console.log('Map features spritesheet... DONE');
// })

// spritesheet('./src/assets/items/*.png', { ...optionsCss, prefix: 'item-', name: 'items-css' }, function (err) {
//     if (err) throw err;

//     console.log('Items CSS spritesheet... DONE');
// })

// spritesheet('./src/assets/objects/*.png', { ...optionsCss, prefix: 'object-', name: 'objects-css' }, function (err) {
//     if (err) throw err;

//     const fname = 'static/objects-css.css'

//     const f = fs.readFileSync(fname, 'UTF-8')
//     const lines = f.split(/\n/)

//     const outf = fs.openSync(fname, 'w+')
//     for (const l of lines) {
//         if (l.startsWith('.')) {
//             const rule = l.match(/^(.+) {$/i)[1]
//             if (rule) {
//                 const fixedRule = rule.replace(/(\s|')+/gi, '-').toLowerCase()
//                 console.log(`${rule} -> ${fixedRule}`)

//                 fs.writeFileSync(outf, `${fixedRule} {\n`)
//                 continue
//             }
//         }

//         fs.writeFileSync(outf, `${l}\n`)
//     }
//     fs.closeSync(outf)


//     console.log('Objects CSS spritesheet... DONE');
// })
