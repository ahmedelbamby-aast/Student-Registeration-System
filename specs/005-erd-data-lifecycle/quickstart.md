# Planning Quickstart: ERD and Data Lifecycle

This is a pre-implementation verification guide. It does not run or create application code.

1. Confirm the feature is Approved for Gate A demo implementation as of
   2026-07-13.
2. Review requirements.md and keep production retention and migration-window
   decisions fail closed at their applicable later gates.
3. Verify each functional requirement appears in spec.md and tasks.md.
4. Walk through each acceptance scenario with the accountable owner.
5. Review data-model.md and contracts/api.md against upstream dependencies.
6. Verify the non-production database contract: isolated Development and
   per-run Testing names, SQL Server 2022 Developer compatibility 160,
   Docker/Testcontainers provisioning, migrations before seed, Testing
   disposal, Development persistence until guarded reset, idempotent versioned
   synthetic-only seed, hash-only credentials, Git-ignore controls, and
   seven-day local-artifact cleanup.
7. Run the repository Spec Kit gate script.
8. Verify the Gate A approval record, then execute approved implementation
   tasks in dependency order; preserve production migration and release gates.
