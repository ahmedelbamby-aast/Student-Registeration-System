# Neutral Design Token Contract

**Schema:** `design-token-set/2.0`<br>
**Current token version:** `2.0.0`<br>
**Scope:** neutral, accessible demo presentation  
**Approval authority:** Ahmed ELbamby

An approved version is immutable. Any value or semantic-name change creates a
new version, reruns contrast/focus/component checks, and requires approval.
Components consume CSS custom properties generated from the canonical JSON;
they do not introduce ungoverned visual literals.

The immutable v1 source is archived at
`src/StudentRegistration.Client/wwwroot/design/archive/design-tokens.v1.0.0.json`
with SHA-256
`4f5da90c9ed10c769bee5b0f895ed714ccef230f0d735d8c8f25a407f8a9865d`.
It is retained as historical evidence and is never projected into runtime CSS.

## Version 2.0 approved visual direction

Ahmed ELbamby approved this neutral modern-academic light palette on
2026-07-20. It is not derived from the AASTMT logo.

| Intent | Value |
|---|---|
| canvas / surface / muted surface | `#F6F8FC` / `#FFFFFF` / `#EEF2F7` |
| primary / secondary text / border | `#172033` / `#475569` / `#CBD5E1` |
| accent / hover / soft accent / focus | `#3730A3` / `#312E81` / `#EEF2FF` / `#4F46E5` |
| typography | system UI; display 32px, page 28px, section 20px, body 16px, supporting 14px |
| radius | controls 8px; surfaces 12px |
| gutters | mobile 16px; tablet 24px; desktop 32px |
| surface padding | comfortable 24px; compact 16px |

Blue, green, amber, and red semantic pairs must satisfy the contrast contract.
All controls remain at least 44px in both densities. Version 2.0 is light-only;
dark mode requires a separately approved contract.

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

- `wwwroot/design/design-tokens.json` is the current approved source of truth;
  immutable v1 remains archived.
- `wwwroot/css/design-tokens.css` contains only deterministic CSS variables and
  accessibility media handling derived from the approved source.
- `wwwroot/css/app.css` and component isolated styles reference variables with
  `var(--srs-...)`; visual literals are permitted only inside the generated
  token projection.
- High contrast, forced colors, reduced motion, zoom/reflow, and browser default
  controls retain usable visible states.
- Verification rejects mismatched or extra projected values, undefined
  variables, alias cycles, external font imports, and raw visual literals
  outside the generated token projection.
