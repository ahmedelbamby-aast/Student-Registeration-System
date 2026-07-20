# Implementation Quickstart: Registration Capacity and Concurrency

Gate A and the 2026-07-20 roadmap/line-approval amendment are recorded. This
guide verifies the approved baseline before amended demo implementation work.

1. Confirm the feature is Approved and both the 2026-07-13 Gate A record and
   2026-07-20 owner amendment remain current.
2. Review requirements.md and identify any later Gate B-D or production decisions that still fail closed.
3. Verify each functional requirement appears in spec.md and tasks.md.
4. Walk through each acceptance scenario with the accountable owner.
5. Review data-model.md and contracts/api.md against upstream dependencies.
6. Run the repository Spec Kit gate script.
7. Begin non-production demo implementation only while dependency baselines and
   automated gates pass. Gate B-D and production/release approvals remain separate.
8. Verify a first-program-term fixture resolves required roadmap roots,
   auto-enrolls them idempotently without approval, and produces no partial
   schedule on shortage.
9. Verify term-two-and-later submission creates one pending line and active
   hold per subject, uses `Enrolled + Held <= Capacity`, and exposes no holder
   PII in capacity projections.
10. Verify Admin can decide every line, Lecturer/TA only a currently assigned
    group line, unrelated or ended assignments disclose nothing, and every
    decision is antiforgery-protected, versioned, idempotent, and audited.
11. Verify final approval converts all holds atomically; any rejection or
    registration-window close releases all; approval/expiry and final-seat
    races preserve the occupied-seat invariant across two replicas.
12. Verify probation remains stricter, <=18 is normal, 19-21 requires CGPA
    >=3.00, >21 fails, and the unified roadmap/capacity/approval components
    render every required state for all four roles.
