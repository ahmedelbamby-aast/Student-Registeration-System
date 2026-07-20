# Manual role testing guide for the local demo

This guide tells a developer how to start the local Student Registration
System, sign in as every supported role, and manually check the important
behavior. The language is intentionally simple so the same guide can also be
given to a test participant.

> [!IMPORTANT]
> Every username and password below is for the local synthetic Development
> database only. Never reuse these passwords for AASTMT, production, personal,
> or shared accounts. Do not paste them into tickets, logs, screenshots, or
> commits.

> [!NOTE]
> One developer testing every role is useful developer verification, but it
> does not satisfy SPEC-003 NFR-8. That requirement needs at least 17 unique
> representative human participants. The exact cohort and recording template
> are near the end of this guide.

## What the normal Development seed contains

The normal seed creates:

- 25 synthetic students: `AI2600001` through `AI2600025`.
- One Administrator, one Lecturer, and one Teaching Assistant. Each enabled
  account has exactly one role.
- One open demo academic term and registration window.
- A published 19-course, exactly-three-credit Data Science catalogue with the
  programme roadmap and prerequisite DAG.
- The published demo policy: probation maximum 12 credits, normal maximum 18,
  and a 19-21-credit overload only when CGPA is at least 3.00.
- One published current-term offering and group for every seeded course, with
  varied capacity, a lecture, a tutorial, two rooms, and separate Lecturer and
  Teaching Assistant assignments.
- A synthetic academic profile for each student. `AI2600001` is the
  first-program-term persona; `AI2600002` and later include multiple term-2+
  personas. Normal personas have no active blocking registration hold.
  `AI2600007` is the one deterministic active-hold persona for blocked-state
  testing.

The seed is idempotent: rerunning the initializer validates and reuses the
published graph instead of duplicating it. Registrations and rosters still
start empty until the automatic/self-service registration workflows create
them.

The current academic seed profile is `synthetic-fixture/2.0`. A retained local
database containing the complete unmodified v1 synthetic profile is upgraded
in place: stable student and term identities are preserved, program-term
ordinals are reconciled, and only v1 seed-owned transcript/hold rows are
replaced. If those academic rows were manually changed, the initializer stops
with `ACADEMIC_SEED_UPGRADE_UNSAFE`; preserve the data, or use
`-ResetDatabase` only when deleting the local demo is intended.

## Application addresses

| Purpose | Address |
|---|---|
| Public gateway | `https://localhost:7078/` |
| Student login | `https://localhost:7078/student/login` |
| Student activation | `https://localhost:7078/student/activate` |
| Staff login | `https://localhost:7078/staff/login` |
| Account recovery | `https://localhost:7078/account/recovery` |
| Health response | `https://localhost:7078/api/health` |
| OpenAPI document | `https://localhost:7078/openapi/v1.json` |

## Demo usernames, role codes, and passwords

| Person to test | Server role code | Login ID | Login page | Password or initial activation secret |
|---|---|---|---|---|
| Student 1, only when previously activated and the database was retained | `Student` | `AI2600001` | `/student/login` | `DemoLogin@2026!!` |
| Fresh or unused student | `Student` | `AI2600001` through `AI2600025` | `/student/activate` first | `Demo@2026-<University ID>`, for example `Demo@2026-AI2600002` |
| Administrator | `Admin` | `ADM-0001` | `/staff/login` | `Demo@2026-ADM-0001` |
| Lecturer | `Lecturer` | `LEC-0001` | `/staff/login` | `Demo@2026-LEC-0001` |
| Teaching Assistant | `TeachingAssistant` | `TA-0001` | `/staff/login` | `Demo@2026-TA-0001` |

The student activation value is called a PIN/password in the specifications.
It is not a separate numeric OTP and it is not MFA. On a fresh database,
activate the student with the initial value and choose a new password. After a
successful activation, the initial value must no longer work.

The deterministic initial student password pattern is:

```text
Demo@2026-AI26NNNNN
```

For example:

```text
University ID: AI2600002
Initial activation secret: Demo@2026-AI2600002
Suggested local-only new password: DemoLogin@2026!!
```

## Start the complete local demo

Install the pinned .NET SDK `10.0.301`, Docker Desktop with Linux containers,
and PowerShell 7. Then run this one command from the repository root:

