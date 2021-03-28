const { createProxyMiddleware } = require('http-proxy-middleware');
const Bundler = require('parcel-bundler');
const express = require('express');

const bundler = new Bundler('src/index.html');
const app = express();

app.use('/account', createProxyMiddleware({ target: 'http://localhost:5000', changeOrigin: true }));
app.use('/api', createProxyMiddleware({ target: 'http://localhost:5000', changeOrigin: true }));
app.use('/graphql', createProxyMiddleware({ target: 'http://localhost:5000', changeOrigin: true }));
app.use('/hangfire', createProxyMiddleware({ target: 'http://localhost:5000', changeOrigin: true }));

app.use(bundler.middleware());

app.listen(Number(process.env.PORT || 1234));
