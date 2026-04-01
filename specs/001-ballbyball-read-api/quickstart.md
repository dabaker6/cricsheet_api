# Quickstart: Cricket Ball-by-Ball Read API

## Prerequisites
- .NET 8 SDK
- Azure account with managed identity-enabled runtime
- Cosmos DB account/container containing cricket match documents

## 1. Restore and build
```bash
dotnet restore
dotnet build
```

## 2. Configure runtime settings
Set environment variables (example names):
- `COSMOS_ACCOUNT_ENDPOINT`
- `COSMOS_DATABASE_NAME`
- `COSMOS_CONTAINER_NAME`
- `ASPNETCORE_ENVIRONMENT`

No secret key is required for production runtime when managed identity is available.

## 3. Run tests (TDD gates)
```bash
dotnet test tests/Cricsheet.Api.UnitTests
dotnet test tests/Cricsheet.Api.IntegrationTests
dotnet test tests/Cricsheet.Api.ContractTests
```

## 4. Run API locally
```bash
dotnet run --project src/Cricsheet.Api
```

## 5. Example browse request
```http
GET /api/v1/matches/browse?gender=male&fromDate=2016-01-01&toDate=2016-12-31&venue=Gros%20Islet&matchType=T20&eventName=Caribbean%20Premier%20League&team=Trinbago%20Knight%20Riders
```

Expected behavior:
- Returns 0..10 summary entries.
- Each summary includes teams, venue, competition, date.

## 6. Example detail request
```http
GET /api/v1/matches/{matchId}
```

Expected behavior:
- Returns full source document for selected match id.
- Returns structured `NOT_FOUND` error when missing.

## 7. Cloud-native checks
- Verify health endpoint readiness in deployed environment.
- Verify Cosmos access uses managed identity principal.
- Verify inbound path is only reachable via intended VNet-connected frontend path.
