# SPEC-015 NFR-1 Receipt Retrieval Evidence

**Recorded:** 2026-07-17
**Requirement:** receipt retrieval p95 is at most 300 ms
**Result:** PASS

The focused `NFR_1EvidenceTests` run exercised the delivered authenticated
`GET /api/student/registrations/{submissionId}` endpoint through ASP.NET Core
TestServer and the production SQL adapter against a real SQL Server 2022
container upgraded through the canonical migrations.

| Measure | Result |
|---|---:|
| Synthetic student accounts | 25,000 |
| Registration submissions | 25,008 |
| Warm-up requests | 20 |
| Measured requests | 120 |
| Sorted nearest-rank p95 | 9.314 ms |
| Maximum permitted p95 | 300 ms |
| Response payload bound | 64 KiB |

All measured requests succeeded and returned the expected accepted receipt.
The fixture is deterministic, synthetic, non-production evidence and creates
no durable environment outside its disposable container.
