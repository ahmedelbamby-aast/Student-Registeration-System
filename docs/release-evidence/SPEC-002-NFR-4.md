# SPEC-002 NFR-4 Policy Audit Metadata Evidence

**Requirement:** Source URL, access date, approval actor, and effective period MUST be auditable.  
**Measured:** 2026-07-13  
**Configuration:** Release, .NET 10.0.9, x64  
**Automated test:** `tests/StudentRegistration.QualityTests/Specs/Spec002/NFR-4EvidenceTests.cs`

## Method

`Harness_sources_match_every_canonical_policy_source_table_field` independently
reads `specs/002-aastmt-policy-rulebook/policy-sources.md`, locates its exact
seven-column source-table header, parses all 12 rows, rejects duplicate IDs,
and requires the harness to expose the same 12 IDs. It compares eight values
for every row: ID, classification, authority, URL/reference, access date,
affected facts, approval status, and content reference. Under the governed
source schema, content reference is the normalized URL/reference cell; parser
normalization removes only enclosing Markdown code backticks.

The same test requires every source URL/content reference to be HTTPS or a
repository-relative Markdown reference. It also requires each typed source
record to have a nonempty approval actor and a valid effective period.

`Decision_bindings_are_canonical_and_policy_selection_metadata_uses_explicit_sentinels`
evaluates every PB-01 through PB-19 input and inspects every ordered decision
result. Each result source ID must exist in the 12-source harness register, and
the complete bound `PolicySourceRecord` must equal that canonical harness
record.

For the 17 decisions that select `DEMO-POC-2026.1`, the test requires a
nonempty approval actor, non-default effective start, and null/open-ended or
later effective end. PB-18 and PB-19 intentionally fail closed without a
selected policy; they must expose the explicit sentinels `PolicyVersion =
NOT_SELECTED`, `ApprovedBy = Not selected`, a default effective start, null
effective end, null maximum credits, no fallback, and an admin alert. The test
therefore does not misrepresent those two failures as having selected-policy
effective metadata.

## Measured result

| Measure | Value |
|---|---:|
| Canonical source rows parsed | 12 |
| Distinct harness sources matched | 12 |
| Canonical source fields compared | 96 (8 x 12) |
| Decisions evaluated | 19 |
| Selected-policy decisions checked | 17 |
| Fail-closed `NOT_SELECTED` decisions checked | 2 |
| Result-to-source bindings inspected | 189 |
| Canonical field or binding mismatches | 0 |
| Targeted Release tests | 2 passed, 0 failed, 0 skipped |

**Result: PASS.** All canonical provenance fields match the harness exactly,
all 189 decision-result bindings resolve to complete canonical source records,
and selected versus unselected policy metadata is represented honestly.

This verifies the governed SPEC-002 test contract. SPEC-015 remains the owner
of durable historical decision snapshots, and runtime persistence evidence is
required at its release gate.
