# How to complete the SPEC-018 NVDA manual test

This guide explains, in simple English, how to perform a manual keyboard and
NVDA test for the Student Registration System if one is requested in the
future. Follow it in order. Do not mark a route as passed unless a real person
completed the steps and heard or observed the expected result.

The official result form is
[`release-evidence/SPEC-018-screen-reader-manual.md`](release-evidence/SPEC-018-screen-reader-manual.md).
Ahmed approved a `WAIVED-DEMO` result on 2026-07-20 because manual NVDA testing
is not required for this non-production demo. The official record honestly
says the manual work was not performed. The automated browser and NVDA probes
already passed, but they do not represent a human test. Use this guide only if
the waiver is retired or additional manual evidence is wanted.

## What you need

Use a Windows computer, Google Chrome Stable, headphones or speakers, and
NVDA. The repository's automated probe used NVDA 2026.1.1. You may use that
version or a later official stable version, but record the exact version shown
in **NVDA menu > Help > About NVDA**.

Allow about 60 to 90 minutes. It is easier if two people take part: one person
uses the system and describes what NVDA says, while the other person records
the results. One person may do both jobs if necessary.

Use only synthetic local demo accounts and data. Never record a real student
name, University ID, password, activation secret, or private student data in
the evidence. Use defect IDs such as `NVDA-001` instead.

## Prepare the local demo

Open PowerShell in the repository root:

```powershell
Set-Location 'C:\Users\Ahmed\Documents\Student Registeration System'
```

Follow [`MANUAL_ROLE_TESTING.md`](MANUAL_ROLE_TESTING.md) to start SQL Server,
migrate and seed the database, and start the application. The final application
address must be:

```text
https://localhost:7078/
```

Confirm that this health check succeeds:

```powershell
Invoke-RestMethod `
    -Uri 'https://localhost:7078/api/health' `
    -SkipCertificateCheck
```

The useful local demo accounts are:

| Role | Login page | Username | Local demo password |
|---|---|---|---|
| Student | `/student/login` | `AI2600001` | `DemoLogin@2026!!` if already activated |
| Lecturer | `/staff/login` | `LEC-0001` | `Demo@2026-LEC-0001` |
| Teaching Assistant | `/staff/login` | `TA-0001` | `Demo@2026-TA-0001` |

If the student password is different, use the Git-ignored credential sheet or
activate an unused student as explained in `MANUAL_ROLE_TESTING.md`. Do not put
the password in the evidence.

The normal seed may show honest empty states because it does not create a full
catalogue, offering, registration plan, staff assignment, roster, or receipt.
Before the formal session, prepare disposable demo data through the Admin
pages if you want to test populated states. At minimum, prepare:

1. One published subject offering with a group, meeting time, room, Lecturer,
   Teaching Assistant, and capacity.
2. One eligible student plan containing that group.
3. One conflicting or otherwise blocked plan.
4. One completed registration receipt that belongs to the demo student.
5. One staff assignment with a roster that belongs to the Lecturer or TA.
6. One editable staff-availability period.

Record the disposable offering, group, and receipt identifiers privately for
the session. Do not add passwords or real personal data. If populated data is
not available, you may test the empty state, but you must write `EMPTY STATE
ONLY` in the notes. A route that requires an interaction which was not
available cannot receive a complete pass.

## Start NVDA and Chrome

Close other screen readers. Start NVDA, then start Google Chrome Stable. Keep
Chrome zoom at 100% for the first pass. Use a normal browser window unless your
local session needs Incognito mode to start signed out.

In NVDA, open **NVDA menu > Tools > Speech Viewer**. Speech Viewer is useful
because the recorder can read what NVDA announced. Do not capture or save text
that contains a password.

Use the keyboard for the complete test. Do not use the mouse to rescue a step.
If you must use the mouse, record that route as failed and explain why.

The **NVDA key** is normally `Insert` or `Caps Lock`. These are the commands
you will use most often:

