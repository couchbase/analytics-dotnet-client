# Docker image for the Couchbase OperationalInsights .NET FIT performer.
#
# Published to ghcr.io/couchbase/insights-dotnet-fit-performer by the
# publish-fit-performer.yml workflow (via couchbaselabs/sdk-docker-build-action).
#
# Build locally from the repository ROOT with:
#   docker build -f fit/Couchbase.OperationalInsights.Performer/Dockerfile -t insights-dotnet-fit-performer .

ARG DOTNET_VERSION=10.0
# Target framework for `dotnet publish`. Kept as a SEPARATE arg from
# DOTNET_VERSION on purpose: the dotnet base images export an ENV named
# DOTNET_VERSION holding the full runtime patch (e.g. 10.0.10), and an inherited
# ENV overrides a Dockerfile ARG of the same name during RUN shell expansion.
# So `-f net${DOTNET_VERSION}` would resolve to `net10.0.10`, which trips
# NETSDK1045 ("does not support targeting .NET 10.0.10"). This arg has no such
# collision, so it expands to the value below.
ARG TARGET_FRAMEWORK=net10.0
# Short SDK name supplied by sdk-docker-build-action (build-arg SDK=insights-dotnet).
# Declared here only so the build arg is consumed and doesn't warn; unused otherwise.
ARG SDK

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env

WORKDIR /src
# Copy the whole repository (SDK + performer) so the performer's
# ProjectReferences to ..\..\src\... and the root Directory.Packages.props resolve.
COPY . .

WORKDIR /src/fit/Couchbase.OperationalInsights.Performer
RUN dotnet restore Couchbase.OperationalInsights.Performer.csproj

FROM build-env AS publish
# Re-declare to make the arg available in this build stage.
ARG TARGET_FRAMEWORK=net10.0
RUN dotnet publish Couchbase.OperationalInsights.Performer.csproj \
    -c Release \
    -f ${TARGET_FRAMEWORK} \
    -o /app/out \
    --no-restore \
    --verbosity quiet

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# FIT tooling overrides this at run time (off|error|warn|info|debug|trace).
ENV LOG_LEVEL="info"

COPY --from=publish /app/out .
ENTRYPOINT ["dotnet", "Couchbase.OperationalInsights.Performer.dll", "disableConsoleRead=true"]
