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