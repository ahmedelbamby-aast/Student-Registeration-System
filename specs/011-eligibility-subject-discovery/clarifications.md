# Clarification Record: Eligibility and Subject Discovery

**Reviewed**: 2026-07-13
**Automated result**: PASS - no hidden NEEDS CLARIFICATION marker.
**Human approval**: APPROVED by Ahmed ELbamby on 2026-07-13 for Gate A demo implementation

## Clarifications

### Session 2026-07-13

- Q: Which simple policy and curriculum should discovery use? → A: Use
  `DEMO-POC-2026.1` and the 19-course `docs/DEMO_CURRICULUM.md` snapshot;
  display an 18-credit default target/normal maximum and 12-credit probation
  maximum, enforce prerequisite/GPA/standing/window/capacity/conflict rules,
  and provide no advisor or exception workflow.

The specification was reviewed for scope, actors, data, business rules, errors,
concurrency, security, accessibility, dependencies, and measurable outcomes.
Unknown production-only product/institutional decisions remain registered in
docs/OPEN_DECISIONS.md or docs/POLICY_RESEARCH.md with an owner and fail-closed
rule. They are not invented requirements and do not extend this demo approval
to production policy use, official AASTMT go-live, or later release gates.
