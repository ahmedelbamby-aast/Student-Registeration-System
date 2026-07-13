# RegistrationPlan ERD Reference

- Runtime source dependency: None
- ERD source: `docs/diagrams/ERD.md`
- Ownership source: `.specify/entity-ownership.json` version `2.0.0`
- Persistence source: `.specify/persistence-manifest.json` version `2.1.0`

### RegistrationPlan

- Canonical entity: RegistrationPlan
- Canonical owner: SPEC-012
- Canonical source path: `src/StudentRegistration.Registration/Domain/RegistrationPlan.cs`
- EF contribution: `src/StudentRegistration.Infrastructure.SqlServer/Persistence/Configurations/RegistrationPlanModelConfiguration.cs` (`writable`)

#### Fields

- `uniqueidentifier Id PK`
- `uniqueidentifier StudentId FK`
- `uniqueidentifier TermId FK`
- `string State`
- `rowversion Version`

#### Relationships and invariants

- `STUDENT ||--o{ REGISTRATION_PLAN : prepares`
- AcademicTerm scopes RegistrationPlan.
- `REGISTRATION_PLAN ||--o{ REGISTRATION_PLAN_ITEM : contains`
- A plan item selects an offering and may prefer a section group.
- `Unique RegistrationPlanItem(PlanId, OfferingId)`.
- `rowversion on mutable aggregate roots and admin records` protects draft edits.
- A client plan remains a draft and is revalidated by the server before submission.

#### Ownership boundary

This is a design-time reference only. SPEC-012 alone may implement or change the runtime entity and its EF mapping.