```powershell
.\ops\scripts\Start-LocalDemo.ps1 -TrustHttpsCertificate
```

The launcher performs all required work in dependency order:

1. Checks .NET, Docker, Compose, and HTTPS certificate trust.
2. Creates random local SQL and certificate passwords when they do not exist.
3. Saves those values outside Git in .NET User Secrets.
4. Creates or reuses the Git-ignored Data Protection PFX.
5. Starts SQL Server and waits for Docker health.
6. Restores and builds the solution, applies migrations, and idempotently seeds
   the synthetic Development database.
7. Starts the API and hosted Blazor WebAssembly client at
   `https://localhost:7078`.

Keep this terminal open. Press `Ctrl+C` to stop the web application. SQL data
is preserved so the next startup is faster. On later runs, skip the build when
source has not changed:

```powershell
.\ops\scripts\Start-LocalDemo.ps1 -SkipBuild
```

To prepare and verify SQL/migrations/seed without starting the long-running
web process:

```powershell
.\ops\scripts\Start-LocalDemo.ps1 -PrepareOnly
```

If an older local volume was initialized with another SQL password, or a fully
fresh demo is required, explicitly delete and recreate only the local demo
database:

```powershell
.\ops\scripts\Start-LocalDemo.ps1 -ResetDatabase
```

### Confirm that the server is responding

```powershell
Invoke-RestMethod `
    -Uri 'https://localhost:7078/api/health' `
    -SkipCertificateCheck
```

Then open `https://localhost:7078/` in the browser.

The local POC has no external telemetry exporter, so the safe health summary
normally says `degraded` while returning HTTP 200. SQL failure returns
`unhealthy` with HTTP 503.

## Show the real generated credential sheet

The seed writes a reveal-once-style, Git-ignored JSON artifact for newly
created accounts. Use this command instead of guessing a password:

```powershell
$credentialFile = Get-ChildItem `
    '.\src\StudentRegistration.Api\.local\credentials\imports\import-*.json' |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if ($null -eq $credentialFile) {
    throw 'No credential artifact exists. Seed a fresh Development database first.'
}

$credentials = Get-Content $credentialFile.FullName -Raw | ConvertFrom-Json
$credentials.accounts |
    Select-Object loginIdentifier, secret |
    Format-Table -AutoSize
```

Show one account only:

```powershell
$login = 'ADM-0001'
($credentials.accounts |
    Where-Object loginIdentifier -eq $login |
    Select-Object -First 1).secret
