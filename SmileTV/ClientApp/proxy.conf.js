const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:32704';

const target2 = env.ASPNETCORE_HTTPS_PORT ? `wss://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'wss://localhost:32704';

//const target2 = "https://localhost:44448";

console.log(target2);

const PROXY_CONFIG = [
  {
    context: [
      "/hub/chat",
      "/ws/client",
    ],
    "ws": true,
    changeOrigin: false,
    secure: false,
    target: target2,
    //pathRewrite: {
    //  "^/ws/client": ""
    //},
    headers: {
      Connection: 'Keep-Alive'
    }
  },
  {
    context: [
      "/weatherforecast",
      //"/hub/chat",
      "/metrics",
      "/walkthroughapi/getmenu",
      "/walkthroughapi/getvideo",
      //"/walkthroughapi/client",
      "/_configuration",
      "/.well-known",
      "/Identity",
      "/connect",
      "/ApplyDatabaseMigrations",
      "/_framework"
   ],
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
