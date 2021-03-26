#!/bin/bash
dotnet ef migrations remove --context SQLiteDatabase -f
dotnet ef migrations remove --no-build --context PgSqlDatabase -f
dotnet ef migrations remove --no-build --context MsSqlDatabase -f
