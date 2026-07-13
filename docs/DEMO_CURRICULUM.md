# Demo College of AI Curriculum Snapshot

Approved by Ahmed ELbamby for the non-production POC on 13 July 2026.

## Provenance and limits

The course codes, titles, sequence, and prerequisite relationships below are a
small curated subset of the official AASTMT College of Artificial Intelligence
[Data Science curriculum](https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=283&unit_id=655),
accessed 13 July 2026. The related
[Intelligent Systems curriculum](https://aast.edu/en/programs-courses/program.php?language_id=1&program_id=277&unit_id=655)
is retained as a second import/provenance fixture.

The public program list does not show course credit values. For this demo,
every listed course has exactly **3 synthetic demo credits**. That credit value
is local POC configuration and MUST NOT be represented as an official AASTMT
value. The snapshot proves catalogue import, prerequisite, GPA, earned-credit,
eligibility, scheduling, and registration behavior; it is not a complete or
official curriculum publication.

## Published demo rows

| Sequence | Code | Title | Demo credits | Prerequisite or condition |
|---:|---|---|---:|---|
| 1 | BA101 | Calculus I | 3 | None |
| 1 | BA113 | Physics | 3 | None |
| 1 | GN111 | Introduction to Computing | 3 | None |
| 1 | GN112 | Introduction to Problem Solving and Programming | 3 | None |
| 2 | BA102 | Calculus II | 3 | BA101 |
| 2 | GN121 | Data Structures | 3 | GN112 |
| 2 | GN123 | Digital Logic Design | 3 | GN111 |
| 3 | BA203 | Probability and Statistics | 3 | BA102 |
| 3 | GN211 | Computing Algorithms | 3 | GN121 |
| 3 | IN211 | Fundamentals of Artificial Intelligence | 3 | GN111 and GN112 |
| 4 | DS221 | Fundamentals of Data Science | 3 | GN111 and GN112 |
| 4 | GN223 | Software Engineering | 3 | GN121 |
| 4 | IN221 | Machine Learning | 3 | IN211 and BA203 |
| 5 | IN311 | Deep Learning | 3 | IN221 |
| 5 | DS312 | Programming for Data Science | 3 | GN121 |
| 6 | DS322 | Statistics for Data Science | 3 | BA203 |
| 6 | DS324 | Computational Linguistics | 3 | IN311 |
| 7 | DS413 | Project I | 3 | GPA >= 2.0 and earned credits >= 96 |
| 8 | DS421 | Project II | 3 | DS413 |

All codes are normalized and unique. The published graph is acyclic and every
course-code prerequisite resolves inside the snapshot. Source rows that refer
to absent or inconsistent codes are excluded from this published POC snapshot
and remain invalid-import test fixtures.

## Offering template

Each seeded open offering follows the SPEC-010 rule:

- one Lecture meeting taught by at least one Lecturer;
- at least one Tutorial (which the UI may label Section) or one Laboratory;
- at least one TA for every present Tutorial and Laboratory activity;
- staff name, activity, room/location, day, start/end time, group capacity, and
  seats remaining available to student discovery; and
- multiple independently capacitated group bundles so schedule recommendation
  and final-seat concurrency can be demonstrated.

Seeded student transcript/GPA combinations deliberately cover eligible,
missing-prerequisite, GPA-blocked, earned-credit-blocked, probation, full-group,
and timetable-conflict outcomes. They remain wholly synthetic under the
non-production data contract.

## Synthetic-gap protocol

If implementation needs an additional row to demonstrate a case that the
curated graph cannot express, its code MUST start with `DEMO-`, its provenance
kind MUST be `SyntheticDemo`, and its UI/admin detail MUST state that it is not
an AASTMT-published course. Synthetic rows pass the same credit, duplicate,
reference, and cycle validation and cannot silently replace an official-source
row.
