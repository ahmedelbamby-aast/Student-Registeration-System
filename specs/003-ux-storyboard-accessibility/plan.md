# Implementation Plan: Frontend Page Design, Storyboard, Accessibility and Functional Testing

**Branch**: 003-ux-storyboard-accessibility | **Date**: 2026-07-13 | **Spec**: [spec.md](spec.md)
**Status**: Planning complete; implementation is not authorized.

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

Future implementation paths are src/StudentRegistration.Client, src/StudentRegistration.Server, src/StudentRegistration.Domain, src/StudentRegistration.Infrastructure, and tests/. These paths are declarations only and do not exist yet.

## Design Artifacts

- [Research](research.md)
- [Data model](data-model.md)
- [API contract](contracts/api.md)
- [Planning quickstart](quickstart.md)
- [Tasks](tasks.md)

## Frontend Verification Toolchain and Evidence

- Razor component behavior uses bUnit with deterministic render fixtures and no live institutional dependency.
- Browser journeys use Microsoft.Playwright. Browser and operating-system builds MUST be pinned per release in tests/StudentRegistration.E2ETests/browser-matrix.json; floating latest labels are not release evidence.
- Automated accessibility uses axe-core from the Playwright accessibility harness plus keyboard and focus assertions. Automated results supplement rather than replace manual assistive-technology review.
- Visual regression uses Playwright screenshot comparisons with approved baselines stored by route, state, browser engine, and viewport under tests/StudentRegistration.VisualTests/Baselines/ and governed by tests/StudentRegistration.VisualTests/Baselines/baseline-manifest.json. Dynamic time, identifiers, animations, and nondeterministic content MUST use controlled fixtures or documented masks.
- Chromium, Firefox, and WebKit automation runs in pinned CI images. WebKit results MUST NOT be reported as Safari results. Current stable Safari requires a manual run on pinned macOS/Safari versions with evidence recorded in docs/release-evidence/frontend/safari-macos-evidence.md.
- Component and contract fixtures MUST pin server time, academic term, identity/role, policy version, capacity, rowversion, correlation IDs, and every applicable UI state. Fixtures MUST contain no production student data.
- A first-run failure remains a failed gate under tests/StudentRegistration.E2ETests/flake-policy.json. A retry MAY collect trace, video, screenshot, console, and network diagnostics but MUST NOT convert the gate to pass. Critical journeys cannot be quarantined; every flake requires an owner, issue, cause, and correction before release.
- Release evidence includes bUnit results, client contract results, Playwright traces/reports, axe-core output, visual-baseline approval, browser-matrix provenance, and the signed Safari/macOS manual record.

## Frontend Test Project Structure

- tests/StudentRegistration.Client.UnitTests: bUnit component and design-token checks.
- tests/StudentRegistration.Client.ContractTests: route-to-API and stable-reason fixtures.
- tests/StudentRegistration.E2ETests: Microsoft.Playwright primary, failure, authorization, stale, offline, and race journeys.
- tests/StudentRegistration.AccessibilityTests: axe-core automation plus keyboard/focus protocols.
- tests/StudentRegistration.VisualTests: deterministic screenshot assertions and approved baselines.
- docs/release-evidence/frontend: manual screen-reader, usability, Safari/macOS, browser provenance, and baseline-approval records.

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
- NFR-6: Student discovery, schedule, and review routes SHOULD achieve Largest
  Contentful Paint at or below 2.5 seconds at p75 with production compression,
  cold browser cache, a four-core/4-GB client profile, 10-Mbps down/2-Mbps up,
  100-ms round-trip latency, and server APIs meeting their specified p95.
- NFR-7: Functional journeys MUST pass in the current and previous stable
  versions of Chrome, Edge, and Firefox, plus current stable Safari on macOS.
  Playwright WebKit MAY provide earlier feedback but MUST NOT be reported as
  proof that actual Safari passed.
- NFR-8: Go-live registration usability completion MUST be at least 90% across
  at least eight representative students including novice, keyboard-only, and
  screen-reader participants, plus at least three Admin, three Lecturer, and
  three TA participants for their critical journeys, with no unresolved
  critical or major core-flow usability defect.
- NFR-9: Visual regression MUST reject every unapproved difference from the
  versioned baseline at 375, 768, 1280, and 1920 CSS pixels.
- NFR-10: Text, labels, formats, and layout MUST be localization-ready; user
  text MUST NOT be embedded as component control logic.

## Complexity Tracking

No constitution violation or distributed component is proposed. Additional infrastructure requires measured evidence and an approved amendment.
