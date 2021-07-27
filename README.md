# Atlantis Economy Advisor

**UNDER DEVELOPMENT**


This is online, multi-user game client and game host for the Atlantis PBEM game. It consists of 2 parts:

1. .Net backend reponsible for the user/player management, data storage and it runs turns by specified schedule
2. Web based game client

## Server

To run game server you will need one of three supported databases: SQLite, PostgreSQL or SQL Server.
SQLite is supported only for development or local, single-user installations.

When server will run first time, it will try to create all needed DB structures and add intial user who will get full privileges.

### Important env variables

When server runs for the first time, it needs to create initial user. For this purpose server will use _once_ such env variables:

* `ADVISOR_SEED__EMAIL` - this will be email/login of the first user
* `ADVISOR_SEED__PASSWORD` - password of the first user


To run server for development needs `ASPNETCORE_ENVIRONMENT` env variable must set with value `Development`.

## Client

Client is built with TypeScript/React and PixiJS. PixiJS is used to draw game map and React is responsible for the UI (with help of
Material UI component library).


## How to run

To run for development execute `dotnet run` or `dotnet watch run` in the `server` folder to start server on port `5000`, and execute
`yanr start` or `npm run start` in the `client` folder to start client on port `1234`. UI development server will create proxy for the
backend.


To run for production the best is to use Docker. For this sake project includes `Dockerfile` that you must build in the project root folder:

```
docker build .
```
