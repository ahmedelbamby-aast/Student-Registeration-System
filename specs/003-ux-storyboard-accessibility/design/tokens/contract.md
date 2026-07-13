# Neutral Design Token Contract

**Schema:** `design-token-set/1.0`  
**Initial token version:** `1.0.0`  
**Scope:** neutral, accessible demo presentation  
**Approval authority:** Ahmed ELbamby

An approved version is immutable. Any value or semantic-name change creates a
new version, reruns contrast/focus/component checks, and requires approval.
Components consume CSS custom properties generated from the canonical JSON;
they do not introduce ungoverned visual literals.

## Three layers

1. **primitive** tokens hold neutral raw values such as gray scale steps,
   spacing increments, font measures, radii, and durations.
2. **semantic** tokens express intent such as surface, text, border, focus,
   success, warning, danger, information, disabled, and pending.
3. **component** tokens alias semantic tokens for a specific reusable component
   and state. Component tokens never bypass or redefine the semantic meaning.

Only the canonical JSON is authored. The CSS file is its deterministic
projection, with names shaped as `--srs-{category}-{token}`. Missing aliases,
cycles, unknown categories, and literal component colors fail the contract.

## Governed categories

| Category | Contract |
|---|---|
| color | Neutral surface/text/border plus semantic information, success, warning, danger, pending, disabled, link, and focus pairs. Normal text is at least 4.5:1; large text is at least 3:1; focus and meaningful non-text boundaries are at least 3:1. Meaning is never color-only. |
| typography | System UI fallback stack, readable size/line-height scale, normal/bold weights, and constrained measure. No institutional typeface is asserted. |
| spacing | A small rem-based primitive scale used consistently for rhythm and gap. |
| sizing | Content widths, control heights, and minimum interactive target of 44 CSS px. |
| border | Width, neutral style, and radius primitives; state borders use semantic color aliases. |
| focus | Visible outline width, offset, and high-contrast semantic color; focus is not removed. |
| elevation | Minimal neutral shadows for overlays only; elevation is not the sole boundary cue. |
| motion | Short feedback durations with reduced-motion equivalents and no required information carried only by animation. |
| breakpoint | The approved 320, 375, 768, 1024, 1280, and 1920 CSS px design widths. |
| z-index | Small named layers for base, sticky, overlay, dialog, and urgent feedback; arbitrary escalation is prohibited. |

## Brand boundary

The official local AASTMT logo is a separate provenance-recorded asset. Token
values must not be inferred from the AASTMT logo. Logo pixels, sampled colors,
shape, or typography do not authorize an institutional palette, font, or usage
rule. The token set records `scope: neutral` and
`brandValuesDerivedFromLogo: false`.

## Projection and use

- `wwwroot/design/design-tokens.json` is the approved source of truth.
- `wwwroot/css/design-tokens.css` contains only deterministic CSS variables and
  accessibility media handling derived from the approved source.
- `wwwroot/css/app.css` and component isolated styles reference variables with
  `var(--srs-...)`; visual literals are permitted only inside the generated
  token projection.
- High contrast, forced colors, reduced motion, zoom/reflow, and browser default
  controls retain usable visible states.
