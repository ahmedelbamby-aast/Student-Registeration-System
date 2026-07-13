# Decisions Required Before Human Spec Approval

These are not implementation assumptions. Each item has a safe planning rule
that prevents unapproved behavior, plus Ahmed ELbamby's decision is required
before the affected specification can become Approved.

| ID | Decision required | Recommended decision / safe planning rule | Affected specs |
|---|---|---|---|
| DEC-01 | Which institutional service/factor proves a student owns a pre-imported University ID? | Use the official AASTMT identity/verification service if available. Until it is named and security-reviewed, activation remains In Review and no substitute secret is invented. | SPEC-007 |
| DEC-02 | Which identity provider and MFA method authenticate Admin, Lecturer, and TA? | Prefer the official AASTMT staff identity provider with its managed MFA. Local staff self-registration remains prohibited. | SPEC-007, SPEC-018 |
| DEC-03 | Which official AASTMT/College-of-AI logo, colors, typography, and usage rules govern production UI? | Use neutral WCAG-compliant semantic tokens for design structure only; do not approve production visual baselines until the institutional brand pack is supplied. | SPEC-003 |
| DEC-04 | Is Arabic/RTL required in MVP or a later release? | Keep the MVP localization-ready and English-first; Arabic/RTL remains a separately estimated/approved scope unless Ahmed selects bilingual MVP. | SPEC-003 |
| DEC-05 | Confirm browser/device support and access to a macOS/Safari validation environment. | Current best-practice baseline is current and previous Chrome/Edge/Firefox plus actual current Safari on macOS, from 320 to 1920 CSS pixels; Playwright WebKit is not labeled Safari. | SPEC-003, SPEC-018 |
| DEC-06 | Is a travel-time buffer between different rooms/campuses required, and who approves its minutes/matrix? | Disable the hard travel-buffer rule until a typed, sourced, approved rule exists; never guess a duration. | SPEC-002, SPEC-012, SPEC-013 |
| DEC-07 | What retention periods apply to student records, security events, audit events, exports, and idempotency records? | Preserve required academic/audit history and prevent hard deletion until AASTMT privacy/legal owners approve a classification-specific schedule. | SPEC-005, SPEC-007, SPEC-014, SPEC-017, SPEC-018 |
| DEC-08 | Confirm that Student Register means activation of a pre-provisioned institutional student record, not open creation of a University identity. | Keep pre-provisioned activation; it prevents users from inventing University IDs. | SPEC-001, SPEC-007 |
| DEC-09 | Who approves the final production population, concurrency, throughput, and availability baseline? | Use SPEC-018 values for engineering validation and require Product Owner plus Operations sign-off before production sizing. | SPEC-001, SPEC-018 |
| DEC-10 | Who will provide final decisions and effective dates for POLICY-Q01 through POLICY-Q08? | Assign the AASTMT Registrar/College policy owner and privacy owner; affected rules remain unpublished or out of MVP until signed approval and boundary examples are recorded. | SPEC-002, SPEC-005, SPEC-009, SPEC-011, SPEC-014 |
| DEC-11 | Which teaching roles are mandatory for each SectionGroup activity type? | Provisional safe rule: Lecture requires at least one Lecturer; Tutorial/Laboratory requires at least one Teaching Assistant; a composite registration choice displays all assigned staff. Keep any institution-specific exception unpublished until approved. | SPEC-002, SPEC-010, SPEC-011 |
| DEC-12 | May an Admin edit a staff member's availability? | Staff own declarations. Admin may view/import; a correction requires a separate permission, reason, preview, expected version, audit record, and notification to the staff member. | SPEC-010, SPEC-016, SPEC-017 |
| DEC-13 | Which production secret provider and certificate/private-key custody process will protect shared Data Protection keys? | Store the shared key ring in SQL Server and protect keys with a deployment certificate obtained from the approved secret provider; production remains blocked until Operations and Security name both providers and rehearse rotation/recovery. | SPEC-007, SPEC-018 |
| DEC-14 | Which SQL Server version/compatibility level and production-like benchmark profile are approved? | Data/Operations must record edition/version, compatibility level, CPU/RAM/storage topology, replica/network shape, sanitized dataset size/distribution, container/CI image and licensing before migration rehearsal or load evidence is accepted. Local development may proceed only against an explicitly labeled non-production profile. | SPEC-004, SPEC-005, SPEC-018 |

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
