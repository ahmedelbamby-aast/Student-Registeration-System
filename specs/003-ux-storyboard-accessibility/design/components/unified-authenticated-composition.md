# Unified Authenticated Composition

**Contract:** `authenticated-composition/2.0`  
**Approved:** Ahmed ELbamby, 2026-07-20  
**Applies to:** all 24 authenticated route templates

Every Student, Admin, Lecturer, and Teaching Assistant page uses the same
visual and interaction language. Role-specific data and authorized actions are
inputs; they are not alternate themes.

## Canonical composition

`AuthenticatedPage` composes `AppShell`, `WorkspaceNavigationCatalog`,
`RoleNavigation`, `PageHeader`, route state, actions, and body content. It is
presentation-only and cannot call an API or decide authorization.

| Workspace | Navigation destinations | Density |
|---|---|---|
| Student | Dashboard, Roadmap, Subjects, Current plan, Registrations, Account | Comfortable |
| Admin | Dashboard, Terms, Users, Students, Catalogue, Offerings, Approvals, Resources, Registrations, Audit | Compact |
| Lecturer / Teaching Assistant | Staff home, Timetable, Availability, Approvals | Compact |

Roster is a contextual group destination, not global navigation. Lecturer and
Teaching Assistant are separate single-role contexts and share the same Staff
composition.

## Shared visual patterns

- `SurfaceCard` owns page sections; `EntityCard` owns repeated records.
- `AppButton`, `AppLink`, `FormField`, and
  `AccessibleValidationSummary` own interactive form language.
- `RouteStatePanel` owns loading, empty, denied, expired, stale, offline, and
  service-error states; `Alert` is inline feedback only.
- `DataTable` always has a compact semantic alternative.
- `ScheduleCalendar` always has an information-equivalent `ScheduleList`.
- `CapacityBreakdown` always presents Total, Enrolled, Held, Available.
- `ApprovalWorkspace` is shared by ADM-10 and STF-05; only server-derived scope
  and permitted actions differ.
- All controls remain at least 44px, focus is visible and restored, and status
  meaning is never color-only.

Page-isolated CSS may arrange domain content using approved spacing tokens. It
may not redefine shell, navigation, typography, forms, buttons, cards, tables,
dialogs, statuses, shadows, breakpoints, or role-specific themes.

