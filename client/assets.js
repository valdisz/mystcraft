const spritesheet = require('spritesheet-js')

const options = {
    format: 'pixi.js',
    path: 'assets'
}

spritesheet('./src/assets/terrain/advisor/*.png', { ...options, name: 'advisor-terrain' }, function (err) {
    if (err) throw err;

    console.log('Advisor terrain spritesheet... DONE');
})

spritesheet('./src/assets/terrain/fantasy/*.png', { ...options, name: 'fantasy-terrain' }, function (err) {
    if (err) throw err;

    console.log('Fantasy terrain spritesheet... DONE');
})
