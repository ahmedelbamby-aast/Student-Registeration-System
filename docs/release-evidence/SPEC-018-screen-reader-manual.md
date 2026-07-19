# SPEC-018 Manual Keyboard and Screen Reader Evidence

**Artifact version:** 1.0.0

**Requirement:** NFR-8 / SC-2

**Recorded:** 2026-07-18

**Status:** NOT EXECUTED

**Release gate:** BLOCKED

**Production authority:** Not granted

## Evidence boundary

This artifact remains the unsigned manual execution record. Static checks and
the automated evidence are now complete: all 36 combinations of nine critical
routes across Chrome, Edge, Firefox, and pinned Playwright WebKit passed axe,
keyboard/focus, target-size, and responsive checks. WebKit is not Safari;
actual Safari on macOS is deferred and not passed.

An objective Windows integration probe also passed with official NVDA 2026.1.1
and headed Chrome, recording 16 speech events, two focus events, and hashed
NVDA/TRX/log artifacts. Those automated results do not represent a human NVDA
usability verdict or signed manual evidence.

## Required evidence record

| Field | Current value |
|---|---|
| Tester | NOT RECORDED |
| Test date | NOT RECORDED |
| Operating system | Windows - NOT RECORDED |
| Assistive technology | NVDA |
| NVDA version | NOT RECORDED |
| Route | NOT EXECUTED |
| Scenario | NOT EXECUTED |
| Keyboard result | NOT EXECUTED |
| Screen-reader result | NOT EXECUTED |
| Overall result | NOT EXECUTED |
| Defect links | NOT RECORDED |
| UX sign-off | UNSIGNED |
| QA sign-off | UNSIGNED |

Every executed journey must retain all fields above. A route row without an
identified tester, date, exact NVDA version, scenario, result, defect status,
and both sign-offs remains invalid and blocks release.

## Machine-readable gate record

This block mirrors the release-gate fields for automated validation. Its
placeholder values are deliberately blocking and must be replaced only from
the signed manual test record after execution.

```text
tester: NOT RECORDED
date: NOT RECORDED
assistiveTechnology: NVDA version NOT RECORDED on Windows
route: NOT EXECUTED
scenario: NOT EXECUTED
result: NOT EXECUTED
defectLinks: NOT RECORDED
uxQaSignOff: UNSIGNED
```

## Critical journey template

| Route | Scenario | Keyboard result | NVDA result | Defect links | Status |
|---|---|---|---|---|---|
| AUTH-02 `/student/login` | Login, invalid credentials, validation-summary focus | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| AUTH-04 `/staff/login` | Shared staff login and authorized context | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STU-02 `/student/subjects` | Search, filters, eligibility and unavailable reasons | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STU-04 `/student/schedule` | Calendar/list equivalence and conflict resolution | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STU-05 `/student/review` | Blocking reasons and disabled submission | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STU-06 `/student/registration/result/{id}` | Accepted/rejected atomic result and recovery | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STF-01 `/staff` | Role context, assignments and warnings | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STF-03 `/staff/groups/{groupId}/roster` | Scoped roster, paging and access removal | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |
| STF-04 `/staff/availability` | Keyboard time-range entry and validation | NOT EXECUTED | NOT EXECUTED | NOT RECORDED | BLOCKED |

## Automated browser evidence

| Gate | Current result |
|---|---|
| Chrome axe and keyboard/focus journeys | PASS - 9/9 routes |
| Edge axe and keyboard/focus journeys | PASS - 9/9 routes |
| Firefox axe and keyboard/focus journeys | PASS - 9/9 routes |
| Playwright WebKit axe and keyboard/focus journeys | PASS - 9/9 routes |
| Official NVDA 2026.1.1 + headed Chrome integration probe | PASS - automated boundary only |
| Serious automated findings unresolved | 0 |

## Sign-off

| Review perspective | Name/signature | Date | Decision |
|---|---|---|---|
| Tester | UNSIGNED | NOT RECORDED | BLOCKED |
| UX | UNSIGNED | NOT RECORDED | BLOCKED |
| QA | UNSIGNED | NOT RECORDED | BLOCKED |

## Activation condition

Activation condition: an identified tester on Windows must complete the
keyboard and representative NVDA journeys, record defect links, and obtain UX
and QA sign-off. Until then, NFR-8 and the release gate remain blocked even
though all automated browser and NVDA integration evidence passes.
