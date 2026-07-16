# SPEC-008 Non-Production Demo Release Approval

**Decision:** APPROVED for the SPEC-008 Development and Testing demo slice<br>
**Decision date:** 2026-07-16<br>
**Sole decision authority and developer:** Ahmed ELbamby<br>
**Governance boundary:** Gate A non-production design-capability demo only

## Decision authority and meaning

Ahmed ELbamby is the project's sole human approval authority. His standing
approval of the clarified SPEC-008 non-production demo contract, recorded on
2026-07-14 in the approved specification and requirements, authorizes this
decision at that boundary. The perspectives below are distinct reviews
performed by Ahmed; they are not additional people, institutional signatories,
or delegated AASTMT authorities.

Ahmed approves the delivered SPEC-008 academic-term, registration-window,
student-profile, owner-API, SQL-persistence, and four-page contributor slice for
synthetic Development and Testing use. This record does **not** represent
institutional approval, official AASTMT policy or data authorization, a
production release, or permission to go live.

This is not institutional approval.

## Eight review perspectives

| Review perspective | Decision | Concrete evidence reviewed | Residual boundary |
|---|---|---|---|
| Product | APPROVED for SPEC-008 demo scope | The [complete trace matrix](SPEC-008-traceability.md) accounts for 11 FRs, 4 NFRs, 9 ACs, 7 edge cases, 3 success criteria, 10 endpoints, 6 entities, 4 pages, the shared client facade, and the foundation migration. Focused Release runs recorded 37 contract and 27 acceptance checks passing with no failures or skips. | This approves authoritative context, profile and term-administration journeys only. Discovery, schedule construction, registration submission, records, staff workspaces, and complete-system release remain owned by later specifications. |
| Registrar / academic-domain owner | APPROVED for synthetic demo behavior | The [scope review](SPEC-008-scope-review.md) confirms explicit persisted term lifecycle, IANA timezone, sourced GPA/credits/standing/transcript/holds, append-only correction provenance, and no date heuristic or browser-clock authority. [NFR-3](SPEC-008-NFR-3.md) records five passing UTC/IANA checks. | No SIS synchronization or production student-data source is approved. The synthetic fixture is not an official AASTMT academic record, and this decision is not official Registrar or institutional authorization. |
| Identity owner | APPROVED for the SPEC-007-dependent demo boundary | The [authorization evidence](SPEC-008-NFR-4.md) records Student-own allow, wrong-resource and missing-claim deny, named permitted-Admin allow, Admin-without-permission deny, and Lecturer/TeachingAssistant full-profile deny. The authenticated load gate used the real cookie pipeline, SQL security-stamp validation, `Context.Read`, student scope, and shared SQL Data Protection across two replicas. | SPEC-007 remains the identity owner. This approval introduces no production identity provider, provisioning authority, recovery provider, credential policy, or institutional identity integration. |
| QA | APPROVED for the recorded candidate evidence | [NFR-1](SPEC-008-NFR-1.md) records 6/6 fake-clock boundary checks; [NFR-2](SPEC-008-NFR-2.md) records 180,000/180,000 successful authenticated context reads across two replicas, p95 15.6454 ms, and 0 unexpected failures; NFR-3 and [NFR-4](SPEC-008-NFR-4.md) each record 5/5 focused checks. Trace and frontend evidence bind the remaining contract, acceptance, component, browser, accessibility, visual, migration, and SQL checks. | Evidence is commit-sensitive and must be rerun after relevant drift. The SPEC-008 read-only load result does not replace SPEC-018 mixed read/write, spike, soak, failover, recovery, or production release gates. |
| Accessibility | APPROVED for the four delivered page contributors | [AUTH-01](SPEC-008-AUTH-01-frontend.md), [STU-01](SPEC-008-STU-01-frontend.md), [ADM-02](SPEC-008-ADM-02-frontend.md), and [ADM-04](SPEC-008-ADM-04-frontend.md) record axe, landmark, keyboard, focus, semantic alternative, 400% zoom, six-width, and governed cross-browser visual verification. Their accessibility suites record 9, 10, 8, and 9 passing checks respectively with no failures. | Approval covers these four SPEC-008 page contributors and their recorded browser matrix only. Future contributor content must pass its owning spec's accessibility gate; no institution-wide or production accessibility certification is claimed. |
| Data / concurrency | APPROVED for SPEC-008 owner writes and guards | The traced SQL evidence covers Code First migration-before-seed, migrated-schema constraints, unique keys, rowversion, bounded queries, payload-bound term-creation replay, expected-version mutations, append-only transcript supersession, student-term serialization, and atomic owner audit writes. The scope review confirms one shared `StudentRegistrationDbContext` and no distributed transaction or premature downstream entity. | SPEC-014 still owns seat allocation, enrollment idempotency, collision handling, overbooking prevention, and end-to-end registration concurrency. This record grants no capacity or registration-commit approval. |
| Security | APPROVED for synthetic Development/Testing scope | NFR-4 proves least-privilege profile authorization and absence of anonymous profile endpoints. NFR-2 used real protected cookies and cross-replica key sharing without recording University IDs, credentials, cookies, security stamps, or full profiles. Admin commands retain separately governed permission claims, XSRF protection, bounded input, safe errors, and stale-version rejection. | No production secrets custody, penetration-test sign-off, institutional privacy approval, production threat acceptance, or official security authorization is granted. Production configuration must continue to fail closed until separately approved. |
| Operations | APPROVED for isolated demo execution | NFR-2 records a pinned SQL Server 2022 container, one migrated per-run database, two stateless TestServer replicas, shared SQL-backed Data Protection, 25,000 synthetic sessions, exact 300 reads/s scheduling for ten minutes, and aggregate-only evidence. The scope review confirms a modular monolith without broker, microservice, distributed lock, or second database context. | Production deployment and operations are not approved. External network/TLS latency, production topology, observability acceptance, backups, recovery, capacity planning, on-call ownership, Gate B-D, and SPEC-018 release operations remain pending. |

