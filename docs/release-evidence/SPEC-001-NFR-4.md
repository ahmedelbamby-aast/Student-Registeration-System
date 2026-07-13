# SPEC-001 NFR-4 modular-monolith evidence

- Requirement: `SPEC-001/NFR-4`
- Evidence date: 2026-07-13
- Automated gate:
  `tests/StudentRegistration.QualityTests/Specs/Spec001/NFR-4EvidenceTests.cs`
- Scope: non-production demo architecture

## Measured architecture

The solution contains exactly **nine governed source projects**. Eight are
in-process client, contract, business-module, or persistence libraries, and
`StudentRegistration.Api` is the **one deployable API**. It is the only project
using `Microsoft.NET.Sdk.Web`, and
`src/StudentRegistration.Api/Composition/ModuleRegistration.cs` contains the
**sole composition root**.

| Measure | Result |
|---|---:|
| Governed source projects | 9 |
| Web/API deployable projects | 1 |
| Composition roots | 1 |
| Explicit project-reference edges | 25 |
| Dependency cycles | 0 |
| Broker/distributed-service framework packages | 0 |

The automated gate reads every source project, requires the exact governed
project set, verifies the reference graph is **explicit and acyclic**, and
rejects known microservice, broker, Dapr, Kafka, RabbitMQ, Service Fabric, and
NServiceBus dependencies. The canonical `ModuleDependencyTests` and
`ProhibitedComplexityTests` independently enforce the same boundary and fail
the architecture suite on drift.

## Replica interpretation

SPEC-004 proves **two replicas of the same API** with shared SQL state and one
encrypted Data Protection key ring. The rows named `api-01` and `api-02` are
instances of `StudentRegistration.Api`; they are not separate business
services or independently deployed modules. Horizontal replication therefore
does not change domain behavior or turn the modular monolith into
microservices.

## Authority boundary

This is architecture evidence for the initial non-production demo and grants
**no production hosting or topology approval**. Production hosting, load
balancing, SQL topology, certificate custody, and deployment authority remain
explicit later Security/DevOps and SPEC-018 gates.

**Result: PASS.** The initial solution remains one deployable modular monolith
with explicit in-process modules and a single API composition/deployment unit.
