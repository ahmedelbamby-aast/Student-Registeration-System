# Timetable Calendar/List Equivalence Contract

**Version:** `timetable-equivalence/1.0`

ScheduleCalendar and ScheduleList render the same canonical collection supplied
by the owning feature. The calendar is never the only representation, and
switching views cannot filter, enrich, reorder semantically, or recalculate the
schedule.

## Required item data

Every meeting instance appears in both views with:

- subject code and name;
- group code and activity kind (lecture, lab, or section);
- Lecturer name for the lecture when supplied;
- each assigned Teaching Assistant name for lab/section when supplied;
- room/location;
- day, start, end, and authoritative timezone context;
- conflict/blocking status in text as well as icon/color;
- the same details and owning-route action link.

Repeated weekly meetings remain distinct meeting instances. A subject with
lecture and lab/section appears once per actual meeting, not as one lossy card.

## Ordering and interaction

The list is chronological by day, then start, end, subject code, group code,
and stable meeting ID. The calendar uses the same order within a cell. View
selection is a presentation preference, keeps the current heading and selected
meeting context, and restores focus to the corresponding item when possible.

Calendar entries are keyboard reachable using documented grid semantics or
ordinary links/buttons. The chronological list uses headings and list/table
semantics with native keyboard behavior. No drag-only operation is permitted;
every available action has a named keyboard-operable control.

## Responsive and update behavior

At narrow widths and 400% zoom the chronological list is the primary readable
view; the calendar may use a labelled horizontal scroll region but never hides
the list. A server refresh updates both projections atomically, announces the
change politely, and does not steal focus. Partial or mismatched projections
fail to a safe state instead of showing contradictory schedules.
