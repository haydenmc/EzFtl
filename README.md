# EzFTL

A simple service for managing and viewing streams provided by [Janus-FTL](https://github.com/Glimesh/janus-ftl-plugin).

## Running

1. Copy `appsettings.default.json` -> `appsettings.json` and modify to your preferences.

   Set `"JanusUri"` to the base URL of your Janus instance.

   `"Channels"` allows you to configure a set of Channel IDs and associated stream keys.

2. `dotnet run`

3. Fire up [Janus-FTL](https://github.com/Glimesh/janus-ftl-plugin) with environment variables configured to point at your EzFTL instance:
```sh
export FTL_SERVICE_CONNECTION=REST
export FTL_SERVICE_REST_HOSTNAME=localhost
export FTL_SERVICE_REST_PORT=5000
export FTL_SERVICE_REST_HTTPS=0
export FTL_SERVICE_REST_PATH_BASE=api
/opt/janus/bin/janus
```

## Docker

You can use the included `Dockerfile` to build a container image for convenience:

```sh
docker build -t ezftl .
```

Here's an example of running the EzFTL image on port 5000 accessible only via loopback
with a single channel configured:

```sh
docker run --rm \
    -p 127.0.0.1:5000:80 \
    -e ASPNETCORE_URLS=http://+:80 \
    -e Channels__0__Id=1 \
    -e Channels__0__Key="aBcDeFgHiJkLmNoPqRsTuVwXyZ123456" \
    -e Channels__0__Name="My First Stream" \
    --name ezftl \
    ezftl
```
