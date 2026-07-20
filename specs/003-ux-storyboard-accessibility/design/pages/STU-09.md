# STU-09 Page Design Record

**Record version:** `2.0`  
**Approval status:** Approved by Ahmed ELbamby on 2026-07-20  
**Readiness:** `design-only`  
**Design owner:** SPEC-003  
**Implementation owner:** SPEC-008  
**Design task:** SPEC-003/T288

This record pins the Student roadmap presentation. Academic progression,
eligibility, enrollment, approval, and capacity remain server-authoritative.

## Machine-readable record

```json
{
  "schemaVersion": "1.0",
  "routeId": "STU-09",
  "routeTemplate": "/student/roadmap",
  "pageName": "StudentRoadmapPage.razor",
  "designOwnerSpec": "SPEC-003",
  "implementationOwnerSpec": "SPEC-008",
  "ownerSpecs": ["SPEC-003", "SPEC-008", "SPEC-009", "SPEC-011", "SPEC-014"],
  "actors": ["Student"],
  "purpose": "Explain the programme roadmap, prerequisite chains, and automatic versus self-registration ownership without making academic decisions in the client.",
  "informationHierarchy": [
    "AuthenticatedPage context and Student navigation",
    "Roadmap heading and registration-ownership explanation",
    "Level and recommended-term groups",
    "Three-credit subject cards and prerequisite chains",
    "Enrollment, approval, and capacity status",
    "Eligible subject action or blocking explanation"
  ],
  "responsiveWireframes": {
    "320": "Drawer navigation, heading, ownership notice, then one-column level/term and subject sequence.",
    "375": "One-column roadmap with status and prerequisite text adjacent to each subject.",
    "768": "Compact drawer navigation and two-column term groups while preserving document order.",
    "1024": "Persistent 272px sidebar and bounded two-column roadmap workspace.",
    "1280": "Persistent sidebar with comfortable three-column term grouping where content permits.",
    "1920": "Centered bounded workspace; surplus width does not detach prerequisites or actions from a subject."
  },
  "components": ["AuthenticatedPage", "AppShell", "RoleNavigation", "PageHeader", "SurfaceCard", "EntityCard", "ApprovalStatusBadge", "CapacityBreakdown", "AppLink", "RouteStatePanel", "Alert"],
  "dataContracts": ["GET /api/students/me/roadmap"],
  "actions": ["Open eligible subject", "Open current plan", "Refresh authoritative roadmap"],
  "navigationTransitions": ["Eligible subject -> STU-03", "Current plan -> STU-04", "Dashboard -> STU-01"],
  "states": [
    {"state":"loading","applicability":"required","fixture":"STU-09-loading-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Loading roadmap","No false academic status"],"expectedFocusTarget":"Roadmap heading","liveRegion":"polite","nextActions":[],"testIds":["STU-09-COMP-STATE-LOADING"]},
    {"state":"empty","applicability":"required","fixture":"STU-09-empty-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["No roadmap available","Safe support action"],"expectedFocusTarget":"Empty-state heading","liveRegion":"none","nextActions":["Open safe support path"],"testIds":["STU-09-COMP-STATE-EMPTY"]},
    {"state":"success","applicability":"required","fixture":"STU-09-success-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Level and recommended term","Exactly three credits per subject","Prerequisite chains","Automatic or self-registration ownership","Completed, eligible, blocked, pending, approved, rejected, released, and registered states"],"expectedFocusTarget":"Roadmap heading","liveRegion":"polite","nextActions":["Open eligible subject","Open current plan"],"testIds":["STU-09-COMP-STATE-SUCCESS"]},
    {"state":"validation-error","applicability":"not-applicable","reason":"The route has no mutation form.","fixture":"STU-09-validation-na-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":[],"expectedFocusTarget":"Roadmap heading","liveRegion":"none","nextActions":[],"testIds":["STU-09-COMP-STATE-VALIDATION-NA"]},
    {"state":"service-error","applicability":"required","fixture":"STU-09-service-error-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Safe error message","Reference ID","Retry action"],"expectedFocusTarget":"Service-error heading","liveRegion":"polite","nextActions":["Retry"],"testIds":["STU-09-COMP-STATE-SERVICE-ERROR"]},
    {"state":"unauthorized","applicability":"required","fixture":"STU-09-unauthorized-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Access denied","No roadmap data"],"expectedFocusTarget":"Access-denied heading","liveRegion":"assertive","nextActions":["Return to authorized home"],"testIds":["STU-09-COMP-STATE-UNAUTHORIZED"]},
    {"state":"session-expired","applicability":"required","fixture":"STU-09-session-expired-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Session expired","Student sign-in action"],"expectedFocusTarget":"Session-expired heading","liveRegion":"assertive","nextActions":["Sign in again"],"testIds":["STU-09-COMP-STATE-SESSION-EXPIRED"]},
    {"state":"stale","applicability":"required","fixture":"STU-09-stale-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Roadmap state changed","Refresh before acting"],"expectedFocusTarget":"Stale-state heading","liveRegion":"polite","nextActions":["Refresh authoritative roadmap"],"testIds":["STU-09-COMP-STATE-STALE"]},
    {"state":"offline","applicability":"required","fixture":"STU-09-offline-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Offline","No cached success claim"],"expectedFocusTarget":"Offline heading","liveRegion":"polite","nextActions":["Retry when online"],"testIds":["STU-09-COMP-STATE-OFFLINE"]}
  ],
  "responsiveWidths": [320, 375, 768, 1024, 1280, 1920],
  "focusOrder": ["Skip link", "Student navigation", "Roadmap heading", "Registration-ownership notice", "Level and term groups", "Subject statuses and prerequisite chains", "Eligible subject actions", "Support action"],
  "testIds": ["STU-09-CONTRACT-T289", "STU-09-COMP-T289", "STU-09-E2E-PRIMARY", "STU-09-E2E-FAILURE", "STU-09-A11Y-T289", "STU-09-VIS-T289"],
  "contributorContractVersions": {"SPEC-003":"frontend-design-index/2.0","SPEC-008":"approved-2026-07-20","SPEC-009":"approved-2026-07-20","SPEC-011":"approved-2026-07-20","SPEC-014":"approved-2026-07-20"},
  "readinessState": "design-only",
  "approvalVersion": "2.0"
}
```

## Acceptance notes

- Recommended-term-1 roots show no prerequisite and clearly identify automatic enrollment.
- Term 2 and later remain self-registration journeys; prerequisite chains and approval state never imply eligibility unless the server says so.
- Capacity, where supplied, is always `Total / Enrolled / Held / Available`.
- The six layouts preserve a single information and keyboard-focus order.

