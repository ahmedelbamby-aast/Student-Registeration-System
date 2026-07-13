# Focused Application-Service Boundary

This project uses a modular monolith. Each feature is added as one focused,
module-owned use case and its narrow ports. `StudentRegistration.Api` remains
composition only and never makes business decisions.

## Endpoint responsibilities

An endpoint may bind and validate transport input, require authentication and
authorization, obtain the server-derived actor and scope, invoke one focused
module-owned use case, and translate its declared result to the shared HTTP
contract. It does not query EF entities, calculate policy or schedule choices,
own a transaction, or infer authorization from client-supplied values.

## Application-service responsibilities

A focused application service coordinates one command or query. It accepts
explicit input, the authorized server context, an injected `TimeProvider`, and
a `CancellationToken`; calls module-owned domain behavior and narrow ports;
defines the transaction and idempotency intent for a mutation; and returns a
typed outcome. Cancellation before commit may stop work. After commit, the
durable outcome remains authoritative and retry/replay rules apply.

The service owns orchestration, not HTTP, Blazor, SQL Server, or UI state. A
cross-module dependency uses an approved public port and does not reach into a
different module's internals.

## Domain responsibilities

Domain code owns invariants and deterministic decisions. It remains
infrastructure independent, receives time and relevant facts explicitly, and
never reads browser state, the system clock, claims, EF Core, or SQL directly.

## Composition and extension rule

The API composition root registers concrete module ports, shared transport
policies, and middleware. A new feature adds its use case within the owning
module and a thin endpoint adapter; existing modules do not require a generic
framework layer.

- No generic Application project is introduced.
- No mediator library is introduced for the current handler volume.
- No generic repository hides focused queries or atomic writes.
- No endpoint or browser component becomes a business-rule owner.

These constraints keep feature additions local while preserving a clear seam
for later scaling or extraction when measured load or team ownership requires
it.
