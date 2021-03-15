FROM node:14 as node-build

WORKDIR /client
COPY client/package.json ./
COPY client/yarn.lock ./
RUN yarn install --ignore-optional
COPY client/ ./
RUN yarn build

####################

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS net-build
WORKDIR /server

COPY server/*.csproj ./
RUN dotnet restore


COPY server/ ./
RUN dotnet publish -c Release -o out

####################

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

WORKDIR /app

COPY --from=net-build /server/out .
COPY --from=node-build /client/dist ./wwwroot

ENTRYPOINT ["dotnet", "advisor.dll"]
