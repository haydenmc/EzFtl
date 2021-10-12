FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine-amd64 AS build-env
WORKDIR /app

# Install node and npm
RUN apk add --update nodejs npm

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore -r linux-musl-x64

# Copy source and build
COPY ./ ./
RUN dotnet publish \
    -c Release \
    -r linux-musl-x64 \
    --self-contained=true \
    -p:PublishReadyToRun=true \
    --no-restore \
    -o publish

# Runtime image
FROM amd64/alpine:latest
WORKDIR /app
RUN apk add --update libgcc libstdc++ icu-dev
COPY --from=build-env /app/publish .
ENTRYPOINT [ "./EzFtl" ]