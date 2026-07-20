# Reusable Component Catalogue

**Version:** `component-catalogue/2.0`<br>
**Manifest:** `.specify/component-manifest.json` version `2.0.0`

This catalogue is indexed by the component manifest. Each entry has one
canonical source writer. Components accept presentation-ready parameters and callbacks;
they do not decide roles, eligibility, capacity, conflicts, or command success.

| Component | Canonical source | Writer task | Native/ARIA semantic and keyboard semantics |
|---|---|---|---|
| AppShell | `src/StudentRegistration.Client/Components/Layout/AppShell.razor` | T077 | landmarks, skip link, authoritative context; normal document/tab order |
| AuthenticatedPage | `src/StudentRegistration.Client/Components/Layout/AuthenticatedPage.razor` | T281 | presentation-only shared shell/page composition; approved-planned |
| PageHeader | `src/StudentRegistration.Client/Components/Layout/PageHeader.razor` | T281 | one page heading, description, metadata/status, and action hierarchy |
| RoleNavigation | `src/StudentRegistration.Client/Components/Navigation/RoleNavigation.razor` | T079 | labelled `nav` and links; Tab/Shift+Tab, Enter activation |
| AppButton | `src/StudentRegistration.Client/Components/Forms/AppButton.razor` | T081 | native `button`; Enter/Space, real disabled/pending behavior |
| AppLink | `src/StudentRegistration.Client/Components/Navigation/AppLink.razor` | T083 | native anchor with meaningful accessible name; Enter activation |
| FormField | `src/StudentRegistration.Client/Components/Forms/FormField.razor` | T085 | explicit label, description, error association; native field keyboard behavior |
| AccessibleValidationSummary | `src/StudentRegistration.Client/Components/Forms/AccessibleValidationSummary.razor` | T087 | focusable error summary with links to invalid controls |
| SearchFilter | `src/StudentRegistration.Client/Components/Forms/SearchFilter.razor` | T089 | search landmark/labelled controls; Enter submit and native clear control |
| EntityCard | `src/StudentRegistration.Client/Components/Display/EntityCard.razor` | T091 | article/section with heading; interactive descendants remain native |
| SurfaceCard | `src/StudentRegistration.Client/Components/Display/SurfaceCard.razor` | T281 | noninteractive section surface with semantic heading hierarchy |
| StatusBadge | `src/StudentRegistration.Client/Components/Feedback/StatusBadge.razor` | T093 | text and icon status, never color alone |
| RouteStatePanel | `src/StudentRegistration.Client/Components/Feedback/RouteStatePanel.razor` | T095 | heading, message, live-region policy, and native next actions |
| ApprovalStatusBadge | `src/StudentRegistration.Client/Components/Feedback/ApprovalStatusBadge.razor` | T292 | textual subject/overload state; never color-only |
| Alert | `src/StudentRegistration.Client/Components/Feedback/Alert.razor` | T097 | status or alert role according to urgency; dismiss button is named |
| ConfirmationDialog | `src/StudentRegistration.Client/Components/Overlay/ConfirmationDialog.razor` | T099 | named modal dialog; Escape cancel, trapped focus, trigger focus restoration |
| DataTable | `src/StudentRegistration.Client/Components/Data/DataTable.razor` | T101 | native caption/table/header cells; sortable buttons expose direction |
| Pagination | `src/StudentRegistration.Client/Components/Data/Pagination.razor` | T103 | labelled navigation and native buttons/links; current page announced |
| GroupCard | `src/StudentRegistration.Client/Components/Registration/GroupCard.razor` | T105 | named group option with explicit selected/disabled reason |
| CapacityBreakdown | `src/StudentRegistration.Client/Components/Registration/CapacityBreakdown.razor` | T287 | fixed Total, Enrolled, Held, Available order and freshness state |
| CapacityIndicator (deprecated) | `src/StudentRegistration.Client/Components/Registration/CapacityIndicator.razor` | T281 | compatibility only until every route uses CapacityBreakdown |
| ScheduleCalendar | `src/StudentRegistration.Client/Components/Scheduling/ScheduleCalendar.razor` | T109 | labelled schedule grid with keyboard-reachable entries, never sole representation |
| ScheduleList | `src/StudentRegistration.Client/Components/Scheduling/ScheduleList.razor` | T111 | chronological headings/list semantically equivalent to calendar |
| ConflictPanel | `src/StudentRegistration.Client/Components/Scheduling/ConflictPanel.razor` | T113 | blocking heading, Red X plus text, overlaps, alternatives, resolution links |
| ReceiptSummary | `src/StudentRegistration.Client/Components/Registration/ReceiptSummary.razor` | T115 | receipt heading, definition/list structure, reference and printable content |
| IdentityFeedback | `src/StudentRegistration.Client/Components/Identity/IdentityFeedback.razor` | T281 | shared identity feedback with appropriate live-region urgency |
| IdentityPageShell | `src/StudentRegistration.Client/Components/Identity/IdentityPageShell.razor` | T281 | public identity composition without authenticated navigation |
| ApprovalWorkspace | `src/StudentRegistration.Client/Components/Registration/ApprovalWorkspace.razor` | T295/T298 | shared bounded list/detail/confirmation composition; approved-planned |

## Common variants and states

Every interactive component documents and tests its relevant default, hover,
active, focus-visible, disabled, loading, and error state. A state that cannot
occur is omitted only with an explicit test reason. Pending command controls
remain disabled until their single authoritative result is presented.

- Accessible name and description come from visible or explicitly supplied
  text; an icon is never the only name.
- Native HTML behavior is preferred. Custom composite interaction must document
  its full keyboard semantics, focus entry, focus movement, Escape behavior,
  and focus restoration.
- Controls and primary clickable regions meet a 44 CSS px target except for a
  documented WCAG inline/spacing exception.
- Focus remains visible and is not moved for background refresh. Validation
  submission moves focus once to the summary; dialogs restore it to the trigger.
- Status is expressed with text and/or icon in addition to color. Loading,
  empty, error, offline, stale, and disabled states retain the same heading and
  landmark structure where possible.

## Styling boundary

All components use token-only styling through approved `--srs-*` semantic or
component aliases. Component source and isolated styles may not introduce raw
color, shadow, spacing, radius, motion, breakpoint, or z-index values. Browser
forced-colors and reduced-motion behavior remains available rather than being
overridden for visual similarity.
