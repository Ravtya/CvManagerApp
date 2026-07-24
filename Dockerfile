## Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage Docker layer caching
COPY CvManager.sln ./
COPY src/CvManager.Web/CvManager.Web.csproj src/CvManager.Web/
COPY src/CvManager.Infrastructure/CvManager.Infrastructure.csproj src/CvManager.Infrastructure/
COPY src/CvManager.Application/CvManager.Application.csproj src/CvManager.Application/
COPY src/CvManager.Domain/CvManager.Domain.csproj src/CvManager.Domain/

RUN dotnet restore ./src/CvManager.Web/CvManager.Web.csproj

# Copy everything else and publish
COPY . .
RUN dotnet publish ./src/CvManager.Web/CvManager.Web.csproj -c Release -o /app/publish --no-restore

## Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Render sets PORT; default to 8080 for local runs
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080}
# Avoid Linux inotify limit on small containers (Render etc.)
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CvManager.Web.dll"]
