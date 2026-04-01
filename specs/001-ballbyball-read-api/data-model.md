# Data Model: Cricket Ball-by-Ball Read API

## MatchDocument
- Description: Canonical source document retrieved from Cosmos DB.
- Source shape reference: `schema/info.json` for `info` subtree.
- Key fields:
  - `id` (string, required): Unique match identifier used by detail endpoint.
  - `info` (object, required): Match metadata and lookup/filter fields.
  - `innings` (array, optional in model but expected in detail docs): Ball-by-ball nested events.
- Validation rules:
  - `id` MUST be non-empty.
  - `info` MUST exist for browse eligibility.
- State transitions:
  - Read-only in this service. No mutation lifecycle.

## MatchInfo
- Description: Sub-document under `MatchDocument.info` used for browse filtering and summaries.
- Fields used in v1:
  - `gender` (string)
  - `dates` (array of date strings)
  - `venue` (string)
  - `match_type` (string)
  - `event.name` (string)
  - `teams` (array of two team names)
- Validation rules:
  - Summary-eligible records require: `dates[0]`, `venue`, `event.name`, and at least two `teams` entries.

## BrowseFilter
- Description: API input model for filtered browse endpoint.
- Fields:
  - `gender` (optional string)
  - `fromDate` (optional date)
  - `toDate` (optional date)
  - `venue` (optional string)
  - `matchType` (optional string)
  - `eventName` (optional string)
  - `team` (optional string; matches either team entry)
- Validation rules:
  - At least one filter field MUST be supplied.
  - `fromDate` MUST be <= `toDate` when both supplied.
  - Unknown filter fields MUST fail validation.

## MatchSummary
- Description: Browse response projection from `MatchInfo`.
- Fields:
  - `matchId` (string)
  - `teams` (array[string])
  - `venue` (string)
  - `competition` (string; sourced from `event.name`)
  - `date` (date; sourced from first element in `dates`)
- Validation rules:
  - Max 10 summary objects per response.

## BrowseResponse
- Description: Envelope for browse endpoint.
- Fields:
  - `items` (array[MatchSummary], length 0..10)
  - `hasMore` (bool)
  - `totalMatched` (integer, optional if available efficiently)

## DetailResponse
- Description: Envelope for full document retrieval endpoint.
- Fields:
  - `matchId` (string)
  - `document` (object; raw retrieved match document)

## ApiError
- Description: Standardized frontend-consumable error payload.
- Fields:
  - `code` (string)
  - `message` (string)
  - `correlationId` (string)
  - `details` (object, optional)
- Typical codes:
  - `VALIDATION_ERROR`
  - `NOT_FOUND`
  - `DATA_PROVIDER_UNAVAILABLE`
  - `INTERNAL_ERROR`

## Relationships
- `MatchDocument` 1..1 `MatchInfo`
- `BrowseFilter` applied against `MatchInfo` fields
- `MatchSummary` projected from `MatchDocument` + `MatchInfo`
- `DetailResponse.document` embeds one `MatchDocument`
