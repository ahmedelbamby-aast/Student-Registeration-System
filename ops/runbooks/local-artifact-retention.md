# Local artifact retention runbook

Generated demo files under `.local`, `credentials`, `logs`, and `exports` are
**Git-ignored** and retained for at most **seven days** by default. Source,
specification, test evidence, and committed fixtures are outside this policy.

Run the repository-bounded cleanup from the repository root:

```powershell
pwsh -NoProfile -File ops/scripts/Remove-ExpiredLocalArtifacts.ps1
```

Preview the exact file removals before scheduling or executing them:

```powershell
pwsh -NoProfile -File ops/scripts/Remove-ExpiredLocalArtifacts.ps1 -WhatIf
```

The script resolves every target, rejects absolute and traversal paths, permits
only the four approved roots or their descendants, skips reparse-point
directories, and removes individual expired files rather than directories.
Use the default seven-day policy for development and test. Any different
retention period requires Ahmed's explicit approval and remains limited to
these local artifact roots.
