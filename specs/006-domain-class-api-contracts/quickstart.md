# Planning Quickstart: Domain Classes and API Contracts

This is a pre-implementation verification guide. It does not run or create application code.

1. Confirm the feature is Approved for Gate A demo implementation as of
   2026-07-13.
2. Review requirements.md and keep production-only decisions fail closed at
   their applicable later gates.
3. Verify each functional requirement appears in spec.md and tasks.md.
4. Walk through each acceptance scenario with the accountable owner.
5. Review data-model.md and contracts/api.md against upstream dependencies.
6. Run the repository Spec Kit gate script and the SPEC-006 consistency and
   implementation-readiness checklists.
7. Execute approved tasks test-first in dependency order. Compiled skipped
   downstream fixtures may document future integration boundaries, but they do
   not satisfy runtime HTTP, OpenAPI, performance, accessibility, security, or
   release evidence.
8. Preserve Gates B-D and all production release gates until their real
   deployment inputs and accountable approvals exist.
