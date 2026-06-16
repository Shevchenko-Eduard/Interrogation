FROM mcr.microsoft.com/dotnet/aspnet:10.0.7-alpine3.23 AS runtime

RUN apk add --no-cache \
    curl \
    libc6-compat \
    icu-libs \
    krb5-libs \
    libgcc \
    libintl \
    libssl3 \
    libstdc++ \
    zlib \
    libldap \
    icu-data-full

WORKDIR /app

RUN addgroup -S app-group && adduser -S app-user -G app-group

RUN chown -R app-user:app-group /app

USER app-user

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /program

COPY --parents ./src/**/*.csproj .
COPY --parents ./src/**/*.props .
COPY --parents ./src/**/*.slnx .

RUN dotnet restore ./src/

COPY ./src/ ./src/

RUN dotnet publish ./src/Presentations/API/ -o ./output

FROM runtime AS backend-api

COPY --from=build /program/output ./

ENTRYPOINT ["dotnet", "./API.dll"]