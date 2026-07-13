# Implementation Plan: Frontend Page Design, Storyboard, Accessibility and Functional Testing

**Branch**: 003-ux-storyboard-accessibility | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Approved for Gate A demo implementation on 2026-07-13; all 27 Page
Design Records are approved as design-only version 1.0 records under the 1.1
governance contract. Route-owner contract pins, Gates B-D, and production
release approval remain required.

## Summary

Deliver Frontend Page Design, Storyboard, Accessibility and Functional Testing inside the modular monolith while keeping server-side academic and authorization decisions authoritative.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Blazor WebAssembly, Entity Framework Core, LINQ
**Storage**: SQL Server with Code First migrations
**Testing**: xUnit plus API, integration, concurrency, accessibility, and browser tests as applicable
**Project Type**: Web application with hosted WebAssembly client and server API
**Performance Goals**: Governed by SPEC-018 and feature NFRs
**Constraints**: Atomic writes, WCAG 2.2 AA, stateless APIs, no client-authoritative decisions
**Scale/Scope**: Registration-peak horizontal scaling; bounded and paginated queries

## Constitution Check

- PASS: Git ownership is reserved for Ahmed ELbamby.
- PASS: Requirements, acceptance scenarios, and tasks use stable traceability identifiers.
- PASS: The design remains a simple modular monolith.
- PASS: Security, policy, schedule, capacity, and term decisions remain server-authoritative.
- PASS: Accessibility, scalability, concurrency, and observability requirements are retained.
- PASS: No application source code or migration is created by this planning phase.

## Dependency Check

- [SPEC-001](../001-product-charter-rbac/spec.md)
- [SPEC-002](../002-aastmt-policy-rulebook/spec.md)

## Project Structure

Future implementation follows the project-per-business-module modular monolith in
`docs/ARCHITECTURE.md`. Frontend source is owned by
`StudentRegistration.Client`; contracts come from `StudentRegistration.Contracts`
and the relevant business module; the composition root is
`StudentRegistration.Api`. No generic `Server`, `Domain`, `Application`, or
`Infrastructure` project is introduced. The complete solution also contains the
IdentityAccess, Academics, Scheduling, Registration, StaffAdministration, and
Infrastructure.SqlServer projects plus `tests/`.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Feature Design

- Produce one approved Page Design Record for each of the 27 route templates,
  with six widths, state applicability, focus order, data/reason contracts,
  component inventory, transitions, and unique test IDs.
- Own frontend view models, design/test metadata, tokens, reusable components,
  and presentation behavior. Runtime domain DTOs and server endpoints remain
  owned by their contributing feature specifications.
- Treat every downstream route as deferred integration: design may proceed
  from a reviewed contract, but contract tests, page implementation, and E2E
  evidence require the implementation-owner spec and all contributing API
  contracts to be approved and version-pinned.

## Execution Strategy

1. Validate SPEC-001/SPEC-002, route ownership, component ownership, and the
   cross-spec contributor matrix; complete consistency analysis.
2. Enforce Ahmed ELbamby's approved English-first neutral UI and the official
   AASTMT logo provenance contract in `docs/BRAND_ASSETS.md`; additional
   institutional brand values remain blocked rather than inferred.
3. Build design/test schemas, then failing component/contract/accessibility/E2E
   tests, then the smallest page/component implementation that passes them.
4. Run each route slice only after its owner/contributor approval gate; collect
   deterministic browser, visual, keyboard, screen-reader, and usability
   evidence without allowing retries to hide a first-run failure.

## Frontend Verification Toolchain and Evidence

- Razor component behavior uses bUnit with deterministic render fixtures and no live institutional dependency.
- Browser journeys use Microsoft.Playwright. Browser and operating-system builds MUST be pinned per release in tests/StudentRegistration.E2ETests/browser-matrix.json; floating latest labels are not release evidence.
- Automated accessibility uses axe-core from the Playwright accessibility harness plus keyboard and focus assertions. Automated results supplement rather than replace manual assistive-technology review.
- Visual regression uses Playwright screenshot comparisons with approved baselines stored by route, state, browser engine, and viewport under tests/StudentRegistration.VisualTests/Baselines/ and governed by tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json. Dynamic time, identifiers, animations, and nondeterministic content MUST use controlled fixtures or documented masks.
- Current stable Chrome, Edge, and Firefox plus a version-pinned Playwright
  WebKit target form the POC browser gate. CI image, browser, and engine builds
  are recorded in the versioned matrix; WebKit evidence MUST NOT be labelled
  Safari. Actual Safari/macOS verification is deferred to a separately
  approved future browser-support/release decision.
