# ADM-10 Page Design Record

**Record version:** `2.0`  
**Approval status:** Approved by Ahmed ELbamby on 2026-07-20  
**Readiness:** `design-only`  
**Design owner:** SPEC-003  
**Implementation owner:** SPEC-017  
**Design task:** SPEC-003/T293

This record pins the global Admin approval inbox. The server owns scope,
decision validity, immutable audit, capacity conversion, and plan finalization.

## Machine-readable record

```json
{
  "schemaVersion":"1.0",
  "routeId":"ADM-10",
  "routeTemplate":"/admin/approvals",
  "pageName":"ApprovalAdministrationPage.razor",
  "designOwnerSpec":"SPEC-003",
  "implementationOwnerSpec":"SPEC-017",
  "ownerSpecs":["SPEC-003","SPEC-014","SPEC-017"],
  "actors":["Admin"],
  "purpose":"Review globally authorized subject and overload requests and record a reasoned decision without bypassing server policy or capacity rules.",
  "informationHierarchy":["AuthenticatedPage Admin context","Approval heading and bounded filters","Pending request list","Selected Student, subject, group, and request context","Total, Enrolled, Held, Available capacity","Reasoned decision confirmation","Immutable result and reference"],
  "responsiveWireframes":{"320":"Drawer navigation, filters, request cards, selected detail, capacity, then decision controls.","375":"One-column list/detail flow with decision reason before confirmation.","768":"Compact drawer navigation with list above detail and confirmation.","1024":"Persistent 272px sidebar with bounded list and detail columns.","1280":"Persistent sidebar, compact list/detail workspace, sticky contextual actions without obscuring content.","1920":"Centered bounded workspace; surplus width does not separate decision controls from context."},
  "components":["AuthenticatedPage","AppShell","RoleNavigation","PageHeader","ApprovalWorkspace","SurfaceCard","DataTable","Pagination","ApprovalStatusBadge","CapacityBreakdown","FormField","AccessibleValidationSummary","ConfirmationDialog","AppButton","RouteStatePanel","Alert"],
  "dataContracts":["GET /api/admin/registration-approvals","GET /api/admin/registration-approvals/{submissionId}/lines/{lineId}","POST /api/admin/registration-approvals/{submissionId}/lines/{lineId}/decision"],
  "actions":["Filter pending requests","Open request detail","Approve with reason","Reject with reason","Refresh stale request"],
  "navigationTransitions":["Actions stay on ADM-10","Student context -> ADM-04","Registration context -> ADM-08","Dashboard -> ADM-01"],
  "states":[
    {"state":"loading","applicability":"required","fixture":"ADM-10-loading-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Loading authorized approvals","No decision success"],"expectedFocusTarget":"Approval heading","liveRegion":"polite","nextActions":[],"testIds":["ADM-10-COMP-STATE-LOADING"]},
    {"state":"empty","applicability":"required","fixture":"ADM-10-empty-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["No pending decisions","Active filter scope"],"expectedFocusTarget":"Empty-state heading","liveRegion":"none","nextActions":["Clear filters"],"testIds":["ADM-10-COMP-STATE-EMPTY"]},
    {"state":"success","applicability":"required","fixture":"ADM-10-success-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Subject or overload request type","Student, subject, and group context","Total, Enrolled, Held, Available","Submitted and expiry time","Reasoned confirmation","Immutable result reference"],"expectedFocusTarget":"Approval heading or originating decision control","liveRegion":"polite","nextActions":["Approve with reason","Reject with reason"],"testIds":["ADM-10-COMP-STATE-SUCCESS"]},
    {"state":"validation-error","applicability":"required","fixture":"ADM-10-validation-error-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Validation summary","Decision reason requirement","No false decision"],"expectedFocusTarget":"Validation summary","liveRegion":"assertive","nextActions":["Review decision reason"],"testIds":["ADM-10-COMP-STATE-VALIDATION-ERROR"]},
    {"state":"service-error","applicability":"required","fixture":"ADM-10-service-error-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Safe error message","Reference ID","Retry action"],"expectedFocusTarget":"Service-error heading","liveRegion":"polite","nextActions":["Retry"],"testIds":["ADM-10-COMP-STATE-SERVICE-ERROR"]},
    {"state":"unauthorized","applicability":"required","fixture":"ADM-10-unauthorized-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Access denied","No approval data"],"expectedFocusTarget":"Access-denied heading","liveRegion":"assertive","nextActions":["Return to Admin home"],"testIds":["ADM-10-COMP-STATE-UNAUTHORIZED"]},
    {"state":"session-expired","applicability":"required","fixture":"ADM-10-session-expired-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Session expired","Staff sign-in action"],"expectedFocusTarget":"Session-expired heading","liveRegion":"assertive","nextActions":["Sign in again"],"testIds":["ADM-10-COMP-STATE-SESSION-EXPIRED"]},
    {"state":"stale","applicability":"required","fixture":"ADM-10-stale-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Decision or assignment changed","Current capacity","Refresh and review","No false success"],"expectedFocusTarget":"Stale-state heading","liveRegion":"polite","nextActions":["Refresh request"],"testIds":["ADM-10-COMP-STATE-STALE"]},
    {"state":"offline","applicability":"required","fixture":"ADM-10-offline-v2","fixtureVersion":"frontend-fixture/2.0","expectedContent":["Offline","No queued decision claim"],"expectedFocusTarget":"Offline heading","liveRegion":"polite","nextActions":["Retry when online"],"testIds":["ADM-10-COMP-STATE-OFFLINE"]}
  ],
  "responsiveWidths":[320,375,768,1024,1280,1920],
  "focusOrder":["Skip link","Admin navigation","Approval heading","Filters","Request list","Selected request detail","Capacity breakdown","Decision reason","Approve and reject controls","Confirmation dialog and restored trigger","Result reference","Support action"],
  "testIds":["ADM-10-CONTRACT-T294","ADM-10-COMP-T294","ADM-10-E2E-PRIMARY","ADM-10-E2E-FAILURE","ADM-10-A11Y-T294","ADM-10-VIS-T294"],
  "contributorContractVersions":{"SPEC-003":"frontend-design-index/2.0","SPEC-014":"approved-2026-07-20","SPEC-017":"approved-2026-07-20"},
  "readinessState":"design-only",
  "approvalVersion":"2.0"
}
```

## Acceptance notes

- The list is bounded and paginated; decision details never expose unrelated records.
- Approval, rejection, double action, expiry, release, and stale decisions render the server result exactly once.
- Capacity is presented in the fixed textual order `Total / Enrolled / Held / Available`.