```

The artifact contains the originally issued student activation secret. It does
not know a password that the student chose later. Credential files expire
after seven days.

## Checks to repeat for every role

Do these checks once as Student, Admin, Lecturer, and Teaching Assistant.

- [ ] Start at `/`. Press `Tab` until **Skip to main content** appears. Press
  `Enter`. Focus should move to the page's main heading.
- [ ] Complete sign-in using only the keyboard. Every focused link, input, and
  button must have a visible focus indicator.
- [ ] Confirm that the login form has visible labels. Browser autofill must not
  remove or hide those labels.
- [ ] Try one wrong password. The error must be generic, the password must be
  cleared, and the page must not reveal whether the account exists.
- [ ] Use the browser at 400% zoom. Also use responsive mode at 320 CSS pixels.
  Actions, full error reasons, and recovery links must remain reachable.
- [ ] Use a screen reader if available. There should be one useful page
  heading, named navigation/regions, understandable labels, and announced
  status/error messages.
- [ ] Confirm that status is never communicated by color alone. A conflict,
  error, or success must also contain text or an icon with a readable name.
- [ ] Open a confirmation dialog. `Tab` and `Shift+Tab` must stay inside it.
  `Escape` or **Cancel** must close it and return focus to the button that
  opened it.
- [ ] Quickly press a save or submit action twice. The control should disable
  while pending and only one final result should appear.
- [ ] Open a route that the role does not own. The result must be a safe denied,
  not-found, or sign-in page with no protected data, SQL text, stack trace,
  password, internal identifier, or foreign record.
- [ ] When an error displays a reference value, record the actual value. Live
  correlation IDs are dynamic 32-character lowercase hexadecimal values. Do
  not expect fixture values such as `STU-02-SAFE-REF` in the live app.

## Student manual test

### Activate a fresh student

Use an unused student such as `AI2600002`.

1. Open `/student/activate`.
2. Enter University ID `AI2600002`.
3. Enter initial secret `Demo@2026-AI2600002`.
4. Enter and confirm `DemoLogin@2026!!` as the new local-only password.
5. Select **Activate account** once.
6. Verify that the result says activation succeeded and gives a safe next
   action.
7. Try activating the same student again. It must not activate twice or reveal
   the old secret.
8. Sign in at `/student/login` with the new password.

### Test the Student pages

| Route | What to do | What must be true |
|---|---|---|
| `/student` | Review the dashboard. | It shows server-controlled date/timezone, term/window, the signed-in student's academic summary, holds or blocking reasons, and the correct Start/Resume action. It must not use the browser clock as authority. |
| `/student/subjects` | Search, filter, clear the search, and reset filters. | The published seeded catalogue and offerings appear. Result count changes are announced and unavailable subjects give text reasons. |
| `/student/subjects/{offeringId}` | Open a seeded offering result. | Credits, capacity, Lecturer, TA, room, day, time, and eligibility reasons come from the server. A made-up ID gives safe not-found/denied behavior. |
| `/student/schedule` | Add seeded groups and compare calendar and list views. | Both views contain the same meetings. An overlap shows a red X **and** the word **Conflict**, including subject, group, day, start, and end. |
| `/student/review` | Review a valid and an invalid plan. Open and cancel the submit confirmation, then submit once when valid. | Every blocking reason is visible. Submit stays disabled while blocked or pending. One atomic result appears; no partial success is claimed. |
| `/student/registration/result/{id}` | Open the returned result ID. Also try a random or foreign ID. | Accepted/rejected outcome is clear and states that nothing was partially registered. Foreign/random records reveal no data. |
| `/student/registrations` | Review current and historical registrations. | Calendar/list information agrees, paging is bounded, and an empty history is honest. |
| `/student/account` | Review the session and use sign-out/security actions. | Only the current student's account/session information appears. Secrets are never displayed. |

### Student authorization checks

While still signed in as the Student, open these addresses directly:

```text
https://localhost:7078/admin
https://localhost:7078/admin/terms
https://localhost:7078/admin/offerings
https://localhost:7078/staff
https://localhost:7078/staff/availability
```

Every address must deny access safely. Changing the URL must never grant a
role.

## Administrator manual test

Sign out, then sign in at `/staff/login` with:

```text
Username: ADM-0001
Password: Demo@2026-ADM-0001
Expected destination: /admin
```

The login form must not contain a role selector. The server decides that this
account is an Admin.

| Route | What to do | What must be true |
|---|---|---|
| `/admin` | Review metrics; pause, resume, and refresh where available. | Metrics have timestamps and text equivalents. Stale/degraded state is explicit. Refresh must not steal keyboard focus. |
| `/admin/terms` | Find the seeded term. In disposable data, try invalid dates, overlapping windows, cancel Publish, then publish a valid item. | Invalid/overlapping values are blocked. Confirmation is accessible. Server row-version conflicts ask for refresh/review instead of silently overwriting. |
| `/admin/users` | Search users, review paging, preview an import, and inspect role/status controls. | No plaintext passwords appear. Student roles cannot be granted through staff-role replacement. The final enabled Admin cannot be removed or disabled; expect `FINAL_ADMIN_REQUIRED`. |
| `/admin/students` | Search by University ID/name and open one student. | Search is bounded and the detail shows only the requested synthetic student, academic term, and provenance. Stale changes produce `STALE_VERSION`. |
| `/admin/catalogue` | Review the published 19-course catalogue and roadmap; use disposable edits to validate rules before publishing. | Every course is three credits, later roadmap subjects have prerequisites, invalid rule types and cycles are explained, and nothing invalid is published. |
| `/admin/offerings` | Review the seeded published groups. In disposable edits, try missing staff, room/time conflicts, bad capacity, then a valid change. | Every seeded group has complete Lecturer/TA/room/time data. Invalid groups remain unpublished and scheduling rules are server-authoritative. |
| `/admin/resources` | Review rooms/staff resources and availability. | Staff availability is read-only for Admin. No correction or override action exists. |
| `/admin/registrations` | Filter and inspect registration receipts/alerts. | The page is read-only. There is no drop, withdrawal, repair, or capacity override command. |
| `/admin/audit` | Apply bounded filters, page results, inspect immutable detail, request an export in disposable data. | Audit detail is immutable. Export shows queued/ready/failed/expired/restricted honestly and never exposes a server file path. |

Useful Admin codes to recognize:

```text
STALE_VERSION
FINAL_ADMIN_REQUIRED
WINDOW_OVERLAP
TERM_CODE_EXISTS
TERM_STATE_CONFLICT
IDEMPOTENCY_KEY_REUSED
VALIDATION_ERROR
FORBIDDEN
```

### Administrator authorization checks

- Open `/student`; no student profile may appear.
- Open `/staff/availability`; Admin must not receive editable staff-owned
  availability ranges.
- Confirm that Admin pages do not contain a hidden capacity, policy, conflict,
  or registration override.

## Lecturer manual test

Sign out, then sign in at `/staff/login` with:

```text
Username: LEC-0001
Password: Demo@2026-LEC-0001
Expected destination: /staff
Expected active role: Lecturer
```

| Route | What to do | What must be true |
|---|---|---|
| `/staff` | Review assignments. | Only the seeded server-authorized Lecturer lecture assignments appear. Tutorial assignments remain outside Lecturer scope. |
| `/staff/timetable` | Compare the seeded calendar and chronological list. | Subject, group, partner staff, room, day, start, and end match in both views. |
| `/staff/groups/{groupId}/roster` | Follow a real assigned group, then replace the ID with a random/unassigned GUID. | The real roster is bounded and shows only University ID, display name, and enrollment state. The unassigned group returns denied/not-found with no rows. |
| `/staff/availability` | Add/edit a labelled range. Try start equal to or after end, try overlap, then save a valid complete set. | Invalid ranges show a summary and do not save. A valid save shows the server version/time. Stale edits return `STALE_VERSION`; a closed deadline returns `AVAILABILITY_DEADLINE_PASSED`. |

Lecturer must be denied from:

```text
/admin
/admin/users
/admin/terms
/admin/offerings
```

## Teaching Assistant manual test

Sign out, then sign in at `/staff/login` with:

```text
Username: TA-0001
Password: Demo@2026-TA-0001
Expected destination: /staff
Expected active role: Teaching Assistant
```

Repeat the Lecturer page checks with the following differences:

- Only server-authorized Tutorial/Section or Laboratory assignments may
  appear. A Lecturer-only lecture group must not appear.
- A roster may show University ID, display name, and enrollment state only.
  It must not show GPA, academic standing, holds, contact details, grades, or
  transcript information.
- A known Lecturer-only or otherwise unassigned roster ID must return denied or
  not-found with no student rows.
- Admin routes must remain denied.

Useful staff codes to recognize:

```text
UNAUTHORIZED
FORBIDDEN
STAFF_GROUP_NOT_FOUND
PAGE_SIZE_INVALID
AVAILABILITY_RANGE_INVALID
STALE_VERSION
AVAILABILITY_DEADLINE_PASSED
```

## Public, recovery, and safe-error checks

### Public gateway

Open `/` while signed out. It should show privacy-safe server context and clear
Student and Staff destinations. It must not show a student's name, GPA,
schedule, or any credential.

### Recovery

Open `/account/recovery` and submit a valid and an unknown identifier. The
public response must not reveal which identifier exists. Development recovery
proofs are local, expiring, Git-ignored artifacts; do not copy their secret
values into this guide or a ticket.

### Safe errors

For any failed page or API request:

- Record the displayed reference/correlation ID.
- Confirm the same request has an `X-Correlation-ID` response header when
  checked in browser developer tools.
- Confirm there is no stack trace, SQL text, database connection string,
  password, activation secret, or foreign record.

Common registration/state codes include:

```text
WINDOW_CLOSED
WINDOW_CHANGED
GROUP_FULL
PLAN_CHANGED
POLICY_CHANGED
SCHEDULE_CONFLICT
REGISTRATION_HOLD
PREREQUISITE_NOT_COMPLETED
PROFILE_NOT_READY
SERVICE_UNAVAILABLE
```

These are stable reason names. The correlation/reference ID is generated per
request and is not one of these reason names.

## Supporting developer commands

Run these after manual testing to catch accidental regressions:

```powershell
dotnet build .\StudentRegistration.slnx --configuration Release