- Component and contract fixtures MUST pin server time, academic term, identity/role, policy version, capacity, rowversion, correlation IDs, and every applicable UI state. Fixtures MUST contain no production student data.
- A first-run failure remains a failed gate under tests/StudentRegistration.E2ETests/flake-policy.json. A retry MAY collect trace, video, screenshot, console, and network diagnostics but MUST NOT convert the gate to pass. Critical journeys cannot be quarantined; every flake requires an owner, issue, cause, and correction before release.
- POC release evidence includes bUnit results, client contract results,
  Playwright traces/reports, axe-core output, visual-baseline approval, and the
  Chrome/Edge/Firefox/WebKit browser-matrix provenance. It contains no claim
  that WebKit proves Safari support.

## Frontend Test Project Structure

- tests/StudentRegistration.Client.UnitTests: bUnit component and design-token checks.
- tests/StudentRegistration.Client.ContractTests: route-to-API and stable-reason fixtures.
- tests/StudentRegistration.E2ETests: Microsoft.Playwright primary, failure, authorization, stale, offline, and race journeys.
- tests/StudentRegistration.AccessibilityTests: axe-core automation plus keyboard/focus protocols.
- tests/StudentRegistration.VisualTests: deterministic screenshot assertions and approved baselines.
- docs/release-evidence/frontend: manual screen-reader, usability, browser
  provenance, WebKit-not-Safari labeling, and baseline-approval records;
  actual Safari/macOS evidence is a future separately approved artifact.

## Non-Functional Requirements

- NFR-1: Every critical route and reusable component MUST conform to WCAG 2.2
  AA with zero unresolved critical or serious automated accessibility finding.
- NFR-2: All actions MUST be keyboard operable in a logical order with a
  visible focus indicator and focus restoration after dialogs/navigation.
- NFR-3: Normal text contrast MUST be at least 4.5:1; large text and interactive
  component boundaries/focus indicators MUST be at least 3:1.
- NFR-4: Pointer targets MUST be at least 44 by 44 CSS pixels except the
  documented WCAG inline/spacing exceptions.
- NFR-5: Core pages MUST reflow at 400% zoom and from 320 through 1920 CSS
  pixels without two-dimensional scrolling except a documented data table that
  has a non-scrolling semantic alternative.
- NFR-6: Student discovery, schedule, and review routes MUST achieve Largest
  Contentful Paint at or below 2.5 seconds at p75 with production compression,
  cold browser cache, a four-core/4-GB client profile, 10-Mbps down/2-Mbps up,
  100-ms round-trip latency, and server APIs meeting their specified p95.
- NFR-7: The POC browser gate MUST pass in current stable Chrome, Edge, and
  Firefox plus a version-pinned Playwright WebKit target. Browser and engine
  builds MUST be recorded in the versioned evidence matrix, and WebKit results
  MUST be labelled WebKit rather than Safari. Actual Safari/macOS verification
  is deferred from the POC and requires a separately approved future browser
  support/release decision.
- NFR-8: Go-live registration usability completion MUST be at least 90% across
  at least eight representative students including novice, keyboard-only, and
  screen-reader participants, plus at least three Admin, three Lecturer, and
  three TA participants for their critical journeys, with no unresolved
  critical or major core-flow usability defect.
- NFR-9: Visual regression MUST reject every unapproved difference from the
  versioned baseline at 375, 768, 1280, and 1920 CSS pixels.
- NFR-10: The MVP MUST be English-first and localization-ready: user-facing
  strings MUST be externalized, formats MUST be culture-aware, layout MUST be
  direction-safe, and user text MUST NOT be embedded as component control
  logic.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
