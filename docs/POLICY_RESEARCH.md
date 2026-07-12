# AASTMT Policy Research

## Status and authority

Research accessed 12 July 2026. Use an effective-dated policy baseline named
AASTMT-General-2016 plus a College-of-AI overlay. Public sources are enough for
a prototype but not production approval.

Primary sources:

1. [AASTMT Education and Study Regulations, 21 March 2016, Final](https://aast.edu/openfiles/opencmsfiles/pdf_retreive_cms_open.php?disp_unit=390%2FEducationRegulations2016.pdf)
2. [Current Education and Student Affairs regulations page](https://aast.edu/en/vice/edu-affairs/contenttemp.php?page_id=39000019)
3. [College of Artificial Intelligence Regulations](https://aast.edu/en/colleges/CAI/elalamein/contenttemp.php?page_id=65500007)
4. [College of Artificial Intelligence Education System](https://aast.edu/en/colleges/CAI/elalamein/contenttemp.php?page_id=65500014)
5. [College of Artificial Intelligence Grading System](https://aast.edu/en/colleges/CAI/elalamein/contenttemp.php?page_id=65500005)
6. [AASTMT live Academic Calendar](https://aast.edu/en/admission/calendar.php)
7. [Intelligent Systems program curriculum](https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=277&unit_id=655)
8. [Data Science program curriculum](https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655)

The public College pages show no clear publication/version date and must be
stored with an access date. The 2018 amendment linked on the general page
concerns Maritime credit transfers and is not used as a College-of-AI rule.

## Simplified, verified rules

| Code | Engine rule | Source status |
|---|---|---|
| TERM-01 | Fall/Spring: 15 teaching weeks + 2 exam weeks; optional Summer: 5 + 1. Dates come from configured calendar. | General final regulations + CAI page |
| LOAD-01 | Regular hard load is 9-18 credits; graduating students may be below 9. | General final regulations |
| LOAD-02 | CAI describes 12-18 as usual. Treat 12 as recommendation, not hard minimum, until confirmed. | Current CAI page |
| LOAD-03 | GPA below 2.0 means probation, max 12 credits, and advisor-selected courses. | General final regulations |
| LOAD-04 | GPA at least 3.0 may add one course if class and final-exam schedules allow. | General final regulations |
| LOAD-05 | Last two semesters may reach 21 credits only after individual College review. | General final regulations |
| LOAD-06 | Summer maximum is two courses; critical exception needs Academy President approval. | General final regulations |
| LOAD-07 | If too few eligible courses exist, load may be below the usual minimum. | General final regulations |
| PRE-01 | Prerequisites must be completed before registration. | General final regulations |
| PRE-02 | Graduating prerequisite exceptions require College review; never automatic. | General final regulations |
| PRE-03 | Higher-semester courses are allowed when prerequisites are complete. | General final regulations |
| PRE-04 | If an exceptional prerequisite/dependent pair is co-registered, dropping prerequisite drops both. | General final regulations |
| STAND-01 | Underachievement: earned hours below 50% of expected hours since admission. | General final regulations |
| STAND-02 | Probation/underachievement creates warnings and advisor intervention; three consecutive terms guide a new path. Dismissal is reviewed, not automatic. | General final regulations |
| ADDDROP-01 | Add/drop is within configured announced dates, with load/rule validation and advisor consultation. | General final regulations |
| WITHDRAW-01 | Final 2016 regulation permits withdrawal through week 15 with department-head/dean approval and W. Older summaries say week 14, so confirm. | Contradictory; production blocked |
| REPEAT-01 | Failed core courses must be repeated; attempts remain in transcript/GPA history. | General + CAI |
| REPEAT-02 | Successful failed-course retake is capped at B / 3.00. | General + current CAI |
| REPEAT-03 | Passed courses may normally be repeated for improvement up to five courses and within one academic year; probation/advisor exception exists. | General final regulations |
| ATTEND-01 | More than 15% absence without accepted excuse causes forced withdrawal; with official excuse, 20% causes withdrawal; result is W. | General final regulations |
| GRAD-01 | CAI programs are eight terms / 144 credits; general graduation GPA minimum 2.0. | CAI curriculum + general |
| GRADE-01 | CAI requires at least 50/100 total and 12/40 in final assessment to pass. | Current CAI |
| GRADE-02 | Grade scales differ before University ID 2310-00000 and from that 2023 cohort; calculation is cohort-versioned. | Current CAI |

## Course-specific examples

- Intelligent Systems IN413 Project I: GPA >= 2.0 and earned credits >= 96.
- Intelligent Systems IM423 Operations Research: earned credits >= 90.
- Data Science DS413 Project I: GPA >= 2.0 and earned credits >= 96.
- Project II requires Project I.

These demonstrate why prerequisites require minimum-grade, GPA, earned-credit,
program, and cohort dimensions.

## Current calendar example

On 12 July 2026, the public calendar shows Summer 2025-2026:

- Registration/fees: 5 July 2026.
- Teaching begins: 11 July 2026.
- Final exams begin: 15 August 2026.
- Results: 22 August 2026.

The public event listing does not show a summer registration closing timestamp.
Production must use an approved explicit OpensUtc and ClosesUtc record, never
infer an open window from the month or device clock.

## Unverified, contradictory, or product-owned decisions

| ID | Issue | Safe initial behavior / required owner |
|---|---|---|
| POLICY-Q01 | No public CAI capacity/waitlist/seat-allocation policy found. | First-commit-wins within approved group capacity; Registrar/Deanery approval required. |
| POLICY-Q02 | No CAI authority for automatic conflict exceptions. | Hard block every unresolved meeting overlap; no override. |
| POLICY-Q03 | 2017 summary suggests GPA/earned-credit portal restriction; final regulation permits electronic/advisor registration. | Do not lock lower-GPA students out; prepare plan and require approved path after SME decision. |
| POLICY-Q04 | Withdrawal deadline appears as week 15 in final rules and week 14 in older sources. | Configure but do not publish until CAI confirms. |
| POLICY-Q05 | CAI says usual minimum 12; general hard minimum is 9. | Enforce 9 and present 12 as guidance until confirmed. |
| POLICY-Q06 | Public curriculum contains missing/misordered prerequisite references. | Prototype seed only; obtain approved status report/catalogue before production. |
| POLICY-Q07 | Advisor approval workflow details are absent. | Out of MVP until actors, states, deadlines, and authority are approved. |
| POLICY-Q08 | Data retention/privacy periods are absent. | Security/privacy owner and AASTMT records authority must define before go-live. |

Examples of public catalogue integrity issues include GN211 shown before GN121,
references to absent IN321/DS121, and missing displayed DS222. Imports therefore
need source provenance, preview, referential validation, and a publish gate.

## Policy representation

PolicySet:

- Name/version and approval status.
- EffectiveFrom/EffectiveTo.
- Academic term, campus, college, program, cohort and standing scope.
- Priority and source/access date.

Typed rule configuration:

- CreditLoad, Prerequisite, MinimumGpa, MinimumEarnedCredits, Repeat,
  RegistrationWindow, Withdrawal, AcademicStanding, Conflict, and Capacity.

Every decision records:

- Passed/failed reason code and plain-language explanation.
- Policy version, source URL, and access/effective date.
- Input summary used for the decision.
- Whether an approved manual review is possible.

Do not store arbitrary executable expressions or scripts.

## Approval checklist

- Registrar confirms which source wins for every conflict.
- College confirms program/campus-specific overlays and course catalogue.
- Registrar supplies at least two example students per boundary.
- Admin imports verified term windows and curriculum.
- Product/Registrar approve conflict and capacity product rules.
- Security/privacy owner approves retention and data scope.
- QA converts approved examples into immutable rule regression tests.
