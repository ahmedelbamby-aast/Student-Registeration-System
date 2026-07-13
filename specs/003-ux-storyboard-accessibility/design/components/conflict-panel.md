# Conflict Panel Presentation Contract

**Version:** `conflict-panel/1.0`  
**Truth owner:** SPEC-012; recommendation owner: SPEC-013  
**Presentation owner:** SPEC-003

The panel renders a supplied `ConflictView`. It never detects an overlap,
changes a group, ranks alternatives, or permits an override.

## Required presentation

- The heading combines a Red X icon with the visible text **Conflict**. The
  accessible name contains `Conflict`; meaning is not color alone.
- Every involved item shows subject code and name plus group code.
- Every overlap is listed independently with day, start, and end. Multiple
  overlaps are not collapsed into a vague summary.
- A plain-language explanation identifies why submission is blocked without
  exposing internal rule expressions, SQL, or staff-only data.
- Each supplied alternative shows its server summary and affected replacement
  groups without being labelled guaranteed until the server revalidates it.
- Direct manual resolution actions include change group and remove subject
  links when supplied. The component emits the selected action; the owning
  feature performs the mutation and revalidation.

## Interaction and focus

The panel is a labelled region following the schedule it explains. A newly
returned conflict update uses a polite live region and does not steal focus
from the selected group or active control. Following a manual resolution link
moves focus through normal navigation; returning restores focus to the changed
group heading or the first remaining blocker.

The icon is decorative when adjacent to the visible Conflict heading. Links
and buttons use native keyboard behavior and 44 CSS px targets. Hover, focus,
disabled, pending, and error presentation use approved tokens and retain the
full textual reason.

## Failure boundary

Missing or malformed conflict details fail to the safe service-error panel and
reference path. The component never invents a time, group, reason, alternative,
or successful resolution from partial client state.
