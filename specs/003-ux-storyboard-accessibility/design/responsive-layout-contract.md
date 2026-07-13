# Responsive Layout Contract

**Version:** `responsive-layout/1.0`  
**Strategy:** mobile-first CSS reflow using content breakpoints

Every Page Design Record describes the same semantic content at the six required
viewport widths. Width changes may alter composition, never authorization,
available data, state meaning, or action outcome.

| Design width | Navigation | Table alternative | Action placement | Content order | Overflow |
|---|---|---|---|---|---|
| 320 CSS px | Collapsed menu button; skip link remains first | Label/value cards or chronological rows | One-column, primary action last in reading order and reachable without horizontal scroll | Heading, status, primary data, secondary data, actions | Wrap text; internal scroll only for a semantic region that has a label and keyboard access |
| 375 CSS px | Same collapsed pattern with 44px targets | Cards/list remain default | Full-width actions may share a row only when labels fit | Same DOM and reading order as 320 | No page-level horizontal scrolling |
| 768 CSS px | Collapsed or compact navigation according to available content width | Table may appear when every column remains understandable; accessible alternative stays available | Primary and secondary actions may align horizontally | Main content precedes complementary content | Wide schedules use a labelled scroll region plus equivalent chronological list |
| 1024 CSS px | Persistent compact navigation when it does not obscure content | Semantic table with headers; priority columns cannot disappear silently | Page actions align with the heading region | Main and complementary regions may form two columns without changing DOM meaning | Sticky content must not cover focused controls or messages |
| 1280 CSS px | Persistent role navigation | Full priority table plus pagination | Primary actions near page title; destructive actions remain separated | Maximum readable line length constrains text panels | Containers grow within approved maximum widths |
| 1920 CSS px | Persistent navigation; content does not stretch edge to edge | Same data and headers as 1280 with useful whitespace | Actions remain attached to their owning region | Same logical order; optional secondary panels use available columns | No content is positioned only by absolute viewport coordinates |

## Required behavior

- CSS container/media queries respond to available content width; user agent or
  device-name branching is prohibited.
- The DOM order remains the keyboard and screen-reader order. Visual reordering
  cannot move an action ahead of its heading or explanation.
- Every interactive target is at least 44 by 44 CSS pixels unless the WCAG
  inline/spacing exception applies and is documented.
- At 400% zoom, content reflows to an experience equivalent to the 320 CSS px
  design. Blocking reason text, validation messages, and recovery actions do
  not truncate or require two-dimensional page scrolling.
- Data tables retain captions, header associations, sorting state, pagination,
  and the same actions in their narrow alternative. A visual card conversion
  does not change the underlying data or hide a required value.
- Calendar grids always have the information-equivalent chronological schedule
  list; horizontal scrolling alone is never the only way to understand time.
- Fixed or sticky elements provide enough scroll padding for focus targets and
  never obscure the active element, validation summary, or live region.
