# API Versioning Policy

**Current public API version: v1**

The MVP's approved `/api/...` operations form the logical v1 contract. This
avoids a versioning package or duplicate route tree before a real breaking
change exists. Generated OpenAPI records version `v1` and is the review source
for the effective operation, schema, status code, and security requirement
surface.

## Non-breaking changes

Compatible additions may stay in v1 after contract tests and explicit review.
Examples include a new operation, a new optional response field, or a new
optional request capability whose absence preserves existing behavior. New
enum values are breaking unless the owning contract already declares unknown
value handling.

## Breaking changes

A change is breaking when an existing client must change to preserve its
current behavior. This includes removing or renaming an operation or field,
changing a field's meaning/type/nullability, adding a required request field,
removing an outcome, changing a status code, or strengthening a security
requirement without a compatible transition.

Do not introduce v2 until a concrete breaking change, affected clients,
migration path, compatibility window, and Ahmed ELbamby's explicit approval are
recorded. The proposal must include generated OpenAPI before/after artifacts,
semantic diff results, owner, rollout/rollback plan, and removal criteria.

## OpenAPI drift and approval

CI must compare the real generated OpenAPI document semantically once approved
version-pinned handlers and a generator exist. An operation, schema, status
code, or security requirement change requires review. Formatting or ordering
changes alone are not semantic drift, and an empty or hand-authored document is
not a baseline.

## Deprecation

Deprecation is announced in the contract and release notes with an owner,
replacement, affected consumers, support window, and removal approval. The old
v1 behavior remains tested throughout that window. Production removal remains
blocked until usage evidence and the applicable release gate confirm that the
approved consumers have migrated.
