# SPEC-003 NFR-9 Visual Regression Evidence

**Result: PASS (verified 19 July 2026).**

The executable gate requires all 27 routes in the versioned baseline manifest.
Each route must have 16 approved artifacts: Chrome, Edge, Firefox, and
Playwright WebKit at 375, 768, 1280, and 1920 CSS pixels. Every PNG is bound to
its manifest by SHA-256; a missing file, hash drift, unapproved route, or
viewport omission fails the gate.

The Release visual assembly currently contains 467 tests. It was executed as
four read-only route-family shards to use the available CPU, followed by the
two shared registration-record contract tests: 221/221 student and identity,
111/111 admin, 85/85 staff and system, 48/48 shared ADM-01/08/09 baselines,
and 2/2 shared registration-record contracts. The combined result is
467/467 passed with zero failures and zero skips.

The governed artifact inventory contains 27 approved route manifests and 432
PNG targets. A post-capture integrity audit found zero missing artifacts and
zero SHA-256 mismatches. `NFR-9EvidenceTests` passed 1/1 and `EC-7Tests`
passed 1/1 against the same versioned manifests. The visual runners had no
baseline-approval environment variables, so any unapproved pixel difference
would fail rather than replace an artifact.
