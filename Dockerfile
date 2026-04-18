# -------- Base runtime image --------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# -------- Build image --------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy ONLY the API project file
COPY src/cricsheet.api/cricsheet.api.csproj src/cricsheet.api/
RUN dotnet restore src/cricsheet.api/cricsheet.api.csproj

# Copy ONLY the API source
COPY src/cricsheet.api/ src/cricsheet.api/
# Publish
RUN dotnet publish src/cricsheet.api/cricsheet.api.csproj -c Release -o /app/publish

# -------- Final image --------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "cricsheet.api.dll"]