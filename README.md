# EzFTL

A simple service for managing and viewing streams provided by
[Janus-FTL](https://github.com/Glimesh/janus-ftl-plugin).

## Running

1.  Copy `appsettings.default.json` -> `appsettings.json` and modify to your preferences.

    Set `"JanusUri"` to the base URL of your Janus instance.

    `"Channels"` allows you to configure a set of Channel IDs and associated stream keys.

2.  `dotnet run`

3.  Fire up [Janus-FTL](https://github.com/Glimesh/janus-ftl-plugin) with environment variables
   configured to point at your EzFTL instance:

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

## Docker Compose

Included is a `docker-compose.yml` file to make firing up an instance of EzFTL along with
[janus-ftl-server](https://github.com/Glimesh/janus-ftl-plugin) easy.

You need to make a few configuration changes before running `docker compose up`:

1.  Update `ezftl.vars` with channel IDs, names, and stream keys that you'd like to be available.

    You need to define `Channels__#__Id`, `Channels__#__Key`, and `Channels__#__Name`
    for each channel, where `#` starts from 0 and increments by 1 for each new channel.

    These values are equivalent to the `appsettings.json` values described above.

2.  Update the `DOCKER_IP` value in `janus.vars` with the **public ip address** of your host
    machine. This is needed for WebRTC ICE negotiations.

This configuration is intended to be used with a reverse-proxy - the EzFTL service
will not be available externally by default. An example reverse proxy set-up is below.

### Nginx Reverse Proxy

Simply add the following to an Nginx site configuration (ex. `/etc/nginx/sites-enabled/ftl`):

```
server {
    listen 80;
    server_name your.hostname.here;

    location / {
        proxy_pass http://localhost:5000;
    }

    location /ws {
        proxy_pass http://localhost:5000;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "Upgrade";
    }

    location /janus {
        proxy_pass http://localhost:8088/janus;
    }
}
```