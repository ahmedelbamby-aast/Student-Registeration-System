# Specification Review and Gap-Closure Report

**Review completed:** 13 July 2026

**Scope:** All 18 Spec Kit feature packages and their shared planning artifacts

**Automated result:** PASS

**Human approval:** APPROVED by Ahmed ELbamby for demo implementation on 13 July 2026

**Implementation state:** Gate A approved; application source, migrations, and executable tests may now begin by approved slice

## Outcome

The 18 specifications now form one connected, acyclic delivery plan rooted at
SPEC-001. Each feature package contains the required Spec Kit artifacts and a
sequential unchecked task plan. The current baseline contains:

- 185 functional and 85 non-functional requirements;
- 146 Given/When/Then acceptance criteria, 92 edge cases, and 75 explicit
  out-of-scope guards;
- 1,593 exact-file tasks with test-before-delivery ordering;
- 27 canonical routes, 20 reusable frontend components, and 80 canonical API
  endpoints;
- 15 explicit real-SQL concurrency races with deterministic oracles.

The reproducible result is in [SPECKIT_AUDIT.md](SPECKIT_AUDIT.md), and the
environment/verification summary is in [VALIDATION.md](VALIDATION.md).

## Connected specification inventory

| Spec | Name | Accountable role | Direct dependencies |
|---|---|---|---|
| SPEC-001 | Product Charter and RBAC | Product Owner | None |
| SPEC-002 | AASTMT Policy Rulebook | Registrar/Policy SME | SPEC-001 |
| SPEC-003 | Frontend Page Design, Storyboard, Accessibility and Functional Testing | UX Lead | SPEC-001, SPEC-002 |
| SPEC-004 | Architecture and Engineering Principles | Technical Lead/Architect | SPEC-001, SPEC-003 |
| SPEC-005 | ERD and Data Lifecycle | Data/Backend Lead | SPEC-002, SPEC-004 |
| SPEC-006 | Domain Classes and API Contracts | Technical Lead | SPEC-004, SPEC-005 |
| SPEC-018 | Quality, Security, Scalability, and Operations | QA/DevOps/Security Leads | SPEC-003, SPEC-001, SPEC-004, SPEC-005, SPEC-006 |
| SPEC-007 | Identity and Account Lifecycle | Security Lead | SPEC-003, SPEC-004, SPEC-005, SPEC-006, SPEC-018 |
| SPEC-008 | Academic Term and Student Profile | Backend Lead | SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-007, SPEC-018 |
| SPEC-009 | Catalogue, Prerequisites, and Policy Administration | Registrar/Policy SME and Backend Lead | SPEC-002, SPEC-003, SPEC-005, SPEC-006, SPEC-008, SPEC-018 |
| SPEC-010 | Offerings, Groups, and Resources | Backend Lead | SPEC-003, SPEC-005, SPEC-006, SPEC-009, SPEC-018 |
| SPEC-011 | Eligibility and Subject Discovery | Product Owner | SPEC-002, SPEC-003, SPEC-006, SPEC-008, SPEC-009, SPEC-010, SPEC-018 |
| SPEC-012 | Schedule Builder and Conflicts | Technical Lead | SPEC-003, SPEC-010, SPEC-011, SPEC-018 |
| SPEC-013 | Schedule Recommendations | Technical Lead | SPEC-003, SPEC-010, SPEC-011, SPEC-012, SPEC-018 |
| SPEC-014 | Registration Capacity and Concurrency | Data/Backend Lead | SPEC-003, SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-011, SPEC-012, SPEC-013, SPEC-018 |
| SPEC-015 | Student Registration Records | Product Owner | SPEC-003, SPEC-008, SPEC-014, SPEC-018 |
| SPEC-016 | Lecturer and Teaching Assistant Workspace | Product Owner | SPEC-003, SPEC-007, SPEC-010, SPEC-015, SPEC-018 |
| SPEC-017 | Admin Operations, Audit, and Reporting | Product Owner | SPEC-003, SPEC-004, SPEC-007, SPEC-008, SPEC-009, SPEC-010, SPEC-014, SPEC-015, SPEC-016, SPEC-018 |

The machine-readable dependency graph is
[spec-manifest.json](../.specify/spec-manifest.json); the phased Agile delivery
and update rules are in [PROJECT_PLAN.md](PROJECT_PLAN.md).

## Gaps found and closed

### Requirement and task semantics

The former task plans relied on repeated catch-all feature files and generic
verification wording. They now use
[workstream-manifest.json](../.specify/workstream-manifest.json), which maps
every FR exactly once to a named workstream, tailored test focus, exact test
file, and exact delivery file. Acceptance work is grouped as prioritized user
stories with an independent-test statement and dependency baseline. Delivery
tasks explicitly depend on a preceding failing test.

### Frontend design and functional testing

