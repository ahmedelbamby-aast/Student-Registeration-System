# SPEC-015 NFR-3 Accessible Print and PII Evidence

**Recorded:** 2026-07-17
**Requirement:** printed/exported views are accessible and minimize PII
**Result:** PASS

Both `NFR_3EvidenceTests` checks pass. The delivered browser-print view uses
one semantic meeting model for the calendar and its equivalent chronological
list, connects the alternatives through accessible descriptions, provides
table captions and scoped column headers, and hides the print control in print
media.

The bounded history DTO contains only submission identifier, reference, term
snapshot, status, submission time, group count, credit total, and term state.
The serialized evidence contains no student identity, contact, academic-state,
credential, cookie, or security-stamp field. No CSV/export feature was added;
the implemented output is the accessible browser-print view.

Companion route evidence also passes: accessibility `5/5`, visual contract
`2/2`, route contract `2/2`, component `2/2`, and browser journeys `9/9`.
