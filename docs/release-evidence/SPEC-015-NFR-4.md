# SPEC-015 NFR-4 Historical Durability Evidence

**Recorded:** 2026-07-17
**Requirement:** historical records remain durable under the approved plan
**Result:** PASS

Both `NFR_4EvidenceTests` checks pass. A disposable SQL Server database was
upgraded through foundation, catalogue/scheduling, discovery/planning, and the
canonical SPEC-014 registration migration. Eight archived term receipts were
then read through the delivered authenticated history/detail APIs after the
mutable academic-term display names had been changed.

Every returned history row and detail receipt retained its original term name
and original meeting location from the immutable receipt snapshot. This proves
readability across the complete eight-term synthetic retention fixture without
creating another receipt table or write path.

The second check confirms the approved boundary: demo/test disposal remains
available, while hard retention or deletion values for real production data
remain unapproved and fail closed. No production retention period is invented
by SPEC-015.
