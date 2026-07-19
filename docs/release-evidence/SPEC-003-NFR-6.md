# SPEC-003 NFR-6 Frontend Performance Evidence

**Result: PENDING EXECUTION.**

The executable measurement is
`Spec003LcpEvidenceTests.Discovery_schedule_and_review_meet_the_cold_cache_p75_lcp_budget`.
It starts the Blazor client in Release configuration, creates a fresh browser
context for every cold-cache sample, fixes `navigator.hardwareConcurrency` and
`navigator.deviceMemory` to the approved four-core/4-GB client profile, and
uses Chromium DevTools network emulation for 10-Mbps down, 2-Mbps up, and
100-ms round-trip latency. It samples STU-02, STU-04, and STU-05 four times
each and rejects p75 above 2,500 ms.

`SPEC-003-NFR-6-results.json` is generated only after the stable Release run;
the quality gate fails closed while that artifact is missing or non-passing.
