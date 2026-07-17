# SPEC-014 Dependency Baseline

**Recorded:** 2026-07-17<br>
**Repository baseline:** `cb61c1e974ad2a2235703386fab84a9172e3c8eb`<br>
**Result:** PASS after the SPEC-014 consistency reconciliations recorded below

## Accepted artifacts

| Task | Specification | Accepted baseline | Artifact SHA-256 (`requirements`, `plan`, `data-model`, `contracts/api`) |
|---|---|---|---|
| T002 | SPEC-003 | Approved Gate A design baseline; STU-05 record 1.0, frontend design index 1.1, route manifest 2.1.0, page/API manifest 1.1.0 | `0461b4d864a5898cc02ee124103d3199517754173400df6c280bb8dba19c3491`, `283c7efd72ae5a3e0a55abc7d81d45e471b4a9bc9a58a0cad81daba34d9d281f`, `e0af1871709e4e28de2cf99eee7f31ebf63344d1b71e0864879ec7e1cfb566df`, `03218fb4aadae2e2b9420559316816776aa4530eeec117fcedd9b98798a9c31e` |
| T003 | SPEC-007 | Approved and completed runtime baseline `c3e26a1e8b237bbe66b8ae4a5f98b2c1cdc4a82c` | `da64ac6585e09fcb67105d3b755f095d969f76aeecb693dcf0214de828fb1fbc`, `3ded6edf4763a5c21c1396dedd08d404650e5f4c551cdf329a6d5c2ece610192`, `c0821084dbe77b8354c4e94b3cd615a2463141b7b99f4088682eed17406d0ab4`, `28b2cd5b9e1ecc3152cd6bf5ba2ba48bb85255a94c7ad44fdce72a9c04997e5b` |
| T004 | SPEC-008 | Approved shared academic boundary baseline; artifacts last changed at `d2eb68d3` | `152f102d24c3c7f44b7b57400e76c590082306cea5e0fdbed20773dcd312c5b5`, `ba6c8fd308b85fe3db8d51d2d87b0ecc72871cb9a82f57f0ac6f7257c392949f`, `6bbf406a1f9d3afb55424e4dc5091bfe3162a01d464671b19c02afbfc8d49678`, `2e4d6177c66ab60c2214e9fd008c8531c7fecb28dfccfcc804cb50debc61e257` |
| T005 | SPEC-009 | Approved/completed baseline `12cfecfab1787cb719bda44318bb604bca603a22` | `d2bc3706c37711267c90dad7f3b025f3d432f51468b2357914f93b65e55f0e9e`, `170059f5b7a9247bd97fc291cbf940b9eb79a7957c5cfb6259c1edbdc7685764`, `428e30f129cf5af000b69596f2348fd8fc7f3ae47e9ebf57df413ad3678314e6`, `1a809fec654af45577b4ce73407690dddd678dfdffc4f943658c06ef4c10fb58` |
| T006 | SPEC-010 | Approved/completed baseline `99c25eee2fc1078605a426d19325351e69916140` | `7f5d133195dab8ac0d17092cd096eeea1300e8ed11f95689231f042243ed0e41`, `423dc929bebc756db37efdd77eee8244b621e9c7f2d9cb38385a85430c28cd0d`, `83862f7065e299cc622648fd9dfe1b7cab0e4532207e96a339835a5bc0e5f739`, `e17932d1088b28ac9738cc874bc269103cd0609e0af35d012a00384703e078c4` |
| T007 | SPEC-011 | Approved/completed baseline `0182713780fa4cace24ea4ffea4065f764b8e6b7` | `64c69a3781990f1112d3982a18c4c90b0aed34d1aa36707819f250f8c97aec1f`, `6a80a9b6ef25ac7e2e43fa7d9579b0199cc18b6482b53e6024a9e90e8ba39211`, `626126bfdecaecb2ff95da1fc942a107a538c1b388cdff3b7598e3ff6ea4a6c1`, `1674c5992f8ae9035a7125ad5c2c2446232bffbff85bb59386b0ed094358e994` |
| T008 | SPEC-012 | Approved/completed baseline `c76af079d9029ab70459bee7c0a176a2fa3bac58`; credit-load response amendment `spec012-credit-load/1.0` | `bd53b32d87b0915e4aa85a03ff2392aa91c9a9e4ad1f8dc759d945c50dce4450`, `dae8e52cc1f9c8ded92c9210a6bf10bb0df2b7f56f895ca6543ab6c4ca4b23b3`, `0c4c36458c174bc87de082f6d0fbd6ba01077e9c3954a3824132be0903e636d1`, `3f07c011e1fd64e208b28576aaef01aebd2ce0191a8f0ba03e63a844ff9f7ba3` |
| T009 | SPEC-013 | Approved/completed baseline `cb61c1e974ad2a2235703386fab84a9172e3c8eb` | `79c3f66f47cfe5643fa8bdde3179d2a0b56a96d3b9d3ded1e542898907a3784b`, `5393aa810e5747a02ebc9cea79aa0bdc78749ba439ba257240200901d2017bb4`, `039a1c980f65cdc3f26f071ff894f4ac62abb9601b3d715d20c7b68b42f5e889`, `0fb69cb8dd38200d086f5899c36814aed8ba744351b6de66591ecd55095cc31d` |
| T010 | SPEC-018 | Approved operations contract baseline `4ac311825bf2c82ec382e488db1a4de145650c79` | `9e0973c6670fec97733c2c0de15ffb4a5597889497216829c0a90f87fcd7118d`, `9dc035269854e7f142a9d4db323240360edcf43a187ca7af1b85e56215f6565a`, `940e519d5e9fad78908a00a40f9c0b905f4e41edc190ec8766e6220450814486`, `0423b7ab07d56a1e529a00378f24c0238a12bdcb628d107cbbb8eeee2dd07c6b` |

