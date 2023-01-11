FROM node:14 as node-build

WORKDIR /client
COPY client/package.json ./
COPY client/yarn.lock ./
RUN yarn install --ignore-optional
COPY client/ ./
RUN yarn build

####################

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS net-build
WORKDIR /server

COPY server/*.csproj ./
RUN dotnet restore

COPY server/ ./
RUN dotnet publish -c Release -o out

####################

FROM mcr.microsoft.com/dotnet/aspnet:6.0

EXPOSE 5000

RUN apt-get update && apt-get --yes install curl
HEALTHCHECK --start-period=10s --interval=5s --timeout=30s CMD curl --silent --fail http://localhost:5000/system/ping || exit 1

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ADVISOR_ConnectionStrings__database="Data Source=/usr/var/advisor/advisor.db"
ENV ADVISOR_ConnectionStrings__hangfire="/usr/var/advisor/advisor-hangfire.db"
ENV ADVISOR_DataProtection__Path="/usr/var/advisor"
ENV ADVISOR_SEED__EMAIL="gm@advisor"
ENV ADVISOR_SEED__PASSWORD="1234"

RUN groupadd --gid 1001 app \
    && useradd --gid 1001 --create-home --home /app --shell /bin/sh --uid 1001 app \
    && chmod u-w /app

WORKDIR /app

COPY --from=net-build /server/out .
COPY --from=node-build /client/build ./wwwroot

RUN mkdir -p /usr/var/advisor \
    && chown app:app /usr/var/advisor \
    && chmod u+w /usr/var/advisor \
    && chmod u+r /usr/var/advisor

VOLUME /usr/var/advisor

USER app
ENTRYPOINT ["dotnet", "server.dll"]
