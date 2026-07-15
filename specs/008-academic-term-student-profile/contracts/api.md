# API Contract: Academic Term and Student Profile

**Standing approval**: Ahmed ELbamby approved this clarified non-production
demo contract on 2026-07-14. Production and later release gates remain
separate.

## Shared vocabulary

SPEC-006 owns `ApiError`, `Page<T>`, `TermSummaryDto`, `PublicContextDto`,
`AppContextDto`, and `RegistrationWindowSummaryDto`. SPEC-008 supplies their
academic values and MUST NOT define competing shared types.

The C# equivalents of the SPEC-008-specific transport declarations below are
logically owned by SPEC-008 and defined once in the dependency-neutral
`StudentRegistration.Contracts.Academics` namespace. The Contracts assembly
contains shapes and validation only; Academics retains all business behavior.

```typescript
type AcademicTermState =
  | "draft"
  | "registrationOpen"
  | "registrationClosed"
  | "teaching"
  | "completed"
  | "archived";

type RegistrationWindowLifecycleState =
  | "draft"
  | "published"
  | "emergencyClosed"
  | "superseded";

interface RegistrationWindowSummaryDto { // canonical SPEC-006 type
  id: string;
  state: "upcoming" | "open" | "closed";
  opensAtUtc: string;
  closesAtUtc: string;
  rowVersion: string;
}

interface AcademicHoldDto {
  termId: string;
  code: string;
  message: string;
  blocksRegistration: boolean;
  effectiveFromUtc: string;
  effectiveToUtc?: string;
  source: string;
}

interface AdminAcademicHoldDto extends AcademicHoldDto {
  holdId: string;
  sourceReference: string;
}

interface StudentAcademicContextDto {
  universityId: string;
  programCode: string;
  cohort: string;
  currentGpa: number;
  earnedCredits: number;
  standing: string;
  transcriptSummary: {
    attemptedCredits: number;
    earnedCredits: number;
    attemptCount: number;
  };
  transcriptAttempts: Page<{
    attemptId: string;
    supersedesAttemptId?: string;
    courseCode: string;
    termCode: string;
    credits: number;
    grade?: string;
    status: string;
    provenance: string;
  }>;
  activeHolds: AcademicHoldDto[];
  dataVersion: string;
  dataAsOfUtc: string;
  provenance: Page<{
    source: string;
    reference: string;
    importedAtUtc: string;
  }>;
}

interface AdminStudentAcademicContextDto
  extends Omit<StudentAcademicContextDto, "activeHolds"> {
  studentId: string;
  termId: string;
  activeHolds: AdminAcademicHoldDto[];
  studentRowVersion: string;
  studentTermStateRowVersion: string;
}

interface AdminRegistrationWindowDto {
  id: string;
  scopeType: "all-students" | "program" | "cohort";
  scopeValue?: string;
  opensAtUtc: string;
  closesAtUtc: string;
  lifecycleState: RegistrationWindowLifecycleState;
  computedState: "upcoming" | "open" | "closed";
  rowVersion: string;
}

interface AdminTermDto {
  id: string;
  code: string;
  displayName: string;
  timeZoneId: string;
  teachingStartsOn: string;
  teachingEndsOn: string;
  state: AcademicTermState;
  rowVersion: string;
  windows: AdminRegistrationWindowDto[]; // maximum 20
}

interface TermWindowInput {
  id?: string;
  scopeType: "all-students" | "program" | "cohort";
  scopeValue?: string;
  opensAtUtc: string;
  closesAtUtc: string;
  lifecycleState: RegistrationWindowLifecycleState;
}

interface TermInput {
  code: string;
  displayName: string;
  timeZoneId: string;
  teachingStartsOn: string;
  teachingEndsOn: string;
  state: AcademicTermState;
}

interface CreateTermRequest {
  clientRequestId: string;
  reason: string;
  source: string;
  term: TermInput;
  windows: TermWindowInput[]; // maximum 20; new windows must be draft
}

interface UpdateTermRequest {
  expectedTermRowVersion: string;
  expectedWindowRowVersions: Record<string, string>; // maximum 20
  reason: string;
  source: string;
  term: TermInput;
  windows: TermWindowInput[]; // maximum 20
}

interface PublishRegistrationWindowRequest {
  expectedTermRowVersion: string;
  expectedWindowRowVersion: string;
  reason: string;
  source: string;
}

type AcademicProfileCorrectionOperation =
  | { kind: "set-gpa"; currentGpa: number; sourceReference: string }
  | { kind: "set-earned-credits"; earnedCredits: number; sourceReference: string }
  | { kind: "set-standing"; standingCode: string; sourceReference: string }
  | { kind: "upsert-transcript-attempt"; supersedesAttemptId?: string; courseCode: string; termCode: string; credits: number; grade?: string; status: "in-progress" | "passed" | "failed" | "withdrawn"; sourceReference: string }
  | { kind: "upsert-hold"; holdId?: string; code: string; message: string; blocksRegistration: boolean; effectiveFromUtc: string; effectiveToUtc?: string; sourceReference: string }
  | { kind: "remove-hold"; holdId: string; sourceReference: string };

interface AcademicProfileCorrectionRequest {
  termId: string;
  expectedStudentRowVersion: string;
  expectedStudentTermStateRowVersion: string;
  reason: string;
  source: string;
  operations: AcademicProfileCorrectionOperation[]; // 1..20
}

interface AdminStudentLocatorDto {
  studentId: string;
  universityId: string;
  programCode: string;
  cohort: string;
  standing: string;
  dataVersion: string;
}
```

