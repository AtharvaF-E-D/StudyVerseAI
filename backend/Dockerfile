# syntax=docker/dockerfile:1

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY Directory.Build.props ./
COPY src/StudyVerse.Domain/StudyVerse.Domain.csproj src/StudyVerse.Domain/
COPY src/StudyVerse.Application/StudyVerse.Application.csproj src/StudyVerse.Application/
COPY src/StudyVerse.Infrastructure/StudyVerse.Infrastructure.csproj src/StudyVerse.Infrastructure/
COPY src/StudyVerse.Api/StudyVerse.Api.csproj src/StudyVerse.Api/
RUN dotnet restore src/StudyVerse.Api/StudyVerse.Api.csproj

# Copy the rest of the source and publish
COPY src/ src/
RUN dotnet publish src/StudyVerse.Api/StudyVerse.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

RUN useradd --uid 5678 --user-group --create-home --shell /usr/sbin/nologin appuser
COPY --from=build /app/publish .
USER appuser

ENTRYPOINT ["dotnet", "StudyVerse.Api.dll"]
