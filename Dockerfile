# +==============================================================
# | compile github-codeql-analytics-cli
# +==============================================================

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine-amd64 as build

WORKDIR /usr/src/github-codeql-analytics
COPY src/GitHub.CodeQL.Analytics.Cli/*.csproj src/GitHub.CodeQL.Analytics.Cli/
COPY src/GitHub.CodeQL.Analytics.Data/*.csproj src/GitHub.CodeQL.Analytics.Data/

RUN dotnet restore src/GitHub.CodeQL.Analytics.Cli/GitHub.CodeQL.Analytics.Cli.csproj

COPY src/GitHub.CodeQL.Analytics.Cli src/GitHub.CodeQL.Analytics.Cli
COPY src/GitHub.CodeQL.Analytics.Data src/GitHub.CodeQL.Analytics.Data

# build

WORKDIR /usr/src/github-codeql-analytics/src/GitHub.CodeQL.Analytics.Cli
RUN dotnet build -c release --no-restore

# +==============================================================
# | publish github-codeql-analytics-cli
# +==============================================================

FROM build AS publish
RUN dotnet publish -c release --no-build -o /app
COPY scripts/launch.sh /app

# +==============================================================
# | package github-codeql-analytics-cli
# +==============================================================

FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine-amd64

ENV TERM xterm-256color

WORKDIR /app
COPY --from=publish /app .
RUN apk --no-cache add bash

ENTRYPOINT ["./launch.sh"]

