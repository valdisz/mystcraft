// this scipt adjusts all terrain srpites to be correct size, and adds height
const sharp = require('sharp')
const glob = require('glob')
const path = require('path')
const fs = require('fs')

function fullTiles(cwd) {
    glob('**/*.png', { cwd }, async (err, files) => {
        for (const f of files) {
            const filePath = path.join(cwd, f)

            const meta = await sharp(filePath).metadata()
            if (meta.width !== 96) {
                continue
            }

            if (meta.height === 136) {
                continue
            }

            await sharp(filePath)
                .resize(meta.width, meta.height + 11, {
                    background: { r: 0, g: 0, b: 0, alpha: 0 },
                    fit: 'contain',
                    position: 'top'
                })
                .toFile(filePath + '.1.png')

            await sharp(filePath + '.1.png')
                .resize(meta.width, 136, {
                    background: { r: 0, g: 0, b: 0, alpha: 0 },
                    fit: 'contain',
                    position: 'bottom'
                })
                .toFile(filePath + '.2.png')

            fs.rmSync(filePath)
            fs.rmSync(filePath + '.1.png')

            await sharp(filePath + '.2.png')
                .composite([{ input: 'src/assets/overlay.png' }])
                .toFile(filePath)

            fs.rmSync(filePath + '.2.png')
        }
    })
}

function jungleTiles(cwd) {
    glob('**/*.png', { cwd }, async (err, files) => {
        for (const f of files) {
            const filePath = path.join(cwd, f)

            const meta = await sharp(filePath).metadata()
            if (meta.width !== 96) {
                continue
            }

            if (meta.height === 136) {
                continue
            }

            await sharp(filePath)
                .resize(meta.width, meta.height + 11, {
                    background: { r: 0, g: 0, b: 0, alpha: 0 },
                    fit: 'contain',
                    position: 'top'
                })
                .toFile(filePath + '.1.png')

            await sharp(filePath + '.1.png')
                .resize(meta.width, 136, {
                    background: { r: 0, g: 0, b: 0, alpha: 0 },
                    fit: 'contain',
                    position: 'bottom'
                })
                .toFile(filePath + '.2.png')

            fs.rmSync(filePath)
            fs.rmSync(filePath + '.1.png')

            await sharp('src/assets/terrain/plain/main/1.png')
                .composite([
                    { input: filePath + '.2.png' },
                    { input: 'src/assets/overlay.png' }
                ])
                .toFile(filePath)

            fs.rmSync(filePath + '.2.png')
        }
    })
}

function transitionTiles(cwd) {
    glob('**/*.png', { cwd }, async (err, files) => {
        for (const f of files) {
            const filePath = path.join(cwd, f)

            const meta = await sharp(filePath).metadata()
            if (meta.width !== 96) {
                continue
            }

            if (meta.height === 136) {
                continue
            }

            await sharp(filePath)
                .resize(meta.width, meta.height + 11, {
                    background: { r: 0, g: 0, b: 0, alpha: 0 },
                    fit: 'contain',
                    position: 'top'
                })
                .toFile(filePath + '.1.png')

            fs.rmSync(filePath)

            await sharp(filePath + '.1.png')
                .resize(meta.width, 136, {
                    background: { r: 0, g: 0, b: 0, alpha: 0 },
                    fit: 'contain',
                    position: 'bottom'
                })
                .toFile(filePath)

            fs.rmSync(filePath + '.1.png')
        }
    })
}

fullTiles('src/assets/terrain/plain/main')
fullTiles('src/assets/terrain/lake/main')
fullTiles('src/assets/terrain/swamp/main')
fullTiles('src/assets/terrain/water/main')
fullTiles('src/assets/terrain/desert/main')
fullTiles('src/assets/terrain/tundra/main')
fullTiles('src/assets/terrain/mountain/main')
fullTiles('src/assets/terrain/volcano/main')
fullTiles('src/assets/terrain/forest/main')
jungleTiles('src/assets/terrain/jungle/main')