`PublicAcademicContextDto` is an alias for the exact six-field
`PublicContextDto`, not a second DTO. The shared authenticated `AppContextDto`
contains nullable `teachingTerm`, nullable `registrationTerm`, and nullable
`registrationWindow`; its service state is exactly `available`, `maintenance`,
or `unavailable`. Its required `supportReferencePath` is an application-relative
support route containing no student identifier or diagnostic secret.

## Authoritative context resolution

- All instants are normalized to UTC. Window containment uses the half-open
  interval `OpensAtUtc <= serverNowUtc < ClosesAtUtc`; a request received at
  `ClosesAtUtc` is closed.
- A service-ready context admits exactly one `RegistrationOpen` term. Zero is
  an authoritative no-registration-term result; more than one is invalid
  configuration and returns `503 CONTEXT_UNAVAILABLE` without a partial body.
- Zero or one `Teaching` term is allowed. A second Teaching term is rejected;
  legacy or concurrently detected ambiguity returns `503 CONTEXT_UNAVAILABLE`.
- Within the resolved registration term, applicable Published windows are
  filtered by authenticated student scope. Public context considers Published
  windows without exposing their scope. Selection is deterministic: select the
  open window; otherwise the earliest upcoming window ordered by
  `(OpensAtUtc, Id)`; otherwise the latest closed window ordered by
  `(ClosesAtUtc DESC, Id)`. No candidate produces state `none` and a null
  `registrationWindow`.
- Publication forbids any overlap between Published windows in the same term,
  regardless of scope. Direct Draft-to-Published changes through PUT are
  rejected; the publish endpoint owns that transition. Published interval and
  scope values are immutable. PUT may make an authorized, versioned
  `Published -> EmergencyClosed` or `Published -> Superseded` transition.

## Bounded validation contract

- Shared pages default to page 1/size 20 and permit at most 100 items.
  Transcript and provenance pages are independent.
- A term contains at most 20 registration windows; `windows` and
  `expectedWindowRowVersions` each contain at most 20 entries and their IDs
  must match for existing windows.
- A profile correction contains 1 through 20 operations. Duplicate scalar
  operations or contradictory operations on the same hold/attempt are rejected.
- `reason` is 10 through 500 trimmed characters. `source` and every
  `sourceReference` are nonblank and at most 200 trimmed characters.
- A supplied search query is 3 through 50 trimmed characters. The Admin student
  locator always requires both `termId` and a nonblank query; it is term-scoped,
  and it is not an unrestricted student dump.
- Feature-produced `ApiError.fieldErrors` contains at most 20 field keys, at
  most 5 messages per key, and at most 256 characters per message. Conflict
  diagnostics never contain an unbounded window, profile, or version collection.
- Every active hold is returned together, up to 100. More than 100 returns
  `409 PROFILE_NOT_READY`; the API never returns a partial hold set.

## Idempotency, concurrency, and cancellation

Term creation is the only SPEC-008 command that creates an aggregate without an
expected version. It therefore requires a nonblank payload-bound
`clientRequestId`. `AcademicTerm` stores globally unique
`CreationClientRequestId` and its server-canonical `CreationPayloadHash` in the
same transaction as the term, windows, and audit. Same-key/same-payload retry
replays the created aggregate; same-key/different-payload returns
`409 IDEMPOTENCY_KEY_REUSED`.

Window publication and profile correction use their required term/window or
student/student-term expected rowversions and one atomic transaction rather
than a second idempotency entity. A retry after their successful commit cannot
execute the same mutation again because its expected version is stale; the
authorized client refetches before deciding whether another command is needed.

Cancellation before commit rolls back audit and every mutation. Cancellation
or response loss after commit does not undo state. A create retry replays by
client request ID; a publication or correction retry receives the safe current
concurrency outcome and refetches. Transient failures commit no partial state.

## Endpoint contracts

All protected endpoints authorize before resource lookup or version disclosure.
Every unexpected failure returns `500 INTERNAL_ERROR` as canonical `ApiError`;
SQL/contributor unavailability returns `503` with no partial success body.

