# SPEC-002 T038-T041 Scope Review

**Reviewed:** 2026-07-13  
**Repository scope:** current tracked-style source tree, excluding generated
`bin`, `obj`, and `tmp` content  
**Items:** T038/OS-1, T039/OS-2, T040/OS-3, T041/OS-4

## Verdict

| Item | Excluded behavior | Verdict | Repository evidence |
|---|---|---|---|
| OS-1 | Legal interpretation by software | **PASS** | No production implementation was found. The governed contract applies only approved typed demo rules; unresolved repeat interpretation returns `REPEAT_POLICY_UNAVAILABLE` and fails closed. |
| OS-2 | Arbitrary executable scripts or expressions uploaded by users | **PASS** | No execution-engine dependency or production implementation was found. The closed schema has nine typed rule kinds, the registry says `RejectWithoutStoreOrRun`, and negative acceptance coverage rejects executable input without storage. |
| OS-3 | Waitlists, capacity/conflict overrides, automatic exceptions, add/drop, withdrawal, and Advisor or Deanery approval workflows | **PASS** | No production implementation, migration, concrete route, or prohibited contract endpoint was found. Waitlist/override settings are fixed false or rejection probes, every demo blocking result has `overridePossible=false`, and owner contracts explicitly omit enrollment drop/withdrawal endpoints. |
| OS-4 | Automatic dismissal or academic-path decisions | **PASS** | No production implementation, migration, route, or academic-path mutation contract was found. Existing standing values are authoritative inputs used to gate registration; the software does not derive dismissal, change standing automatically, or choose a program/major/path. |

These PASS verdicts mean the four behaviors remain excluded in the repository
snapshot reviewed. This scope review does not authorize a system or production release.

## Inspected scope and commands

Commands were run from the repository root. Searches excluded generated build
output so framework binaries and cached package content could not hide or
inflate repository-owned findings.

```powershell
$sourceFiles = @(rg --files src -g '*.cs' -g '*.razor' -g '*.csproj' -g '*.json' -g '*.html' -g '!**/bin/**' -g '!**/obj/**')
$contractFiles = @(rg --files specs -g '**/contracts/*.md')
$spec002Schemas = @(rg --files specs/002-aastmt-policy-rulebook/schemas)
$testFiles = @(rg --files tests -g '*.cs' -g '*.csproj' -g '*.json' -g '!**/bin/**' -g '!**/obj/**')

rg -n -i --glob '!**/bin/**' --glob '!**/obj/**' --glob '*.{cs,razor,json,html,csproj}' 'legal[[:space:]_-]*interpret|user[[:space:]_-]*defined[[:space:]_-]*expression|executable[[:space:]_-]*(policy|script|expression)|waitlist|capacity[[:space:]_-]*override|conflict[[:space:]_-]*override|automatic[[:space:]_-]*exception|add[[:space:]/_-]*drop|withdraw(al)?|advisor|deanery|academic[[:space:]_-]*dismiss|academic[[:space:]_-]*path' src

rg -n -i --glob '!**/bin/**' --glob '!**/obj/**' --glob '*.{cs,json}' 'legal[[:space:]_-]*interpret|user[[:space:]_-]*defined[[:space:]_-]*expression|executable[[:space:]_-]*(policy|script|expression)|waitlist|capacity[[:space:]_-]*override|conflict[[:space:]_-]*override|automatic[[:space:]_-]*exception|add[[:space:]/_-]*drop|withdraw(al)?|advisor|deanery|academic[[:space:]_-]*dismiss|academic[[:space:]_-]*path' tests

rg -n -i --glob '**/contracts/*.md' 'legal|interpret|script|expression|waitlist|override|exception|add[ /_-]*drop|withdraw|advisor|deanery|dismiss|academic[ -]*path|degree[ -]*path|program[ -]*(change|selection)|major[ -]*(change|selection)' specs

rg --files src -g '*Migration*.cs' -g '*ModelSnapshot.cs' -g '*.sql' -g '!**/bin/**' -g '!**/obj/**'
rg -n --glob '!**/bin/**' --glob '!**/obj/**' --glob '*.{cs,razor}' '@page|Map(Get|Post|Put|Delete|Patch|Group|Methods)|\[(Route|Http(Get|Post|Put|Delete|Patch))' src
rg -n -i --glob '*.{cs,csproj,props,targets}' --glob '!**/bin/**' --glob '!**/obj/**' 'CSharpScript|Microsoft\.CodeAnalysis\.Scripting|System\.Linq\.Dynamic|DynamicExpresso|ClearScript|IronPython|Jint|NCalc|Flee' src tests
```