## Consumed contracts and reconciliations

- SPEC-003 retains design/test-contract ownership. SPEC-014 implements STU-05
  and later pins its contributor evidence; STU-06 and ADM-08 remain bounded
  contributor contracts owned for implementation by SPEC-015 and SPEC-017.
- Both registration endpoints require the server-issued Student role and exact
  `Registration.SubmitOwn` permission from the approved SPEC-001 permission
  vocabulary. POST additionally requires same-origin antiforgery. SPEC-014
  adds the missing runtime policy/grant and proves denial when the claim is
  absent; it does not accept client identity.
- SPEC-014 consumes SPEC-008's unique `StudentTermAcademicState` and
  `ExecuteRegistrationBoundaryAsync` callback/rowversion protocol. It does not
  introduce a second student-term guard or in-memory lock.
- Final commit ignores advisory plan/recommendation credit-cap projections. It
  re-reads the effective SPEC-009 PolicySet and academic state and recomputes
  the authoritative maximum: 12 credits when GPA is below 2.0, otherwise 18.
  `POLICY_CHANGED` is reserved for a changed governing version; a current
  probation violation keeps its stable policy reason.
- Catalogue and policy publication share serializable update/range locks on
  their normalized scope-code indexes with registration. After those locks,
  registration re-reads effective IDs and versions before it locks sorted
  SPEC-010 SectionGroup rows.
- Receipt data preserves SPEC-010 meeting identity/activity and meeting-bound
  staff, carries PolicySet ID plus version, and uses the accepted .NET
  `DayOfWeek` encoding `0..6`.
- SPEC-011 eligibility, SPEC-012 plans, and SPEC-013 recommendations are
  advisory inputs only. Registration always consumes the canonical current
  plan and revalidates every mutable input inside the commit transaction.
- SPEC-018 owns the operational metrics endpoint and the mandatory target and
  200/s spike profiles. SPEC-014 emits its safe signals into that boundary;
  SPEC-017 may consume them read-only. Optional 2x/5x/soak profiles do not
  become release gates.
- The S6 migration persists SPEC-014-owned registration state and is therefore
  delivered with the SPEC-014 mapping. SPEC-015 remains a projection over the
  accepted submission/receipt snapshot rather than a prerequisite table owner.

## Dependency result

All links are valid and the dependency graph is acyclic. The accepted upstream
contracts are sufficient for SPEC-014 after the recorded consistency
reconciliations. Upstream approval does not inherit SPEC-014 runtime,
concurrency, load, accessibility, traceability, or release evidence; every
SPEC-014 task still requires its own named evidence before it may be checked.
