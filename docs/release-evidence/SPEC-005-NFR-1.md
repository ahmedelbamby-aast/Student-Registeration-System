# SPEC-005 NFR-1 Critical Query Evidence

**Requirement:** SPEC-005/NFR-1  
**Recorded:** 2026-07-19  
**Owner:** Ahmed ELbamby  
**Scope:** non-production production-like POC fixture

The approved inventory is `docs/data/critical-query-inventory.md`. The
executable SQL Server 2022 Developer compatibility-160 rehearsal migrated the
canonical model and loaded 25,000 accounts, 25,000 students, 8,000 plans,
25,000 submissions, 75,000 enrollments, and 100,000 audit events using
synthetic-only data.

Actual ShowPlan XML was captured for all seven critical-query families. The
review record `SPEC-005-NFR-1-plans.json` contains a SHA-256 hash of each exact
actual plan, its physical operators and indexes, the measured 15-run p95 after
three warmups, row counts, and exception metadata. Measured p95 ranged from
6.251 ms to 20.611 ms, below the 300 ms POC read target.

CQ-01, CQ-03, CQ-04, CQ-06, and CQ-07 use indexed seeks with no scan. CQ-02 is
below the 10,000-row threshold and was still measured diagnostically. CQ-05's
bounded 75,000-row roster scan returned at most 100 rows at p95 9.756 ms under
approved exception `SPEC005-SCAN-20260719`, owned by Ahmed ELbamby and expiring
2026-08-02. The exception is POC-only and requires a reviewed covering roster
index before any production-readiness claim.

Executable gates:

- `Spec005QueryPlanRehearsalTests.Production_like_fixture_captures_reviewable_actual_plans`
- `NFR_1EvidenceTests.Recorded_actual_plans_meet_latency_index_and_bounded_scan_gate`

No missing plan, unknown scan, p95 breach, or unapproved exception remains.

**Result: PASS.**