| # | Endpoint | Request and success | Authorization | Documented non-success outcomes |
|---|---|---|---|---|
| 01 | `GET /api/public/context` | No input; `200 PublicContextDto` | Anonymous | `503 CONTEXT_UNAVAILABLE`; `500 INTERNAL_ERROR`. Validation, 401/403, and conflict are not applicable because this is a public singleton read. |
| 02 | `GET /api/context` | No client time/term input; `200 AppContextDto` | Authenticated + `Context.Read` | `401`, `403`, `503 CONTEXT_UNAVAILABLE`, `500`. Validation and conflict are not applicable. |
| 03 | `GET /api/students/me/academic-context` | Independent transcript/provenance page parameters; `200 StudentAcademicContextDto` | Student + `AcademicProfile.ReadOwn` + ApplicationUser ownership | `400 PAGE_SIZE_INVALID`, `401`, `403`, `404 PROFILE_NOT_FOUND`, `409 PROFILE_NOT_READY`, `503`, `500`. |
| 04 | `GET /api/admin/terms` | Optional query (3..50), page, pageSize, state and allow-listed sort; `200 Page<AdminTermDto>` | `AcademicTerms.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`, `401`, `403`, `503`, `500`; conflict is not applicable. Default sort is `code,id`; allowed primary sorts are code, teaching start, and state, always ending in ID. |
| 05 | `POST /api/admin/terms` | `CreateTermRequest`; `201 AdminTermDto` | `AcademicTerms.Manage` + antiforgery | `400 VALIDATION_ERROR`, `401`, `403`, `409 TERM_CODE_EXISTS/TERM_STATE_CONFLICT/IDEMPOTENCY_KEY_REUSED`, `503`, `500`. |
| 06 | `PUT /api/admin/terms/{termId}` | `UpdateTermRequest`; `200 AdminTermDto` | `AcademicTerms.Manage` + antiforgery | `400 VALIDATION_ERROR`, `401`, `403`, authorized `404`, `409 STALE_VERSION/TERM_STATE_CONFLICT/WINDOW_OVERLAP`, `503`, `500`. `ApiError.currentVersion` is only the directly contested aggregate; refetch the bounded aggregate for all versions. |
| 07 | `POST /api/admin/terms/{termId}/registration-windows/{windowId}/publish` | `PublishRegistrationWindowRequest`; `200 AdminTermDto` | `AcademicTerms.Manage` + antiforgery | `400 VALIDATION_ERROR`, `401`, `403`, authorized `404`, `409 STALE_VERSION/WINDOW_OVERLAP`, `503`, `500`. |
| 08 | `GET /api/admin/students` | Required `termId` and query (3..50), page/pageSize and allow-listed sort; `200 Page<AdminStudentLocatorDto>` | `AcademicProfiles.Manage` | `400 PAGE_SIZE_INVALID/VALIDATION_ERROR`, `401`, `403`, `503`, `500`; conflict is not applicable. Default sort is `universityId,studentId`; allowed primary sorts are University ID, program, cohort, and standing, always ending in Student ID. |
| 09 | `GET /api/admin/students/{studentId}/academic-context` | Required `termId` plus independent transcript/provenance pages; `200 AdminStudentAcademicContextDto` | `AcademicProfiles.Manage` + named StudentId/TermId scope | `400`, `401`, `403`, authorized `404`, `409 PROFILE_NOT_READY`, `503`, `500`; conflict otherwise is not applicable. |
| 10 | `PATCH /api/admin/students/{studentId}/academic-profile` | `AcademicProfileCorrectionRequest`; `200 AdminStudentAcademicContextDto` | `AcademicProfiles.Manage` + named StudentId/TermId scope + antiforgery | `400 VALIDATION_ERROR`, `401`, `403`, authorized `404`, `409 STALE_VERSION/INVALID_SUPERSESSION/PROFILE_NOT_READY`, `503`, `500`. |

Term and profile commands require reason, source, expected versions where an
aggregate already exists, atomic privacy-safe audit, and server time. No client
role, browser clock, arbitrary property name, navigation property, credential,
or generic bulk overwrite is accepted.
Term commands require `AcademicTerms.Manage`; profile commands require the separately governed `AcademicProfiles.Manage` permission.
An Admin role without the exact permission claim is denied, and neither
permission substitutes for the other.

Student self-access and a named, permitted Admin request are allowed. Lecturer
and TeachingAssistant are denied academic-profile endpoints; their later roster
contracts expose only separately approved minimum fields.

## Transcript and hold correction rules

- Transcript attempts are immutable. `supersedesAttemptId` identifies a current
  leaf for the same student, course, and transcript term. It is normalized as a
  nullable identifier and cannot reference itself, a different chain, or an
  already-superseded row.
- A prior attempt has at most one direct successor. The resulting chain must be
  acyclic. Concurrent attempts to replace the same leaf serialize through
  `StudentTermAcademicState`; one succeeds and the other returns
  `409 INVALID_SUPERSESSION` or `STALE_VERSION`.
- Transcript summary counts only current leaves, so historical correction rows
  never double-count attempted or earned credits. The attempts page may include
  historical rows and exposes the link needed to explain supersession.
- `Code` and `Message` are normalized trimmed values. Student self responses do
  not expose hold IDs. Only the named Admin detail exposes `holdId` and sourced
  correction metadata needed for a governed update/removal.

SPEC-017 may orchestrate audit/report views but does not duplicate these owner
handlers. SPEC-014 consumes the student-term lock/version protocol but remains
responsible for real seat and enrollment conformance.