## Explicit pending contributors and approvals

The following states are mandatory and cannot be inferred as approved by this
record:

- **SPEC-015 timetable contributor: PENDING.** STU-01 intentionally renders
  the current timetable contributor as unavailable. No timetable, schedule,
  registration record, or resume state is fabricated or approved.
- **SPEC-017 audit/report contributor: PENDING.** SPEC-008 delivers only its
  privacy-safe atomic owner audit records and owner APIs. Broader registration
  administration, audit orchestration, reporting, and views remain pending.
- **Gate B, Gate C, and Gate D: PENDING.** This record closes neither the
  secure walking-skeleton/master-data gate, the end-to-end beta/concurrency
  gate, nor UAT/security/load/recovery/release approval.
- **Production data source: NOT APPROVED.** No SIS connector, institutional
  source authority, reconciliation mechanism, production credential, or real
  student dataset is authorized.
- **Production deployment and operations: NOT APPROVED.** No production
  infrastructure, migration execution, secret custody, monitoring acceptance,
  backup/restore rehearsal, support model, or operational SLA is authorized.
- **Official AASTMT go-live approvals: NOT GRANTED.** Ahmed's project authority
  governs this non-production design-capability demo only and is not a
  substitute for institutional Registrar, security, privacy, operations, legal,
  accessibility, or executive approval.

## Evidence and scope binding

This decision is admitted only with the following complete records:

- [SPEC-008 traceability](SPEC-008-traceability.md), which binds every approved
  requirement and delivered surface to implementation and executable evidence.
- [SPEC-008 scope review](SPEC-008-scope-review.md), which keeps grade
  computation, date inference, browser-clock authority, SIS integration, and
  downstream ownership outside this slice.
- [NFR-1](SPEC-008-NFR-1.md), [NFR-2](SPEC-008-NFR-2.md),
  [NFR-3](SPEC-008-NFR-3.md), and [NFR-4](SPEC-008-NFR-4.md), which bind the
  clock, authenticated scale, time persistence, authorization, and privacy
  results.
- The four route records linked in the accessibility perspective, including
  their governed visual manifests and honest downstream contributor states.

Any missing evidence file, unresolved template marker, failed required check, or
material source/hash drift invalidates this approval until Ahmed reviews a
reconciled candidate. Later-spec or production evidence may extend this record
only through its own approved gate; it may not silently broaden this decision.

## Approval statement

Ahmed ELbamby approves the evidenced SPEC-008 slice for the non-production
Development and Testing design-capability demo under the standing Gate A
authority described above. All explicit pending and not-approved boundaries in
this record remain in force.

**Result: APPROVED — NON-PRODUCTION SPEC-008 DEMO ONLY.**
