# Authentication Entry-Point Boundary

**Contract:** `entry-point-boundary/1.0`
**Runtime owner:** SPEC-007
**Frontend route owner:** SPEC-003

## Public choices

| Audience | Allowed route | Allowed public action |
|---|---|---|
| Student | `/student/login` | Sign in with University ID and password |
| Student | `/student/activate` | Activate an institutionally generated student identity |
| Admin, Lecturer, TeachingAssistant | `/staff/login` | Sign in through one shared staff login |

The student experience MUST NOT display a staff role picker. The staff login
has no role selector and accepts no role claim in its request. There is no public staff registration.
No MFA or 2FA is required for this demo.

## Post-authentication behavior

- Student proceeds directly to the authorized student context.
- One supported staff role proceeds to its role-scoped workspace.
- Multiple supported staff roles produce a server-issued context choice limited
  to the effective set; selecting a context does not grant a role.
- No supported staff role produces a safe denial and support reference.

Form controls and client routing are usability mechanisms only. SPEC-007 must
derive the identity and roles on the server; SPEC-003 may expose only routes
declared in its approved route manifest.
