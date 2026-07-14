# Planning Quickstart: Academic Term and Student Profile

This is a pre-implementation verification guide. It does not run or create application code.

1. Confirm the feature is APPROVED for Gate A demo implementation as of
   2026-07-13.
2. Baseline upstream policy, UX, ERD, Identity, and quality contracts.
3. Verify AppContext ownership, half-open deterministic term/window resolution,
   explicit lifecycle/version semantics, Admin API journeys, and the upstream
   student-term serialization protocol.
4. Verify the synthetic profile contains every existing FR-5 field, stable
   University-ID/ApplicationUser links, synthetic provenance, and no production
   or invented demographic/contact data.
5. Verify all ten endpoint request/response/status/auth/error matrices, the
   20-window/20-operation and error/string/search bounds, term-creation replay,
   versioned mutation cancellation/refetch behavior, and role/resource scope.
6. Walk every AC/EC/SC, deterministic seed, supersession-chain, and race fixture
   against tasks.md, including the exact authenticated 25,000-account shared-SQL
   two-replica load.
7. Confirm exact test-first delivery paths for the shared AppContext/window
   source, executable permission claims/policies, nullable frontend shell,
   Testing bootstrapper, and class-diagram reconciliation.
8. Run readiness and cross-spec consistency analysis.
9. Verify Ahmed ELbamby's Gate A approval record before implementation starts.
10. Execute test-first implementation tasks in dependency order while
   preserving separate production data-source, Gate B-D, and release approvals.
