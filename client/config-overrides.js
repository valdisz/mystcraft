const {
    override,
    addDecoratorsLegacy,
    disableEsLint,
    disableEsLint,
    overrideDevServer,
    watchAll
} = require("customize-cra");


module.exports = {
    webpack: override(
        // enable legacy decorators babel plugin
        addDecoratorsLegacy(),
    ),
    devServer: overrideDevServer(
        // dev server plugin
        watchAll(),

        // enable legacy decorators babel plugin
        addDecoratorsLegacy(),

        // disable eslint in webpack
        disableEsLint(),
    )
};
