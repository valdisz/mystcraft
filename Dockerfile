ARG UID=1001
ARG GID=1001

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

EXPOSE 5000
EXPOSE 5001

ENV UID=${UID}
ENV GID=${GID}
ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ADVISOR_ConnectionStrings__database="Data Source=/usr/var/advisor/advisor.db"
ENV ADVISOR_DataProtection__Path="/usr/var/advisor"

RUN addgroup -g ${GID} --system app \
    && adduser -G app --system -d /app -s /bin/sh -u ${UID} app \
    && chmod u-w /app

WORKDIR /app

COPY --from=net-build /server/out .
COPY --from=node-build /client/build ./wwwroot
COPY --from=node-build /client/static ./wwwroot

RUN mkdir -p /usr/var/advisor \
    && chown app:app /usr/var/advisor \
    && chown -r app:app /app \
    && chmod u+w /usr/var/advisor \
    && chmod u+r /usr/var/advisor

VOLUME /usr/var/advisor

USER app
ENTRYPOINT ["dotnet", "advisor.dll"]
