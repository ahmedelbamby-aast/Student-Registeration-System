# Policy Source, Provenance, Conflict, and Approval Register

**Schema:** `policy-source-register/1.0`
**Profile:** `DEMO-POC-2026.1`
**Implementation verification date:** 2026-07-13

Public AASTMT material establishes provenance only. Ahmed ELbamby's approval
authorizes this non-production demo profile; it is not official AASTMT
production approval.

## Source records

| ID | Classification | Authority | URL/reference | Accessed | Affected facts | Approval/status |
|---|---|---|---|---|---|---|
| `SRC-GENERAL-2016` | `OfficialAASTMT` | AASTMT Education Affairs Sector | https://aast.edu/openfiles/opencmsfiles/pdf_retreive_cms_open.php?disp_unit=390%2FEducationRegulations2016.pdf | 2026-07-13 | Electronic registration; prerequisite completion; regular 9/18 limits; GPA below 2.0 and 12-credit probation cap; sourced repeat statements | Source recorded; production interpretation still requires institutional approval |
| `SRC-REGULATIONS-PAGE` | `OfficialAASTMT` | AASTMT Education and Student Affairs | https://aast.edu/en/vice/edu-affairs/contenttemp.php?page_id=39000019 | 2026-07-13 | Current host page for education/study regulations | Source recorded; page has no policy-version guarantee |
| `SRC-CAI-EDUCATION` | `OfficialAASTMT` | College of Artificial Intelligence | https://aast.edu/en/colleges/CAI/elalamein/contenttemp.php?page_id=65500014 | 2026-07-13 | College education-system context | Source recorded; no clear publication/version date |
| `SRC-CAI-GRADING` | `OfficialAASTMT` | College of Artificial Intelligence | https://aast.edu/en/colleges/CAI/elalamein/contenttemp.php?page_id=65500005 | 2026-07-13 | Grading/repeat research context; not an enabled registration rule | In Review outside bounded demo behavior |
| `SRC-DATA-SCIENCE` | `OfficialAASTMT` | College of Artificial Intelligence | https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655 | 2026-07-13 | Codes, titles, term sequence, displayed prerequisites/conditions for the 19 selected rows | Approved provenance for snapshot fields only |
| `SRC-INTELLIGENT-SYSTEMS` | `OfficialAASTMT` | College of Artificial Intelligence | https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=277&unit_id=655 | 2026-07-13 | Secondary comparison and invalid-import fixture | Not part of the 19-course seed |
| `DEMO-APPROVAL-2026.1` | `AhmedApprovedDemo` | Ahmed ELbamby | `specs/002-aastmt-policy-rulebook/checklists/approval.md` | 2026-07-13 | Demo profile, 18-credit target, capacity allocation, overlap behavior, exclusions | Approved for non-production demo only |
| `DEMO-CREDITS-3` | `SyntheticDemo` | Ahmed ELbamby | `docs/DEMO_CURRICULUM.md` | 2026-07-13 | Three-credit value on every selected course | Approved synthetic field; never attributed to AASTMT |
| `DEMO-CAPACITY-FIRST-COMMIT` | `AhmedApprovedDemo` | Ahmed ELbamby | `docs/POLICY_RESEARCH.md#approved-simple-demo-poc-profile` | 2026-07-13 | First successful commit, no waitlist/reservation/override | Approved for demo only |
| `DEMO-CONFLICT-EXACT` | `AhmedApprovedDemo` | Ahmed ELbamby | `docs/POLICY_RESEARCH.md#approved-simple-demo-poc-profile` | 2026-07-13 | Exact overlap blocks, travel buffer zero, no override | Approved for demo only |
| `UNRESOLVED-REPEAT-WORKFLOW` | `UnresolvedInstitutional` | Registrar/College approval required | `docs/POLICY_RESEARCH.md#simplified-research-baseline-approval-status-shown-per-rule` | 2026-07-13 | Which repeats may be registered online and any advisor/manual conditions | In Review; `REPEAT_POLICY_UNAVAILABLE` fails closed |
| `UNRESOLVED-PRODUCTION-VALUES` | `UnresolvedInstitutional` | Registrar/College approval required | `docs/POLICY_RESEARCH.md#demo-resolutions-for-unverified-or-product-owned-questions` | 2026-07-13 | Out-of-profile exceptions, withdrawal/drop, advisor workflow, production retention | In Review and fail closed |

## Field-level curriculum provenance

For each of the 19 published rows:

- `Code`, `Title`, `Sequence`, and each displayed prerequisite/condition are
  `OfficialAASTMT` fields referencing `SRC-DATA-SCIENCE`.
- `Credits=3` is a `SyntheticDemo` field referencing `DEMO-CREDITS-3`.
- Excluded missing/inconsistent references remain invalid-import fixtures; they
  are not silently repaired.

Synthetic gap rows: none in `DEMO-POC-2026.1`. A future wholly synthetic row
must use a `DEMO-` code, classification `SyntheticDemo`, a gap label, rationale,
and a visible “not AASTMT-published” disclaimer.

## Conflict resolution and historical ownership

- The general 9-credit minimum wins for the demo over the College page's usual
  12-credit guidance because Ahmed explicitly approved the bounded demo choice.
- Withdrawal timing is not resolved because withdrawal is out of scope.
- Capacity and conflict behavior are product-owned demo rules because no
  authoritative public College rule was found.
- A source outage marks the source for review but cannot rewrite a published
  version or its historical decisions.
- SPEC-009 owns runtime policy aggregates/evaluation. SPEC-015 owns immutable
  historical decision snapshots including the source metadata recorded here.
