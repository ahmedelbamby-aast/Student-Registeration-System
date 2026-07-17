# Admin Command Delegation Conformance

SPEC-017 is an orchestration and read-model boundary, not a second command
owner. ADM-02 through ADM-07 call the existing SPEC-007/008/009/010 clients and
endpoints. ADM-01, ADM-08, and ADM-09 add only SPEC-017-owned bounded reads and
scoped export lifecycle operations.

The following facades are prohibited:

- `AdminCommandService`;
- `AdminConfirmationService`;
- a second `RoleAssignment` or `AdminSecurityGuard` writer; and
- any break-glass, capacity/conflict bypass, enrollment repair, or staff
  availability mutation surface.

SPEC-007 remains the sole owner of Admin role changes. SPEC-017 preserves its
`STALE_VERSION` and HTTP 409 `FINAL_ADMIN_REQUIRED` results unchanged.

Evidence: all five `AdminCommandInvariantTests` passed on 2026-07-17. The
route-owner matrix is recorded in `SPEC-017-FR-1-delegation.md`; Phase 5 route
rows remain explicitly pending and are mandatory inputs to T100.