| Keys | Meaning |
|---|---|
| `Tab` / `Shift+Tab` | Move forward or backward through controls |
| `Enter` | Open a link or activate the focused command |
| `Space` | Activate a button, checkbox, or selected control |
| `Escape` | Close a dialog or cancel the current popup |
| Arrow keys | Move inside a list, radio group, menu, or select control |
| `H` / `Shift+H` | Move to the next or previous heading |
| `1` / `Shift+1` | Move to the next or previous level-one heading |
| `D` / `Shift+D` | Move between landmarks |
| `F` / `Shift+F` | Move between form fields |
| `B` / `Shift+B` | Move between buttons |
| `T` / `Shift+T` | Move between tables |
| `NVDA+Tab` | Read the currently focused item |
| `NVDA+T` | Read the browser-page title |
| `NVDA+F7` | Open NVDA's list of links, headings, and landmarks |
| `NVDA+Space` | Switch between browse mode and focus mode if needed |
| `Ctrl+Home` | Return to the beginning of the page |

## What to check on every page

Start each route at the top of the page. Press `Ctrl+Home`, then use the
keyboard and NVDA commands above.

The route passes the common checks only when all of these statements are true:

1. `NVDA+T` reads a useful page title, not only a URL or the word “document.”
2. There is one clear level-one heading. It describes the current page.
3. The first useful `Tab` stop is **Skip to main content**. Pressing `Enter`
   moves focus to the main heading or main content.
4. Header, navigation, main content, and footer landmarks have useful names.
5. Every link, button, input, select control, table, and dialog has a useful
   spoken name and role. NVDA must not say only “blank,” “button,” or an
   unexplained identifier.
6. The `Tab` order follows the visible reading order. Focus never disappears
   and never becomes trapped, except inside an open modal dialog.
7. Every focused item has a visible focus indicator for a sighted keyboard
   user.
8. Instructions, unavailable reasons, conflicts, errors, warnings, and success
   messages are understandable without relying on color.
9. A status or validation message is announced when it appears. The user
   should not need to search the whole page to discover the result.
10. Required fields, formats, and validation errors are connected to the
    correct input. NVDA reads the field label and its error together or in a
    clear sequence.
11. Tables have a spoken table name and useful column headings. Moving through
    a cell makes its row or column meaning understandable.
12. A dialog announces its title, keeps `Tab` and `Shift+Tab` inside it, closes
    with `Escape` or **Cancel**, and returns focus to the button that opened it.
13. No keyboard command causes a duplicate submission or an unexplained page
    change.

After the normal pass, set Chrome zoom to 200%, then 400%, and repeat the main
action on the route. The content may reflow, but the focused control, message,
and next action must remain reachable. Record any horizontal scrolling that is
needed for ordinary text or controls.

## Journey 1: student login — AUTH-02

Open:

```text
https://localhost:7078/student/login
```

First complete the common checks. NVDA should announce **Student login** as
the main heading. Move through the form and confirm that the University ID,
password, and sign-in button have clear names.

Enter a valid-looking University ID and an incorrect password, then activate
**Sign in**. The error must be announced as an alert or status message. It must
not reveal whether the account exists. Focus must move to the error summary or
to the first field that needs attention, and the password must not be spoken.

Now enter the valid synthetic student credentials and sign in. NVDA must
announce the successful navigation or the new page heading. Record whether
you could complete both the invalid and valid journey without help.

## Journey 2: staff login — AUTH-04

Sign out, then open:

```text
https://localhost:7078/staff/login
```

NVDA should announce **Staff login** as the main heading. Confirm that the
staff username, password, and sign-in button are named. There must not be a
role selector before authentication.

Submit an incorrect password. Confirm that the generic failure is announced,
the password is not spoken, and no account detail is leaked. Then sign in as
`LEC-0001`. The destination should be the staff area and the server should
identify the Lecturer role. Repeat later with `TA-0001` for the TA-specific
checks.

## Journey 3: subject discovery — STU-02

Sign in as the student and open:

```text
https://localhost:7078/student/subjects
```

Read the heading and page summary. Move to the search and filter controls.
NVDA must announce each label, current value, and any help or availability
meaning.

