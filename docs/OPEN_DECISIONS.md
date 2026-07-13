# Demo Decision Register

These are Ahmed ELbamby's approved demo decisions. They authorize implementation
of the non-production POC and do not authorize production deployment.

For this non-production design-capability demo, Ahmed ELbamby is the sole
developer and sole human approval authority. Named institutional and delivery
roles below are review perspectives he fulfills. His demo decisions do not
claim official AASTMT production authorization.

## Clarifications

### Session 2026-07-13

- Q: Who holds development and approval authority for this project? → A:
  Ahmed ELbamby is the sole developer and sole approver for this
  non-production design-capability demo.
- Q: Which identity and database model should the demo use? → A: Use no MFA or
  second factor. Generate isolated Development and Testing databases with
  synthetic student records, unique University IDs, and generated
  PIN/password credentials; persist only password hashes.
- Q: Which demo language and brand treatment should the frontend use? → A: Use
  an English-first, localization-ready, neutral accessible UI and the
  unchanged official AASTMT logo sourced from the official AASTMT website.
- Q: Which academic rules should the POC implement? → A: Use the simple
  `DEMO-POC-2026.1` profile: an 18-credit default target/normal maximum,
  probation maximum 12, hard prerequisites/capacity/overlap checks, no travel
  buffer or exception workflows, first-commit capacity, required
  Lecturer/TA activity staffing, and a curated AASTMT AI curriculum snapshot
  with clearly labeled synthetic gap rows.
- Q: Which final operating profile and approval gate should apply? → A: Use SQL
  Server 2022 Developer compatibility level 160, current Chrome/Edge/Firefox
  plus Playwright WebKit, the approved two-replica scale fixture, disposable
  Testing and guarded-reset Development data, seven-day local artifacts,
  local-only secrets/key protection, staff-owned availability with read-only
  Admin access, and approve Gate A for demo implementation.

## Resolved governance decisions

| ID | Decision | Resolution | Affected specs |
|---|---|---|---|
| DEC-15 | Who develops and approves the demo? | Ahmed ELbamby is the sole developer and approval authority; named roles are mandatory review perspectives, not separate signatories. | All specs |
| DEC-01 | How does a demo student prove ownership of a generated University ID? | First use submits the generated University ID and PIN/password; ASP.NET Core Identity verifies the stored hash and one atomic transition activates only that pre-provisioned identity. | SPEC-007 |
| DEC-02 | How do Admin, Lecturer, and TA authenticate in the demo? | Pre-provisioned local username/password on the shared staff page; no MFA/2FA, no role selector, and server-derived roles. | SPEC-007, SPEC-018 |
| DEC-08 | Does Student Register create or activate an identity? | It activates a system-generated, pre-provisioned synthetic student identity; the browser cannot invent a University ID or create an open account. | SPEC-001, SPEC-007 |
| DEC-16 | How are non-production identities and student records created? | Code First creates isolated Development and per-run Testing databases; guarded seed contributors generate wholly synthetic students, unique University IDs, and PIN/passwords while SQL stores only ASP.NET Core Identity hashes. | SPEC-005, SPEC-007, SPEC-008, SPEC-018 |
| DEC-03 | Which demo logo and visual identity are approved? | Use the unchanged logo hosted on the official `aast.edu` domain and documented in [BRAND_ASSETS.md](BRAND_ASSETS.md), served from a provenance-recorded local copy after Gate A. Use neutral accessible semantic tokens and do not infer other colors, typefaces, or brand rules from the logo. This is demo approval, not official production-brand authorization. | SPEC-003 |
| DEC-04 | Is Arabic/RTL required in the demo MVP? | No. The MVP is English-first and localization-ready; Arabic translation and RTL delivery require a future approved specification. | SPEC-003 |
| DEC-06 | Does the demo enforce travel time between rooms/campuses? | No. Exact meeting overlaps remain hard conflicts, but travel-buffer conflicts are disabled and the system does not guess a duration or matrix. | SPEC-002, SPEC-012, SPEC-013 |
| DEC-10 | Which unresolved public-policy questions govern the demo? | `DEMO-POC-2026.1` resolves POLICY-Q01 through POLICY-Q07 as documented in [POLICY_RESEARCH.md](POLICY_RESEARCH.md): simple hard rules, 18-credit default target/normal maximum, probation maximum 12, first-commit capacity, no waitlist/override, and no exception/advisor/drop/withdrawal workflows. POLICY-Q08 uses DEC-07's synthetic-demo retention profile. | SPEC-002, SPEC-005, SPEC-009, SPEC-011, SPEC-014 |
| DEC-11 | Which teaching roles are mandatory for an open subject? | Every published offering has a Lecture with at least one Lecturer plus at least one Tutorial/Section or Laboratory activity; each present Tutorial/Section and Laboratory activity has at least one TA. An offering with both requires TAs for both. | SPEC-002, SPEC-010, SPEC-011 |
| DEC-05 | Which browsers must pass the POC? | Current stable Chrome, Edge, and Firefox plus Playwright WebKit at the approved responsive widths. Actual Safari/macOS evidence is deferred to a future production-readiness scope and WebKit is not labeled Safari. | SPEC-003, SPEC-018 |
| DEC-07 | What retention applies to synthetic demo data and artifacts? | Per-run Testing databases are disposed after the run. Development academic/audit/idempotency data persists until an explicit guarded reset. Git-ignored local logs, exports, and generated credential artifacts expire within seven days. No production or real student data is permitted. | SPEC-005, SPEC-007, SPEC-014, SPEC-017, SPEC-018 |
| DEC-09 | Which demo scale profile is approved? | Validate 25,000 accounts, 5,000 sessions, 75 registrations/s, 300 reads/s, and a 200 registrations/s spike across at least two stateless API replicas. These are POC engineering targets, not production sizing or an SLA. | SPEC-001, SPEC-018 |
| DEC-12 | May an Admin edit staff availability in the POC? | No. Staff edit their own availability. Admin may view and import availability but has no correction/override command in the POC. | SPEC-010, SPEC-016, SPEC-017 |
| DEC-13 | How are POC secrets and shared Data Protection keys protected? | Connection secrets use .NET User Secrets or environment variables. Multi-replica tests use a SQL-backed shared key ring protected by a generated local certificate kept outside Git. No production secret provider is selected or implied. | SPEC-007, SPEC-018 |
| DEC-14 | Which SQL Server profile is approved? | SQL Server 2022 Developer with compatibility level 160, run through Docker/Testcontainers for Development, Testing, migration, concurrency, and load evidence. This is not a production edition/topology decision. | SPEC-004, SPEC-005, SPEC-018 |
| DEC-17 | Is Gate A approved? | Yes. Ahmed ELbamby approved all 18 planning packages for demo implementation on 13 July 2026. Gate B-D and production deployment/release approval remain separate. | All specs |

## Open demo decisions

None. Behavior changes discovered during implementation still begin with a
specification amendment and Ahmed ELbamby's approval.

## Decisions already made by best-practice rule

- Scheduled registration cutoff uses authoritative server receipt time; an
  emergency administrative closure/version change blocks uncommitted work.
- Registration serializes per student/term and per contested group in SQL
  Server, not with application-instance or distributed locks.
- Student drop/withdrawal/correction is outside MVP because no approved policy
  or workflow exists.
- Reused idempotency key with a different canonical payload is rejected.
- The 18-spec inventory is preserved; SPEC-003 is the single frontend design
  and functional-testing contract rather than creating a duplicate SPEC-019.
