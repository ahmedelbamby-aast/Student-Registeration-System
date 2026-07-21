# Live composed single-role journeys

These opt-in Playwright journeys use the real hosted UI and its configured database-backed APIs. They do not intercept browser requests. Each Admin, Lecturer, Teaching Assistant, and Student journey receives a fresh browser context.

Set `SRS_LIVE_DEMO_BASE_URL` plus the staff password variables `SRS_LIVE_ADMIN_PASSWORD`, `SRS_LIVE_LECTURER_PASSWORD`, and `SRS_LIVE_TA_PASSWORD`. Staff IDs can be overridden with `SRS_LIVE_ADMIN_ID`, `SRS_LIVE_LECTURER_ID`, and `SRS_LIVE_TA_ID`; otherwise the documented synthetic IDs are used. Never store passwords in source control.

An activated student can be included with `SRS_LIVE_STUDENT_ID` and `SRS_LIVE_STUDENT_PASSWORD`. Run:

```powershell
./tests/StudentRegistration.E2ETests/Run-LiveSingleRoleJourneys.ps1
```

For a release gate, require all four roles and fail closed if the student credential is absent:

```powershell
./tests/StudentRegistration.E2ETests/Run-LiveSingleRoleJourneys.ps1 -RequireStudent
```

The runner validates the TRX test count and rejects failures or skipped tests. Without activated student credentials, the normal runner selects the three staff journeys only; the release form always requires the student journey.