| Surface | Observed result |
|---|---:|
| Production surface | 29 files in one Blazor WebAssembly client project |
| Contract surface | 29 Markdown contract files across SPEC-001 through SPEC-018 |
| SPEC-002 rule schemas | 2 JSON files |
| Test surface | 86 C#/project/JSON files |
| Production targeted domain hits | 0 |
| Test targeted domain hits requiring classification | 23 |
| Source migration/model-snapshot/SQL files | 0 |
| Concrete source route/handler declarations | 0 |
| Contract endpoint lines | 64 |
| Contract endpoint lines containing a prohibited OS-1..OS-4 term | 0 |
| Dynamic execution-engine dependency/code hits | 0 |

The production dependency inventory contains only
`Microsoft.AspNetCore.Components.WebAssembly` and its development server. No
ASP.NET Core server, EF Core, SQL Server, policy-runtime, or migration project
exists in the current source tree.

## Occurrence classification

| Occurrence | Location | Classification |
|---|---|---|
| `RequiresRepeatInterpretation`, PB-17, and `REPEAT_POLICY_UNAVAILABLE` | SPEC-002 test harness and NFR/acceptance fixtures | **Fail-closed guard for OS-1.** The flag detects that an unapproved interpretation would be needed; it does not make that interpretation. |
| `ExecutableExpression`, `UserDefinedExpression`, `EXECUTABLE_POLICY_CONTENT_FORBIDDEN`, and `StoredExecutableContent=false` | SPEC-002 test harness and AC-4 | **Negative probe/guard for OS-2.** TestSupport is compiled into test assemblies, not the Blazor production project. Validation rejects the input and reports that nothing executable was stored. |
| `executableContentOutcome=RejectWithoutStoreOrRun`; absence of `expression`/`script` properties | SPEC-002 type registry, closed JSON schema, and schema tests | **Closed-contract guard for OS-2.** `additionalProperties=false` and the nine reviewed type keys prevent an arbitrary rule shape. |
| `PolicyRuleAdminDto.value` limited to number, boolean, string, or string-list | SPEC-009 contract | **Typed future contract, not a script surface.** The future runtime must bind rule codes to the SPEC-002 closed registry; this review does not claim that unimplemented enforcement already exists. |
| HTML `<script>` boot tags | `src/StudentRegistration.Client/wwwroot/index.html` | **Framework infrastructure, not OS-2.** They load the static Blazor WebAssembly boot assets and do not accept user policy code. |
| `waitlistEnabled=false`, capacity/meeting `overrideEnabled=false`, and `overridePossible=false` | SPEC-002 schema and decision/rule-coverage contracts | **Disabled representation for OS-3.** These fields make the prohibition explicit; they expose no enablement command. |
| `WaitlistEnabled`/`CapacityOverrideEnabled` true inputs and `WAITLIST_NOT_APPROVED`/`CAPACITY_OVERRIDE_NOT_APPROVED` | SPEC-002 test harness and AC-2 | **Negative probe/guard for OS-3.** Publication is rejected and a separately approved amendment is required. |
| Withdrawal, advisor, add/drop, correction, and override wording | SPEC-002 source-resolution/rule-coverage contracts and SPEC-015/SPEC-017 API contracts | **Documented exclusion for OS-3.** The text says the behaviors or endpoints do not exist; it is not an implementation. |
| Transcript-attempt status `withdrawn` | SPEC-008 academic-profile contract | **Representational fact, not an OS-3 workflow.** It records a sourced historical/imported transcript status through an audited profile correction; it does not drop a current enrollment, decrement a seat, or expose a registration-withdrawal endpoint. Any future use that performs those actions requires separate approval. |
| `set-standing` academic-profile correction and read-only `programCode` | SPEC-008 contract | **Human-authorized sourced correction/read model, not OS-4 automation.** There is no automatic dismissal, program/major selection, or academic-path command. |
| `AcademicStanding` and `ProbationLoad` rules | SPEC-002 closed rule schema/coverage and test harness | **Registration gates, not OS-4 decisions.** They consume authoritative standing/GPA and limit registration; they never calculate or mutate standing, dismissal, or academic path. |
| `IsDismissible`, `OnDismiss`, and dismiss styling/tests | Blazor Alert component and component tests | **Lexical false positive for OS-4.** This dismisses a UI message only. |
| `OverrideHtmlAssetPlaceholders`, C# `override`, and `*Exception` types | Client project/source | **Build/language false positives.** They are unrelated to capacity/conflict overrides or automatic academic exceptions. |
| “advisory capacity” and “executable authorization policy” | test/support and RBAC contract language | **Lexical false positives.** “Advisory” is not an Advisor workflow; executable authorization is fixed access-control enforcement, not uploaded policy code. |
| Scope-review token checks | `Spec002ReleaseEvidenceTests` | **Evidence assertion.** These test strings require this review to state all four exclusions and the release limitation; they implement none of the behaviors. |

