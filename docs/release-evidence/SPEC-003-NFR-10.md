# SPEC-003 NFR-10 Localization Readiness Evidence

**Result: PASS — T268 is complete.**

## Automated evidence

- `ResourceUiTextProvider` is registered in the Blazor composition root and
  delegates to `LocalizedUiText`, which resolves resources with
  `CurrentUICulture`, formats values with `CurrentCulture`, and safely falls
  back to the stable English key.
- The English resource catalogue contains 1,359 entries. The executable source
  inventory observes 2,132 resource calls and 1,260 unique referenced keys;
  every referenced key has a case-insensitive resource entry.
- The full Razor/control-code audit reports zero residual user-facing English
  literals. Machine values remain explicit and auditable: route/state tokens,
  authorization roles, IDs, CSS classes, ARIA protocol values, input formats,
  stable error codes, and the documented sample JSON fixture.
- Every `InvariantCulture` occurrence in client source must match the explicit
  path/context/rationale allowlist in `NFR-10EvidenceTests`. The allowlist is
  limited to machine representations: HTML date/time/number values, HTTP query
  integers, API/JSON decimal values, ISO instant attributes, and the
  deterministic registration-window concurrency hash. User-visible formats
  use `CurrentCulture`.
- Direction readiness is enforced through logical inline/block CSS properties.
  Arabic translations and delivered RTL layouts remain out of scope; this gate
  proves English-first localization readiness.

## Verification run (2026-07-19)

- Client build: PASS, zero warnings and zero errors.
- `NFR_10EvidenceTests`: PASS, 5/5.
- Client unit tests: PASS, 183/183, including the three migrated ARIA
  source-contract assertions.
- Contract tests: 222/223. The only failure is the independent shared-contract
  secret-member policy for concurrently introduced preview/option token DTOs
  (`OfferingValidationResult.PreviewToken`,
  `PublishOfferingRequest.PreviewToken`, `ScheduleOptionDto.OptionToken`, and
  `ApplyScheduleOptionRequest.OptionToken`). It does not exercise localization,
  culture, layout direction, or user-text placement and is not an NFR-10
  failure.

The dedicated gate is fail-closed: a new visible/control-code English literal,
a missing resource key, or a new invariant-culture use without a documented
machine-protocol rationale fails the quality suite.
