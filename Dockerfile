FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./src .

RUN dotnet build WebApi/Phoenix.WebApi.csproj --configuration Release --output /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN apt-get update && apt-get install -y curl=7.88.1-10+deb12u7 supervisor=4.2.5-1 --no-install-recommends && rm -rf /var/lib/apt/lists/*

RUN groupadd -g 10000 dotnet && useradd -u 10000 -g dotnet dotnet && chown -R dotnet:dotnet /app
RUN mkdir -p /var/log/supervisor && touch /var/log/supervisor/supervisord.log && chown -R dotnet:dotnet /var/log/supervisor
RUN chmod -R 777 /var/run
RUN mkdir -p /var/log/Phoenix.WebApi && chmod -R 777 /var/log/Phoenix.WebApi && chown -R dotnet:dotnet /var/log/Phoenix.WebApi

USER dotnet:dotnet

ENV ASPNETCORE_URLS=http://*:5080
EXPOSE 5080

COPY ./supervisord.conf /etc/supervisor/conf.d/supervisord.conf
COPY --chown=dotnet:dotnet --from=build /app/build .

HEALTHCHECK --interval=5s --timeout=10s --retries=3 CMD curl --fail http://localhost:5080/health || exit 1

ENTRYPOINT ["/usr/bin/supervisord", "-n"]
