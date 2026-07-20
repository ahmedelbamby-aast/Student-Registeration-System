# SPEC-003 NFR-6 Frontend Performance Evidence

**Result: WAIVED-DEMO — not a threshold pass.**
**Production/go-live approval: withheld.**

The executable measurement is
`Spec003LcpEvidenceTests.Discovery_schedule_and_review_meet_the_cold_cache_p75_lcp_budget`.
The original run completed on 2026-07-19 against the published Release client
output and used a fresh browser context for each of four samples per route.
Every sample reached the canonical `data-state="success"` element for its
route. The complete machine record is in
[`SPEC-003-NFR-6-results.json`](SPEC-003-NFR-6-results.json).

## Measured result

| Route | Four cold-cache LCP samples (ms) | p75 (ms) | Threshold (ms) |
|---|---:|---:|---:|
| STU-02 discovery | 5,328; 4,664; 5,808; 5,604 | 5,604 | 2,500 |
| STU-04 schedule | 5,144; 5,104; 4,744; 5,280 | 5,144 | 2,500 |
| STU-05 review | 4,900; 5,732; 4,880; 5,004 | 5,004 | 2,500 |

The measured result therefore exceeds the NFR-6 LCP budget on all three
routes. FCP remained approximately 288–328 ms. The largest-contentful-paint
candidate was the 33,000-pixel app-shell logo rendered after the Blazor
startup path; the test retains the browser's standard LCP semantics and does
not substitute FCP for LCP.

## Profile and asset fidelity

- Configuration: Release, published Blazor WebAssembly client; the test host
  serves the publish-generated static assets without starting SQL, identity, or
  background-worker services.
- Browser cache: cold for every sample (new browser context each time).
- Browser profile: simulated four logical cores and 4 GB (`navigator` values),
  CPU throttling rate 2, 10-Mbps down, 2-Mbps up, and 100-ms emulated round-trip
  latency. This is a reproducible browser profile, not a claim of physical
  hardware isolation.
- Compression: Brotli (`Content-Encoding: br`) was observed on WebAssembly
  responses. Each sample transferred 2,767,874 encoded framework bytes; total
  encoded page/resource bytes were 2,800,664 (STU-02), 2,799,812 (STU-04), and
  2,799,292 (STU-05).
- API timing: p95 **289.3 ms** across 24 deterministic Playwright fixture
  resources. This is fixture/network evidence only; it is not a live SQL/API
  server-latency claim.

## Explicit demo waiver

Ahmed ELbamby approved the `WAIVED-DEMO` result on 2026-07-19 for this
non-production design-capability demo because the measured latency is accepted
for demo use. The waiver does not convert the 2,500-ms requirement into a pass,
does not authorize a production release, and does not claim live server API
performance. `NFR-6EvidenceTests` fails closed unless the artifact is either a
true threshold `PASS` or this exact approved `WAIVED-DEMO` record.
