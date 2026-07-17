# SPEC-016 NFR-4 Evidence

STF-02 and STF-04 render the same server data through both a keyboard-operable
calendar and a chronological `ScheduleList` alternative. STF-03 uses a
focusable roster table with labelled controls. Availability editing uses
labelled text/time/select controls, explicit buttons, live status/error
regions, and server-current stale/deadline recovery states.

**Automated evidence:** `NFR-4EvidenceTests`, 10 client unit tests, 6 client
route contract tests, 12 static accessibility tests, and 13 SPEC-016 E2E
journeys passed. Manual assistive-technology evidence remains a later
SPEC-018/Gate-D obligation and is not claimed here.