Enter a search term, apply the filter, clear it, and reset all filters. After
each action, listen for a result-count or status announcement. Open one result
if populated data exists. Confirm that unavailable subjects have a full text
reason, not only a disabled appearance or color.

If the route is empty, NVDA must announce an honest empty-state message and a
useful next action. Record `EMPTY STATE ONLY`; do not claim that filtering and
unavailable-reason behavior passed unless you actually exercised it.

## Journey 4: schedule builder — STU-04

Open:

```text
https://localhost:7078/student/schedule
```

Confirm that NVDA explains the selected groups, total credits, and primary
actions. If both calendar and list views exist, move through both. The list
view must communicate the same subject, group, day, start time, end time, room,
and staff information as the calendar.

Add or select groups with a known time conflict. NVDA must announce the word
**Conflict** and identify both meetings, including subject, group, day, start,
and end. The conflict must not be communicated only by a red mark. Remove or
resolve the conflict and confirm that the updated status is announced.

Move to the review action using only the keyboard. If no populated plan exists,
record exactly which steps were unavailable.

## Journey 5: registration review — STU-05

Open:

```text
https://localhost:7078/student/review
```

Test a blocked plan first. NVDA must read every blocking reason and the submit
button must remain disabled. The reason must explain what the student can do
next.

Test a valid plan next. Activate **Review and submit**. The confirmation dialog
must announce its title and summary. Press `Tab` and `Shift+Tab` several times;
focus must stay inside the dialog. Press `Escape` or **Cancel**. Focus must
return to **Review and submit**.

Open the dialog again and confirm once. The submit control must become
unavailable while processing. Pressing the key twice must not create two
submissions. NVDA must announce the final accepted or rejected result and the
next action.

## Journey 6: registration result — STU-06

Use the receipt identifier returned by Journey 5 and open:

```text
https://localhost:7078/student/registration/result/<receipt-id>
```

NVDA must clearly announce whether the registration was accepted or rejected.
It must explain that the result is atomic and that no partial registration was
silently created. Move through the receipt details and confirm that labels and
values are understandable in reading order.

Move to **Print receipt** and any recovery or return link. Each action must have
a useful name. Also try a random receipt identifier. The page must give a safe
not-found or denied message without reading another student's information.

## Journey 7: staff dashboard — STF-01

Sign in as `LEC-0001` and open:

```text
https://localhost:7078/staff
```

NVDA must announce the active staff role, the page heading, assignments, and
any warning or empty-state message. Each assignment link must include enough
spoken information to identify the subject and group. Follow an assignment to
its timetable or roster using only the keyboard.

With the stock seed, **No authorized assignments** is a valid empty state. It
must be announced clearly, but it does not prove the assignment-link journey.
Record `EMPTY STATE ONLY` when this happens.

Repeat the role and assignment check as `TA-0001`. The TA must not hear or see
Lecturer-only assignments or private student academic information.

## Journey 8: staff roster — STF-03

Use a real group identifier assigned to the signed-in Lecturer or TA:

```text
https://localhost:7078/staff/groups/<group-id>/roster
```

NVDA must announce the group context and a named roster table. Move through
the table with table-navigation commands. The three expected meanings are
University ID, display name, and enrollment state. A TA or Lecturer must not
receive GPA, academic standing, holds, contact information, grades, or
transcript information.

Exercise next/previous paging if it is available. The page number and result
change must be announced. Replace the group ID with a random or unassigned ID.
The result must be safely denied or not found, with no student rows spoken.

## Journey 9: staff availability — STF-04

Open:

```text
https://localhost:7078/staff/availability
```

Move through the day, start-time, end-time, label, add/edit/remove, and save
controls. NVDA must announce each field's label, value, and purpose.

Create an invalid range where start is equal to or later than end. Then create
an overlapping range. In both cases, the validation summary and field error
must be announced, and the invalid values must not save.

Create a valid range and save the complete set. NVDA must announce success and
the server version or updated time. If the deadline is closed or the data is
stale, the message must clearly explain that state and the next action. It must
not be communicated only by color.

