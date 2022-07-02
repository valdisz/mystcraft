
// this script will package sprite sheets

// required ImageMagic installed globaly
const os = require('os')
os.tmpDir = os.tmpdir

const fs = require('fs')
const path = require('path')
const glob = require('glob')

const spritesheet = require('spritesheet-js')

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
function makeSprite(path, name, extension) {
    return {
        index: index++,
        path,
        name,
        extension
    }
}

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
            sprites.push(makeSprite(frame, `${tileName}-main-${i++}`, ''))
        }
    }

    return sprites
}

function addTiles(prefix, sourcePath) {
    const sprites = []

    const frames = glob.sync('*.png', { cwd: sourcePath, absolute: true })
    for (const frame of frames) {
        sprites.push(makeSprite(frame, `${prefix}-${path.basename(frame, path.extname(frame))}`, ''))
    }

    return sprites
}

let sprites = [
    makeSprite('src/assets/silhouette.png', 'silhouette', '')
]

sprites = sprites.concat(addTerrainTiles('src/assets/terrain'))
sprites = sprites.concat(addTiles('map', 'src/assets/map'))
sprites = sprites.concat(addTiles('road', 'src/assets/road'))
sprites = sprites.concat(addTiles('border', 'src/assets/border'))

console.log(`Packing ${sprites.length} sprites...`)
spritesheet(sprites, {
    name: 'sprites',
    format: 'pixi.js',
    padding: 4,
    path: 'static',
    trim: false
},
function (err) {
    if (err) throw err;

    const textureManifest = JSON.parse(fs.readFileSync(`public/sprites.json`))
    fs.writeFileSync(`public/sprites.json`, JSON.stringify({ ...textureManifest, tiles: manifest }, null, 4), { encoding: 'utf-8' })
})
