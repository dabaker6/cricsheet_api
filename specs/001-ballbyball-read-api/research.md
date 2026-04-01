# Research: Cricket Ball-by-Ball Read API

## Decision 1: Use .NET 8 Minimal API with layered boundaries
- Decision: Implement as ASP.NET Core Minimal API with explicit Application and Infrastructure layers.
- Rationale: Meets minimal API requirement while preserving SOLID boundaries and extension points.
- Alternatives considered: Traditional MVC controllers; rejected because endpoint overhead adds complexity for a small read-only surface.

## Decision 2: Cosmos DB access via provider abstraction
- Decision: Define provider interfaces (`IMatchBrowseProvider`, `IMatchDetailProvider`) and implement Cosmos adapter behind them.
- Rationale: Supports future architecture changes for alternate document stores without API contract changes.
- Alternatives considered: Direct Cosmos SDK calls from endpoints; rejected because this tightly couples transport to storage.

## Decision 3: Browse filters operate on `info` fields from schema sample
- Decision: Browse endpoint filters on `info.gender`, `info.dates`, `info.venue`, `info.match_type`, `info.event.name`, and `info.teams`.
- Rationale: Aligns with user requirements and confirmed sample in `schema/info.json`.
- Alternatives considered: Full text or arbitrary JSON-path search; rejected for v1 due to complexity and weaker contract predictability.

## Decision 4: Browse response returns summary projections, capped at 10
- Decision: Return array of max 10 summary items with teams, venue, competition (event name), and date.
- Rationale: Matches frontend filtered-browse workflow and controls payload size.
- Alternatives considered: Return full documents in browse response; rejected due to higher payload and slower UX iteration.

## Decision 5: Detail endpoint returns full document pass-through
- Decision: Detail endpoint returns the retrieved document unchanged except for envelope metadata.
- Rationale: User requested processing to occur in frontend; API remains read-only retrieval service.
- Alternatives considered: Server-side transformation/normalization; rejected for v1 to avoid unnecessary processing logic.

## Decision 6: Cosmos authentication via managed identity
- Decision: Use managed identity with Azure.Identity (`DefaultAzureCredential`) for Cosmos auth.
- Rationale: Avoids secret storage and aligns with cloud-native security controls.
- Alternatives considered: Connection string/key auth; rejected due to secret management burden.

## Decision 7: Inbound access restricted by network placement
- Decision: Deploy API in same VNet/private network path as frontend and avoid public-only exposure in normal path.
- Rationale: Meets stated inbound authentication/network boundary requirement.
- Alternatives considered: Public API + token-only boundary; rejected for v1 due to stricter network isolation preference.

## Decision 8: Testing strategy is TDD with xUnit
- Decision: Use xUnit unit/integration/contract tests with tests-first workflow.
- Rationale: Required by constitution and user request.
- Alternatives considered: Integration-only approach; rejected due to weaker regression and abstraction safety.