## How to decide pass or fail

Mark a route `PASS` only when the tester completed every available required
step using the keyboard, NVDA announced the necessary names, roles, states,
errors, and results, and no critical or major defect remains open.

Mark a route `FAIL` when any of these occurs:

- a required control cannot be reached or activated by keyboard;
- focus disappears, moves in a confusing order, or becomes trapped;
- NVDA does not announce an important label, error, warning, dialog, status,
  conflict, or result;
- a password, secret, or another user's protected information is spoken;
- color is the only way to understand a required state;
- the user cannot complete the critical journey without mouse assistance;
- a required populated interaction was unavailable and therefore untested.

Use these defect levels:

| Level | Meaning |
|---|---|
| Critical | The user cannot complete a critical journey, protected data is exposed, or an unsafe action occurs. |
| Major | An important step is seriously confusing or inaccessible, but a difficult workaround exists. |
| Minor | The journey remains understandable and usable, but wording, announcement timing, or focus behavior should improve. |

Any critical or major defect keeps the SPEC-018 gate blocked until it is fixed
and the failed route is tested again.

## Record each route immediately

Copy this block once for each of the nine routes. Use a privacy-safe tester
code instead of a personal name if desired, but keep a separate controlled
record that identifies who performed the test.

```markdown
### <route ID and path>

- Tester code:
- Test date and local time:
- Windows version:
- Google Chrome exact version:
- NVDA exact version:
- Demo account role:
- Data state: populated / empty state only
- Keyboard result: PASS / FAIL
- NVDA result: PASS / FAIL
- Overall result: PASS / FAIL
- Steps completed:
- What NVDA announced correctly:
- Problems or missing announcements:
- Mouse assistance used: Yes / No
- Defect IDs or `NONE`:
- Retest result and date, if needed:
```

Do not place passwords, secrets, real University IDs, or copied private roster
data in this record.

## Complete the official evidence file

If the demo waiver is retired and all nine journeys are finished, open
`docs/release-evidence/SPEC-018-screen-reader-manual.md` and replace only the
placeholders with real facts from the session.

1. Record the tester, date, exact Windows, Chrome, and NVDA versions.
2. Update every route row with keyboard result, NVDA result, defect IDs, and
   final status.
3. Update the machine-readable block with the same facts.
4. Add every defect and its final disposition. Do not write `NONE` if a defect
   is still open.
5. Ask the UX reviewer and QA reviewer to read the completed evidence and sign
   with their names and dates.
6. Change `Status` and `Release gate` to `PASS` only when all nine routes pass,
   all critical or major defects are closed and retested, and both sign-offs
   are present.

The tester, UX reviewer, and QA reviewer may be the same person for this demo
only if Ahmed explicitly accepts that arrangement. Record that decision in the
evidence rather than hiding it.

## Optional automated probe

The repository includes an automated Windows/NVDA integration probe. It is
useful as an additional check, but it is not a human verdict. If the portable
NVDA executable exists at `.local/nvda-2026.1.1/nvda.exe`, close any running
NVDA instance and run:

```powershell
.\tests\StudentRegistration.AccessibilityTests\Run-NvdaProbe.ps1 `
    -Configuration Release
```

The command writes Git-ignored evidence under `.local/accessibility-evidence`.
Keep the human route notes and signed official record even when this probe
passes.

## Final check

Manual evidence is complete only when you can answer **Yes** to all of these
questions:

- Did a real identified tester complete all nine routes with keyboard and NVDA?
- Are the exact Windows, Chrome, and NVDA versions recorded?
- Does every route have a keyboard result, NVDA result, overall result, and
  defect status?
- Are all critical and major defects fixed and retested?
- Are UX and QA names, dates, and decisions recorded?
- Does the official evidence contain no `NOT EXECUTED`, `NOT RECORDED`,
  `UNSIGNED`, or `BLOCKED` placeholder that should have been replaced?

If any answer is **No**, do not replace the honest `WAIVED-DEMO` record with a
manual `PASS`. Record what remains to be done for any future production or
institutional accessibility review.
