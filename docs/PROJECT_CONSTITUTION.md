# Project Constitution

**Project:** Student Registration System<br>
**Owner:** Ahmed ELbamby<br>
**Status:** Binding<br>
**Version:** 1.0.0<br>
**Ratified:** 2026-07-12

This constitution governs every specification, source change, test, document,
database migration, and repository commit in this project. Where another
project document conflicts with this constitution, this constitution takes
precedence until it is formally amended.

## Article I: Ownership and Git Identity

1. Ahmed ELbamby is the project owner and the sole author/committer identity
   used in this repository.
2. Every Git commit MUST record both author and committer exactly as:
   - Name: Ahmed ELbamby
   - Email: A.Elbamby61869@student.aast.edu
3. Codex, OpenAI, or any other AI tool MUST NOT appear as an author, committer,
   co-author, contributor, sign-off identity, or generated-by attribution in
   commit metadata, commit-message trailers, release notes, or contributor
   lists.
4. AI tools are treated only as development tools operating under Ahmed
   ELbamby's direction and review. They do not receive repository authorship
   or contribution attribution.
5. Commit messages MUST NOT contain Co-authored-by, Signed-off-by,
   Generated-by, or similar attribution for Codex, OpenAI, or another AI tool.
6. Automated workflows MUST NOT create commits with a bot or service identity.
   Any repository commit produced through automation MUST still use the exact
   owner identity above and remain under the owner's review.

## Article II: Specification-First Development

1. No application behavior is implemented before its specification is
   reviewed and approved.
2. Every implementation change MUST trace to a SPEC-NNN functional requirement
   and acceptance criterion.
3. A missing or changed requirement is resolved in the specification before
   code changes.
4. AASTMT policy behavior requires Registrar/Policy SME approval and an
   effective-dated source.

## Article III: Simplicity and Modular Design

1. The default architecture is the approved modular monolith.
2. KISS and YAGNI govern technical decisions.
3. Microservices, message brokers, dynamic policy scripting, event sourcing,
   distributed locks, and institution-wide timetable solvers require evidence,
   a new specification, and an approved ADR.
4. Modules communicate through narrow approved contracts and MUST NOT bypass
   ownership boundaries.

## Article IV: Correctness, Security, and Academic Integrity

1. Server time, identity, role, academic term, policy, eligibility, conflicts,
   and capacity are authoritative.
2. Client-side checks improve UX but never replace API authorization or final
   server validation.
3. Registration is atomic: a complete schedule succeeds or no part is
   committed.
4. Group capacity MUST never be exceeded, including during concurrent
   submissions.
5. Policy decisions MUST be explainable and retain their source/version.
6. Secrets, credentials, and unnecessary student data MUST NOT enter source
   control, commit messages, or unsafe logs.

## Article V: Accessibility and User Experience

1. Critical flows target WCAG 2.2 AA.
2. Eligibility, capacity, and timetable conflicts MUST NOT rely on color alone.
3. Every calendar has an equivalent chronological list or table.
4. An unresolved hard conflict visibly blocks submission and provides manual
   resolution guidance.

## Article VI: Quality and Change Control

1. Approved acceptance criteria produce automated tests before implementation.
2. Domain rules, SQL constraints, concurrency behavior, authorization,
   accessibility, performance, migration, backup, and rollback are verified in
   proportion to risk.
3. Architecture changes require an ADR; policy changes require a new approved
   effective-dated version.
4. Production migrations are reviewed deployment steps and do not run
   automatically at application startup.
5. Released behavior is superseded through versioned changes, not silently
   rewritten.

## Article VII: Commit Verification

Before a commit is accepted, the following checks MUST pass:

```powershell
git config --local user.name
git config --local user.email
git show -1 --format=fuller
git log --format="%an|%ae|%cn|%ce|%B"
```

The expected identity for every author and committer is:

```text
Ahmed ELbamby|A.Elbamby61869@student.aast.edu
```

The commit message/body MUST contain no AI attribution trailers.

## Amendment Process

1. An amendment MUST state the affected article, rationale, owner, and date.
2. Amendments MUST be reviewed by Ahmed ELbamby and relevant project owners.
3. Material architecture, security, data, or policy amendments also require
   the approvals defined by their specifications.
4. The constitution version MUST be incremented and the amendment recorded in
   Git using the identity rules in Article I.
