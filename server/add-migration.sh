#!/bin/bash
dotnet ef migrations add $1 --context SQLiteDatabase --output-dir Migrations/sqlite -- Provider=SQLite
dotnet ef migrations add $1 --no-build --context PgSqlDatabase --output-dir Migrations/pgsql -- Provider=PgSQL
dotnet ef migrations add $1 --no-build --context MsSqlDatabase --output-dir Migrations/mssql -- Provider=MsSQL
