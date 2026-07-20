# SPEC-018 Manual Keyboard and Screen Reader Evidence

**Artifact version:** 1.1.0

**Requirement:** NFR-8 / SC-2

**Recorded:** 2026-07-20

**Status:** WAIVED-DEMO

**Release gate:** WAIVED-DEMO

**Production authority:** Not granted

**Approved by:** Ahmed ELbamby

**Approval date:** 2026-07-20

**Execution guide:**
[`../SPEC018_NVDA_MANUAL_TESTING_GUIDE.md`](../SPEC018_NVDA_MANUAL_TESTING_GUIDE.md)

## Demo waiver

Ahmed ELbamby, the sole human approval authority for this non-production
design-capability demo, decided on 2026-07-20 that manual NVDA scenarios are not
required for the demo. This is an explicit scope waiver. It is not a statement
that a person executed or passed the manual scenarios.

The waiver does not approve production use, does not satisfy a future
production accessibility review, and does not convert unperformed manual work
into a test pass. If this project moves beyond the current demo, the manual
journeys in the linked guide must be executed and signed.

## Evidence boundary

Automated accessibility evidence remains valid. All 36 combinations of nine
critical routes across Chrome, Edge, Firefox, and pinned Playwright WebKit
passed the recorded axe, keyboard/focus, target-size, and responsive checks.
WebKit is not Safari; actual Safari on macOS remains deferred.

An objective Windows integration probe also passed with official NVDA 2026.1.1
and headed Chrome. It recorded 16 speech events, two focus events, and hashed
NVDA/TRX/log artifacts. This automated probe is not represented as a human
NVDA usability verdict.

## Manual execution record

| Field | Current value |
|---|---|
| Tester | NOT PERFORMED - DEMO WAIVER |
| Test date | NOT PERFORMED - DEMO WAIVER |
| Operating system | NOT ASSESSED MANUALLY |
| Assistive technology | NVDA - MANUAL EXECUTION NOT REQUIRED FOR DEMO |
| NVDA version | NOT ASSESSED MANUALLY |
| Route | NINE CRITICAL ROUTES - MANUAL EXECUTION WAIVED |
| Scenario | MANUAL KEYBOARD AND NVDA JOURNEYS WAIVED FOR DEMO |
| Keyboard result | NOT PERFORMED - AUTOMATED EVIDENCE PASSED |
| Screen-reader result | NOT PERFORMED - AUTOMATED PROBE PASSED |
| Overall result | WAIVED-DEMO |
| Defect links | NOT APPLICABLE - NO MANUAL SESSION |
| UX sign-off | NOT REQUIRED FOR DEMO - AHMED WAIVER |
| QA sign-off | NOT REQUIRED FOR DEMO - AHMED WAIVER |

## Machine-readable gate record

```text
tester: NOT PERFORMED - DEMO WAIVER
date: 2026-07-20
assistiveTechnology: NVDA manual execution not required for demo
route: nine critical routes - manual execution waived
scenario: manual keyboard and NVDA journeys waived for demo
result: WAIVED-DEMO
defectLinks: NOT APPLICABLE - NO MANUAL SESSION
uxQaSignOff: NOT REQUIRED FOR DEMO - AHMED WAIVER
approvedBy: Ahmed ELbamby
approvedOn: 2026-07-20
productionAuthorized: false
```

## Critical journey disposition

| Route | Scenario | Automated evidence | Manual result | Status |
|---|---|---|---|---|
| AUTH-02 `/student/login` | Login, invalid credentials, validation-summary focus | PASS | NOT PERFORMED | WAIVED-DEMO |
| AUTH-04 `/staff/login` | Shared staff login and authorized context | PASS | NOT PERFORMED | WAIVED-DEMO |
| STU-02 `/student/subjects` | Search, filters, eligibility and unavailable reasons | PASS | NOT PERFORMED | WAIVED-DEMO |
| STU-04 `/student/schedule` | Calendar/list equivalence and conflict resolution | PASS | NOT PERFORMED | WAIVED-DEMO |
| STU-05 `/student/review` | Blocking reasons and disabled submission | PASS | NOT PERFORMED | WAIVED-DEMO |
| STU-06 `/student/registration/result/{id}` | Accepted/rejected atomic result and recovery | PASS | NOT PERFORMED | WAIVED-DEMO |
| STF-01 `/staff` | Role context, assignments and warnings | PASS | NOT PERFORMED | WAIVED-DEMO |
| STF-03 `/staff/groups/{groupId}/roster` | Scoped roster, paging and access removal | PASS | NOT PERFORMED | WAIVED-DEMO |
| STF-04 `/staff/availability` | Keyboard time-range entry and validation | PASS | NOT PERFORMED | WAIVED-DEMO |

## Automated evidence

| Gate | Current result |
|---|---|
| Chrome axe and keyboard/focus journeys | PASS - 9/9 routes |
| Edge axe and keyboard/focus journeys | PASS - 9/9 routes |
| Firefox axe and keyboard/focus journeys | PASS - 9/9 routes |
| Playwright WebKit axe and keyboard/focus journeys | PASS - 9/9 routes |
| Official NVDA 2026.1.1 + headed Chrome integration probe | PASS - automated boundary only |
| Serious automated findings unresolved | 0 |
| Critical/major manual barriers unresolved | NOT ASSESSED - MANUAL DEMO WAIVER |

## Demo approval

| Review perspective | Name | Date | Decision |
|---|---|---|---|
| Demo scope authority | Ahmed ELbamby | 2026-07-20 | WAIVED-DEMO |
| Manual tester | Not required for demo | 2026-07-20 | NOT PERFORMED |
| UX | Ahmed ELbamby as sole demo approval authority | 2026-07-20 | APPROVED DEMO WAIVER |
| QA | Ahmed ELbamby as sole demo approval authority | 2026-07-20 | APPROVED DEMO WAIVER |

## Future production activation condition

Before any production or institutional accessibility claim, an identified
tester on Windows must complete the keyboard and representative NVDA journeys,
record real results and defect links, close and retest critical or major
barriers, and obtain the required production UX and QA sign-offs. The optional
execution guide contains the full procedure.
