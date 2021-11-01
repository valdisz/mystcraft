const { createProxyMiddleware } = require('http-proxy-middleware')
const serveStatic = require('serve-static')
const path = require('path')

module.exports = function (app) {
  app.use(createProxyMiddleware('/api', { target: 'http://localhost:5000/' }))
  app.use(createProxyMiddleware('/graphql', { target: 'http://localhost:5000/' }))
  app.use(createProxyMiddleware('/hangfire', { target: 'http://localhost:5000/' }))
  app.use(createProxyMiddleware('/account', { target: 'http://localhost:5000/' }))

  app.use(serveStatic(path.join(__dirname, 'static')))
  app.use(serveStatic(path.join(__dirname, 'dist')))
//   app.use('/assets', express.static(path.join(__dirname, 'src/assets')))
};