All 23 targeted test hits fall into the negative guard/probe, documented
exclusion, evidence assertion, or lexical-false-positive groups above. No
positive executable implementation was found.

## Per-item evidence

### OS-1 — PASS: Legal interpretation remains excluded

The only interpretation-shaped executable-test path is repeat eligibility. It
returns `REPEAT_POLICY_UNAVAILABLE` whenever a repeat would require an
institutional interpretation. Source conflicts remain `In Review` until a
human-approved, versioned rule resolves the bounded demo behavior. The fixed
credit, capacity, and conflict checks apply Ahmed-approved demo values; they
do not claim to determine law or official institutional meaning.

### OS-2 — PASS: Arbitrary executable content remains excluded

No scripting/evaluation package, compile API, dynamic-expression library,
upload endpoint, storage migration, or production evaluator was found. The
negative publication fixture accepts a hostile expression only as test input,
then rejects it with `EXECUTABLE_POLICY_CONTENT_FORBIDDEN` and
`StoredExecutableContent=false`. The governed schema and registry contain only
closed typed data.

### OS-3 — PASS: Enrollment exceptions and approval workflows remain excluded

No waitlist, override, exception, add/drop, enrollment-withdrawal, Advisor, or
Deanery route/migration/production code was found. Capacity is first successful
commit, conflicts block, and the contract's `overridePossible` value is false.
Cross-spec contracts explicitly state that enrollment correction, drop,
withdrawal, seat decrement, and related endpoints do not exist. The one
`withdrawn` transcript status is historical academic-profile data, not a
registration mutation.

### OS-4 — PASS: Automatic academic-outcome decisions remain excluded

No dismissal/path engine or mutation was found. `AcademicStanding` checks only
whether an authoritative current standing permits registration; an unknown
value fails closed. Probation affects the permitted registration credit load
only. SPEC-008's sourced Admin correction can record an externally determined
standing, but nothing in the reviewed surface derives dismissal, changes a
program/major, or selects an academic path.

## Targeted verification

```powershell
dotnet test tests/StudentRegistration.AcceptanceTests/StudentRegistration.AcceptanceTests.csproj --configuration Release --filter 'FullyQualifiedName~Specs.Spec002.AC_2Tests|FullyQualifiedName~Specs.Spec002.AC_4Tests' --no-restore
dotnet test tests/StudentRegistration.SpecificationTests/StudentRegistration.SpecificationTests.csproj --configuration Release --filter 'FullyQualifiedName~PolicyRuleTypeRegistryTests|FullyQualifiedName~PolicyEvidenceSchemaTests|FullyQualifiedName~PolicySourceResolutionTests|FullyQualifiedName~Scope_review_records_all_four_exclusions' --no-restore
```

| Release test set | Result |
|---|---:|
| AC-2 and AC-4 negative publication behavior | 2 passed, 0 failed, 0 skipped |
| Closed schema/registry, source resolution, and scope-review evidence | 8 passed, 0 failed, 0 skipped |

## Runtime-owner and release limitation

SPEC-002 owns the rulebook shape, provenance, closed rule registry, and
fail-closed semantics. It owns no API handler. SPEC-009 is the future runtime
owner for policy aggregates/evaluation, SPEC-011 owns student eligibility
projection, SPEC-014 owns atomic registration, and SPEC-015 owns immutable
decision history.

Because those server and persistence implementations are not present, this
review proves only that the prohibited capabilities are absent from the
current repository implementation surface and remain excluded by current
contracts/tests. It cannot prove future runtime behavior, deployed binaries,
database contents, configuration, or infrastructure. Each runtime owner must
repeat the source/route/migration/dependency scan and add authorization,
integration, and persistence evidence before its own release gate. A later
feature that adds any OS-1 through OS-4 behavior requires a separately approved
specification and cannot rely on this PASS result.
