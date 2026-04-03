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
- If the database provider is unavailable in local/dev execution, returns structured `DATA_PROVIDER_UNAVAILABLE` with HTTP 503.

## 6. Example detail request
```http
GET /api/v1/matches/{matchId}
```

Expected behavior:
- Returns full source document for selected match id.
- Returns structured `NOT_FOUND` error when missing.
- If the database provider is unavailable in local/dev execution, returns structured `DATA_PROVIDER_UNAVAILABLE` with HTTP 503.

## 7. Cloud-native checks
- Verify health endpoint readiness in deployed environment.
- Verify Cosmos access uses managed identity principal.
- Verify inbound path is only reachable via intended VNet-connected frontend path.

## 8. Deployment notes

### VNet-only inbound access
- Deploy the API behind a private inbound path and keep normal application traffic inside the same VNet or a peered private network as the frontend.
- Do not expose the API as a public internet endpoint for standard production access. If the hosting platform supports public ingress controls, disable public ingress or restrict it to deployment and operational paths only.
- Prefer private endpoints, internal load balancing, or equivalent private-routing features so the frontend reaches the API without traversing the public internet.
- Validate deployment by confirming browse, detail, `/health`, and `/health/ready` are reachable from the intended frontend network path and blocked from unauthorized public paths.

### Managed identity and Cosmos RBAC
- Run the API with a managed identity, either system-assigned or user-assigned, and provide `Cosmos:ManagedIdentityClientId` when a user-assigned identity is used.
- Assign the managed identity a read-only Cosmos data-plane role. Use the minimum scope that supports the application, preferably the target container or database instead of the full account.
- The identity must be able to read documents required by the browse and detail endpoints; it must not be granted write permissions because this API is read-only by design.
- Record the identity principal, assigned scope, and role name in deployment records so access can be audited and re-created consistently.
