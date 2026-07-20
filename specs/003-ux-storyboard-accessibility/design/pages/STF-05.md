# STF-05 Page Design Record

**Record version:** `2.0`  
**Approval status:** Approved by Ahmed ELbamby on 2026-07-20  
**Readiness:** `design-only`  
**Design owner:** SPEC-003  
**Implementation owner:** SPEC-016  
**Design task:** SPEC-003/T296

This record reuses ADM-10's approval composition with server-derived Lecturer
or Teaching Assistant assignment scope. It never introduces a combined role.

## Machine-readable record

```json
{
  "schemaVersion":"1.0",
  "routeId":"STF-05",
  "routeTemplate":"/staff/approvals",
  "pageName":"StaffApprovalInboxPage.razor",
  "designOwnerSpec":"SPEC-003",
  "implementationOwnerSpec":"SPEC-016",
  "ownerSpecs":["SPEC-003","SPEC-014","SPEC-016"],
  "actors":["Lecturer","Teaching Assistant"],
  "purpose":"Review only registration requests within the active single-role teaching assignment scope.",
  "informationHierarchy":["AuthenticatedPage Staff context","Approval heading and assignment scope","Bounded assigned request list","Selected request context","Total, Enrolled, Held, Available capacity","Reasoned decision confirmation","Immutable result and reference"],
  "responsiveWireframes":{"320":"Drawer navigation, assignment scope, request cards, detail, capacity, then decision controls.","375":"One-column list/detail flow with complete reason text.","768":"Compact drawer navigation with list above detail and confirmation.","1024":"Persistent 272px sidebar and bounded list/detail columns.","1280":"Persistent sidebar with compact Staff density and contextual actions.","1920":"Centered bounded workspace; assignment scope remains adjacent to the request list."},
  "components":["AuthenticatedPage","AppShell","RoleNavigation","PageHeader","ApprovalWorkspace","SurfaceCard","DataTable","Pagination","ApprovalStatusBadge","CapacityBreakdown","FormField","AccessibleValidationSummary","ConfirmationDialog","AppButton","RouteStatePanel","Alert"],
  "dataContracts":["GET /api/staff/registration-approvals","GET /api/staff/registration-approvals/{submissionId}/lines/{lineId}","POST /api/staff/registration-approvals/{submissionId}/lines/{lineId}/decision"],
  "actions":["Filter assigned requests","Open request detail","Approve with reason","Reject with reason","Refresh stale assignment or request"],
  "navigationTransitions":["Actions stay on STF-05","Group context -> STF-03","Timetable -> STF-02","Dashboard -> STF-01"],
  "states":[
    {"state":"loading","applicability":"required","fixture":"STF-05-loading-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Loading assigned approvals","No decision success"],"expectedFocusTarget":"Approval heading","liveRegion":"polite","nextActions":[],"testIds":["STF-05-COMP-STATE-LOADING"]},
    {"state":"empty","applicability":"required","fixture":"STF-05-empty-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["No assigned pending decisions","Active Lecturer or Teaching Assistant scope"],"expectedFocusTarget":"Empty-state heading","liveRegion":"none","nextActions":["Return to Staff home"],"testIds":["STF-05-COMP-STATE-EMPTY"]},
    {"state":"success","applicability":"required","fixture":"STF-05-success-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Single active Staff role","Assigned subject or overload request","Minimal Student and group context","Total, Enrolled, Held, Available","Submitted and expiry time","Reasoned confirmation"],"expectedFocusTarget":"Approval heading or originating decision control","liveRegion":"polite","nextActions":["Approve with reason","Reject with reason"],"testIds":["STF-05-COMP-STATE-SUCCESS"]},
    {"state":"validation-error","applicability":"required","fixture":"STF-05-validation-error-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Validation summary","Decision reason requirement","No false decision"],"expectedFocusTarget":"Validation summary","liveRegion":"assertive","nextActions":["Review decision reason"],"testIds":["STF-05-COMP-STATE-VALIDATION-ERROR"]},
    {"state":"service-error","applicability":"required","fixture":"STF-05-service-error-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Safe error message","Reference ID","Retry action"],"expectedFocusTarget":"Service-error heading","liveRegion":"polite","nextActions":["Retry"],"testIds":["STF-05-COMP-STATE-SERVICE-ERROR"]},
    {"state":"unauthorized","applicability":"required","fixture":"STF-05-unauthorized-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Access denied or assignment removed","No out-of-scope request data"],"expectedFocusTarget":"Access-denied heading","liveRegion":"assertive","nextActions":["Return to Staff home"],"testIds":["STF-05-COMP-STATE-UNAUTHORIZED"]},
    {"state":"session-expired","applicability":"required","fixture":"STF-05-session-expired-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Session expired","Staff sign-in action"],"expectedFocusTarget":"Session-expired heading","liveRegion":"assertive","nextActions":["Sign in again"],"testIds":["STF-05-COMP-STATE-SESSION-EXPIRED"]},
    {"state":"stale","applicability":"required","fixture":"STF-05-stale-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Assignment or decision changed","Current capacity","Refresh and review","No false success"],"expectedFocusTarget":"Stale-state heading","liveRegion":"polite","nextActions":["Refresh assignment and request"],"testIds":["STF-05-COMP-STATE-STALE"]},
    {"state":"offline","applicability":"required","fixture":"STF-05-offline-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Offline","No queued decision claim"],"expectedFocusTarget":"Offline heading","liveRegion":"polite","nextActions":["Retry when online"],"testIds":["STF-05-COMP-STATE-OFFLINE"]}
  ],
  "responsiveWidths":[320,375,768,1024,1280,1920],
  "focusOrder":["Skip link","Staff navigation","Approval heading","Assignment scope","Filters","Assigned request list","Selected request detail","Capacity breakdown","Decision reason","Approve and reject controls","Confirmation dialog and restored trigger","Result reference","Support action"],
  "testIds":["STF-05-CONTRACT-T297","STF-05-COMP-T297","STF-05-E2E-PRIMARY","STF-05-E2E-FAILURE","STF-05-A11Y-T297","STF-05-VIS-T297"],
  "contributorContractVersions":{"SPEC-003":"frontend-design-index/2.0","SPEC-014":"approved-2026-07-20","SPEC-016":"approved-2026-07-20"},
  "readinessState":"design-only",
  "approvalVersion":"2.0"
}
```

## Acceptance notes

- Lecturer and Teaching Assistant fixtures are separate single-role contexts with identical visual composition.
- Removed or stale assignment fails closed and reveals no out-of-scope request.
- Capacity is presented in the fixed textual order `Total / Enrolled / Held / Available`.

