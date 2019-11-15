const proxy = require('http-proxy-middleware');
const Bundler = require('parcel-bundler');
const express = require('express');

const bundler = new Bundler('src/index.html', {
    // cache: false,
    watch: true
});

const app = express();

// app.use(
//     proxy(
//         [
//             '/api',
//             '/login',
//             '/debug'
//         ],
//         {
//             target: 'http://localhost:5000',
//         })
// );
app.use(express.static('src/assets'));
app.use(bundler.middleware());

app.listen(Number(process.env.PORT || 1234));
