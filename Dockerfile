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

FROM mcr.microsoft.com/dotnet/aspnet:5.0

EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS="http://*:8080;https://*:8081"
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ADVISOR_ConnectionStrings__database="Data Source=/usr/var/advisor/advisor.db"

RUN useradd --create-home app

VOLUME /usr/var/advisor
WORKDIR /app

COPY --from=net-build /server/out .
COPY --from=node-build /client/build ./wwwroot
COPY --from=node-build /client/static ./wwwroot

RUN mkdir -p /usr/var/advisor \
    && chown app /usr/var/advisor \
    && chmod u+w /usr/var/advisor \
    && chown app /app \
    && chmod u+r /app

USER app
ENTRYPOINT ["dotnet", "advisor.dll"]
