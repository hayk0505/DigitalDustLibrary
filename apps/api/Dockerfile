# Dev-oriented image: runs `dotnet watch` for hot reload against a volume-mounted
# source tree (see ../../docker-compose.dev.yml). Not a production image — a real
# deploy Dockerfile (multi-stage build -> runtime-only final image) is a separate
# concern for when this actually ships to the droplet.
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

# dotnet-ef isn't part of the SDK image by default — installing it here means
# migrations can be generated/applied from inside the container, so nothing
# needs installing on the host beyond Docker itself.
RUN dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="$PATH:/root/.dotnet/tools"

# Restore separately first so dependency layers cache even when source changes.
COPY DigitalDustLibrary.Api.csproj ./
RUN dotnet restore

COPY . .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

CMD ["dotnet", "watch", "run", "--no-launch-profile", "--urls", "http://+:8080"]
