# Clarification Record: AASTMT Policy Rulebook

**Reviewed**: 2026-07-13
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-13 for Gate A demo implementation

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown product/institutional decisions are registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
planning rule. They are not invented requirements; unresolved out-of-profile
or production-only policy decisions remain fail closed at their applicable
later gate and do not revoke this demo implementation approval.

## Session 2026-07-13

- Ahmed approved a simple, demo-only policy profile: configured
  window/standing/hold/prerequisite gates; regular load 9-18 with an
  18-credit default/recommended target and hard normal maximum; GPA below 2.0
  maximum 12; first-successful-commit capacity without waitlist; unresolved
  overlaps blocked; and travel-time buffering disabled.
- Automatic exceptions, add/drop, withdrawal, advisor workflow, waitlists,
  and overrides remain outside the POC.
- The demo uses the 19-course official-source AASTMT College of Artificial
  Intelligence Data Science snapshot in `docs/DEMO_CURRICULUM.md`. Any gap-filling course is explicitly
  synthetic, retains that label in data/UI, and is not attributed to AASTMT.
- This answer resolves the demo behavior only. Ahmed approved SPEC-002 for
  Gate A demo implementation on 2026-07-13; it is not official AASTMT
  production policy approval.
