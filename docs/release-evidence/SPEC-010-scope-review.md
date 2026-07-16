# SPEC-010 Scheduling Scope Review

**Review date:** 2026-07-16  
**Decision authority:** Ahmed ELbamby  
**Boundary:** Gate A non-production demo

## Verified exclusions

| ID | Verified exclusion | Delivered-source result |
|---|---|---|
| OS-1 | No institution-wide timetable generation | Scheduling accepts explicit offering, group, meeting, room, and staff inputs. No institution timetable generator, optimizer, or automatic whole-campus scheduling service exists. |
| OS-2 | No automatic reassignment | Resource and availability changes create durable impact alerts for Admin revalidation. They do not silently move a class, room, Lecturer, TA, or one student's activity. |
| OS-3 | No waitlist or seat reservation | The group boundary exposes current selection and serialized seat allocation only. No waitlist, reservation hold, expiry, or queued-seat entity/service is present. |
| OS-4 | No force-over-capacity | Capacity changes reject values below active enrollment, and allocation rejects a full group. No Admin override or force-capacity command exists. |

## Architecture and ownership boundary

The implementation remains one modular monolith:

```text
Client -> Scheduling contracts -> Scheduling endpoints
Scheduling endpoints -> Scheduling application/domain ports
SQL infrastructure -> Scheduling mapping contribution -> shared DbContext
```

There is no scheduling microservice, message broker, event-sourced aggregate,
distributed transaction, generic timetable engine, waitlist subsystem, or
capacity override workflow.

SPEC-010 owns ADM-06 and ADM-07 scheduling behavior. SPEC-017 contributor work
for broader monitoring, audit, reporting, and exports is downstream and is not
claimed by this scope review. Staff availability mutation remains owned by
the SPEC-016 workspace contract; Admin use is read-only.

## Authority boundary

This review covers synthetic Development and Testing demo behavior only. It
does not authorize production timetable generation, official institutional
resource publication, production migration execution, or AASTMT go-live.

**Result: PASS.**
