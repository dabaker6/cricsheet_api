# Tasks: Cricket Ball-by-Ball Read API

**Input**: Design documents from `specs/001-ballbyball-read-api/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Setup

- [x] T001 Create .NET solution and projects (`src/Cricsheet.Api`, `tests/Cricsheet.Api.UnitTests`, `tests/Cricsheet.Api.IntegrationTests`, `tests/Cricsheet.Api.ContractTests`).
- [x] T002 Add package references for ASP.NET Core, Azure Cosmos SDK, Azure.Identity, FluentValidation, Serilog, xUnit, FluentAssertions, Moq.
- [x] T003 Configure code quality gates (`dotnet format`, analyzers, nullable context) in CI workflow.
- [x] T004 Add baseline app settings and environment binding for Cosmos endpoint/database/container.

## Phase 2: Foundational

- [x] T005 Define domain contracts and provider interfaces in `src/Cricsheet.Api/Application/Interfaces/`.
- [x] T006 Implement shared API error envelope and correlation middleware in `src/Cricsheet.Api/Contracts/` and `src/Cricsheet.Api/Program.cs`.
- [x] T007 Implement validation models for browse filter and match id in `src/Cricsheet.Api/Validation/`.
- [ ] T008 Implement Cosmos managed-identity client factory in `src/Cricsheet.Api/Infrastructure/Cosmos/`.
- [ ] T009 Implement database provider abstraction wiring in `src/Cricsheet.Api/Infrastructure/Providers/`.

## Phase 3: User Story 1 (P1) - Browse/Filter Matches

- [ ] T010 [US1] Write unit tests for browse filter validation and max-10 capping in `tests/Cricsheet.Api.UnitTests/BrowseFilterTests.cs`.
- [ ] T011 [US1] Write integration tests for browse endpoint with filter combinations in `tests/Cricsheet.Api.IntegrationTests/BrowseEndpointTests.cs`.
- [ ] T012 [US1] Write contract tests for browse response schema in `tests/Cricsheet.Api.ContractTests/BrowseContractTests.cs`.
- [ ] T013 [US1] Implement browse query service with `info`-field filters in `src/Cricsheet.Api/Application/Services/BrowseService.cs`.
- [ ] T014 [US1] Implement summary projection mapping (teams, venue, competition, date) in `src/Cricsheet.Api/Application/Services/SummaryMapper.cs`.
- [ ] T015 [US1] Add browse endpoint in `src/Cricsheet.Api/Endpoints/BrowseEndpoints.cs`.

## Phase 4: User Story 2 (P2) - Retrieve Full Match Detail

- [ ] T016 [US2] Write unit tests for detail retrieval and not-found handling in `tests/Cricsheet.Api.UnitTests/DetailServiceTests.cs`.
- [ ] T017 [US2] Write integration tests for detail endpoint in `tests/Cricsheet.Api.IntegrationTests/DetailEndpointTests.cs`.
- [ ] T018 [US2] Write contract tests for detail response schema in `tests/Cricsheet.Api.ContractTests/DetailContractTests.cs`.
- [ ] T019 [US2] Implement detail retrieval service in `src/Cricsheet.Api/Application/Services/DetailService.cs`.
- [ ] T020 [US2] Add detail endpoint in `src/Cricsheet.Api/Endpoints/DetailEndpoints.cs`.

## Phase 5: User Story 3 (P3) - Frontend-Usable Errors

- [ ] T021 [US3] Write unit tests for validation and provider error mapping in `tests/Cricsheet.Api.UnitTests/ErrorMappingTests.cs`.
- [ ] T022 [US3] Write integration tests for 400/404/503 error contracts in `tests/Cricsheet.Api.IntegrationTests/ErrorResponseTests.cs`.
- [ ] T023 [US3] Write contract tests for `ApiError` schema in `tests/Cricsheet.Api.ContractTests/ErrorContractTests.cs`.
- [ ] T024 [US3] Implement exception-to-error translation and status mapping in `src/Cricsheet.Api/Application/Services/ErrorTranslator.cs`.
- [ ] T025 [US3] Ensure endpoints return structured frontend-consumable errors.

## Phase 6: Security, Cloud, and Polish

- [ ] T026 Add health/readiness endpoints and logging enrichment in `src/Cricsheet.Api/Program.cs`.
- [ ] T027 Add infrastructure notes for VNet-only inbound deployment and managed identity RBAC in deployment docs.
- [ ] T028 Run full test suite and verify TDD evidence in commit history.
- [ ] T029 Validate quickstart scenarios from `specs/001-ballbyball-read-api/quickstart.md`.

## Dependencies

- T001-T009 must complete before story phases.
- US1 tasks (T010-T015) precede US2 and US3 for MVP browse capability.
- US2 depends on foundational provider interfaces (T005-T009).
- US3 depends on endpoint implementations from US1/US2.

## MVP Recommendation

- MVP scope: complete through T015 (browse summary flow) plus baseline structured errors from T006.