dotnet test `
    .\tests\StudentRegistration.Client.ContractTests\StudentRegistration.Client.ContractTests.csproj `
    --configuration Release

dotnet test `
    .\tests\StudentRegistration.Client.UnitTests\StudentRegistration.Client.UnitTests.csproj `
    --configuration Release

dotnet test `
    .\tests\StudentRegistration.IntegrationTests\StudentRegistration.IntegrationTests.csproj `
    --configuration Release `
    --filter 'FullyQualifiedName~Specs.Spec003|FullyQualifiedName~Specs.Spec007|FullyQualifiedName~Specs.Spec008|FullyQualifiedName~Specs.Spec016|FullyQualifiedName~Specs.Spec017'
```

For the longer browser suites:

```powershell
dotnet test `
    .\tests\StudentRegistration.E2ETests\StudentRegistration.E2ETests.csproj `
    --configuration Release

dotnet test `
    .\tests\StudentRegistration.AccessibilityTests\StudentRegistration.AccessibilityTests.csproj `
    --configuration Release `
    --filter 'FullyQualifiedName~Routes'
```

Browser automation supports manual testing; it does not replace the required
human usability participants.

## Record a human usability session

Use privacy-safe participant codes. Do not record names, University IDs,
passwords, screenshots containing secrets, or real student data.

Copy this block once for each participant:

```markdown
### Session <privacy-safe code>

