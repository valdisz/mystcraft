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


To run server for development needs `ASPNETCORE_ENVIRONMENT` env variable must be set with value `Development`.

```
ASPNETCORE_ENVIRONMENT=Development ADVISOR_SEED__EMAIL=valdis@zobela.eu ADVISOR_SEED__PASSWORD=valdis dotnet watch run
```

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

## How to create new game

There are no UI to create game right now, and the only way is to use GraphQL dev tool. Here are steps:

1. Open http://localhost:1234/graphql (UI development proxy must be running)
2. Then execute following query to create new game:

```
mutation {
  gameCreateRemote(
    name: "Test"
    engineVersion: "5.0.0",
    rulesetName: "Test",
    rulesetVersion: "1.0.0",
    options: {
      map: [
        { level: 0, label: "nexus", width: 1, height: 1 },
        { level: 1, label: "surface", width: 64, height: 64 }
      ]
    }
  ) {
    isSuccess
  }
}
```

3. Open or refresh http://localhost:1234 and there should appear a test game which you can join, upload reports, etc.
