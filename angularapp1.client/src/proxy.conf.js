const { env } = require('process');
const target = 'http://localhost:5019';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
    ],
    target: target,
    secure: false,
    changeOrigin: true // 增加這行可以避免一些開發環境的 Domain 問題
  }
]

module.exports = PROXY_CONFIG;