- Participant group: Student / Admin / Lecturer / Teaching Assistant
- Representation: novice / keyboard-only / screen-reader / standard
- Date and local time:
- Browser and exact version:
- Operating system:
- Assistive technology and version, if used:
- Starting route:
- Assigned journey:
- Completed without help: Yes / No
- Completion time:
- Assistance requested:
- Observations:
- Defect IDs:
- Critical or major core-flow defect found: Yes / No
- Tester/facilitator:
```

The minimum valid cohort is:

| Group | Minimum unique people |
|---|---:|
| Students, including novice, keyboard-only, and screen-reader representation | 8 |
| Administrators | 3 |
| Lecturers | 3 |
| Teaching Assistants | 3 |
| **Total** | **17** |

With exactly 17 participants, at least 16 must complete their critical journey
to meet the 90% threshold. No critical or major core-flow usability defect may
remain unresolved. Automated personas and repeated sessions by the same person
do not increase the unique-human count.

Record the final privacy-safe results in
`docs/release-evidence/SPEC-003-NFR-8.md`. Do not check T266 until the cohort,
completion threshold, defect disposition, and Ahmed Elbamby's approval are all
recorded.

## Stop or fully reset the demo

Stop SQL Server but keep the database (first stop the web app with `Ctrl+C`):

```powershell
.\ops\scripts\Stop-LocalDemo.ps1
```

Delete the local Development database and start fresh:

```powershell
.\ops\scripts\Stop-LocalDemo.ps1 -ResetDatabase
```

The second command permanently deletes the local demo database. After it, run
the SQL start, migration, and seed steps again. Old credential artifacts are
not deleted with the Docker volume. Remove expired local artifacts separately:

```powershell
Get-ChildItem `
    '.\src\StudentRegistration.Api\.local\credentials\imports\import-*.json' |
    Where-Object LastWriteTimeUtc -lt (Get-Date).ToUniversalTime().AddDays(-7) |
    Remove-Item -Force
```

## Simple pass decision

The developer smoke test passes when every role can authenticate correctly,
every owned page shows either correct data or an honest empty state, forbidden
pages reveal no data, keyboard/zoom/screen-reader checks are usable, and no
critical or major defect remains open.

The NFR-8 human gate passes only when its separate 17-person cohort and 90%
completion rule also pass. Until then, developer verification may be recorded
as useful evidence, but it must not be described as completed human UAT.

For additional startup troubleshooting, see
[LOCAL_DEMO_GUIDE.md](LOCAL_DEMO_GUIDE.md). For the route map, see
[STORYBOARD.md](STORYBOARD.md).