SPEC-003 is the single frontend design/test-contract owner; it does not own
feature server endpoints. The [route manifest](../.specify/route-manifest.json)
defines one design owner and one canonical Razor implementation owner for each
of 27 routes. The [page matrix](../specs/003-ux-storyboard-accessibility/page-matrix.md)
defines composition and minimum journeys, while feature contributors own their
data/actions/reason contracts without writing the page.

The frontend plan now includes:

- six design widths from 320 through 1920 CSS pixels and 400% reflow;
- 20 explicit reusable components, including link and alert coverage;
- bUnit component tests, route/API contract fixtures, Playwright journeys,
  axe-core/keyboard/focus checks, and governed visual baselines;
- pinned OS/browser evidence, actual Safari-on-macOS evidence distinct from
  Playwright WebKit, deterministic fixtures, first-run-failure policy,
  diagnostics-only retries, and no quarantine for critical journeys;
- a single Razor writer and owner E2E test before page delivery.

### Endpoint and entity ownership

[endpoint-manifest.json](../.specify/endpoint-manifest.json) assigns each of 80
method/path pairs to one feature. Only that feature plans its contract test and
handler. [entity-ownership.json](../.specify/entity-ownership.json) assigns
shared domain concepts and governed artifacts to one canonical owner; other
features consume them. SPEC-005 governs schema/lifecycle contracts without
writing downstream runtime models. The persistence manifest assigns every EF
mapping contribution and slice migration to one writer. Staff,
StaffTermAvailability, AuditEvent, AdminSecurityGuard, and receipt-projection
ownership are explicit.

### Dependency seams, audit, and executable migrations

Identity activation now claims an Identity-owned pre-provisioned
`ApplicationUser`; the downstream academic Student links one-to-one after
activation. Academics stores sourced ProgramCode/CourseCode references instead
of foreign keys to downstream catalogue definitions. Schedule recommendation
declares the Scheduling, eligibility, and plan contracts it consumes.

SPEC-004 owns the transaction-aware audit write port, append-only AuditEvent
mapping, and SQL writer. SPEC-007 alone owns RoleAssignment mutation and
AdminSecurityGuard; SPEC-017 delegates role changes and owns read/query/export
experiences. This removes downstream dependency cycles and duplicate writers.

[persistence-manifest.json](../.specify/persistence-manifest.json) defines the
single DbContext, ten mapping contributions, and five Code First migrations in
dependency order. S1 can therefore authenticate and show academic context on a
real database before later catalogue, planning, registration, and reporting
slices exist.

### Race conditions and idempotency

SPEC-014 now defines one test for every row in its
[concurrency matrix](../specs/014-registration-capacity-concurrency/concurrency-matrix.md).
The design serializes by student/term, shared registration-context versions,
and sorted group rows in SQL Server, so two stateless application replicas use
the same ordering boundary.

The final transactional semantics are explicit:

- the idempotency claim is created inside the registration transaction;
- an allocation savepoint is created after claim/revalidation and before the
  first seat mutation;
- deterministic allocation rejection rolls seat changes back to the
  savepoint, then commits a replayable rejected result;
- transient or transaction-aborting failure rolls back the whole transaction
  and claim;
- a request blocked on an uncommitted same-key claim waits at most 500 ms,
  then receives a non-durable 202 with clientRequestId/retry/result URL and no
  submission ID; it never reads an uncommitted Processing row;
- child meeting/resource/staff mutations advance the SectionGroup version;
  availability ranges advance their StaffTermAvailability aggregate version;
- reconciliation mismatch pauses registration for the affected group until an
  authorized audited repair completes.

These choices are synchronized across [ARCHITECTURE.md](ARCHITECTURE.md),
[ADR-002](adr/ADR-002-atomic-capacity.md), and the [ERD](diagrams/ERD.md).

### Validator quality

The repository validator now checks more than copied identifiers. It enforces
manifest uniqueness, dependency connectivity, requirement-to-acceptance and
requirement-to-workstream coverage, test-first order, route/component/endpoint/
entity single ownership, every race row/oracle, concurrency API/ERD fields,
strict Spec Kit scores, the planning-only boundary, and Ahmed ELbamby's exact
Git identity in repository configuration and history.

## Gate A decision outcome

Ahmed resolved every demo implementation decision in
[OPEN_DECISIONS.md](OPEN_DECISIONS.md), including identity/data generation,
English-first branding, simple academic policy, curriculum, browser/SQL/scale/
retention/secrets profiles, and staff-owned availability. Gate A is approved
for all 18 specs. Production deployment, real-data processing, institutional
branding/policy authority, and Gate B-D release decisions remain separate.

## Development update rule

For each Agile slice, the team updates the normative requirements first, then
the relevant ownership/dependency manifest, generated spec/plan/model/API/tasks,
traceability, and evidence. A change is not ready for human approval until the
full validator, Mermaid renderer, relative-link check, and applicable manual
review all pass. Implementation may proceed only within an Approved spec and
its dependency/test-first task order.
