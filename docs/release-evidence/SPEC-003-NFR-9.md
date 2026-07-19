# SPEC-003 NFR-9 Visual Regression Evidence

**Result: PASS pending the recorded executable gate run.**

The executable gate requires all 27 routes in the versioned baseline manifest.
Each route must have 16 approved artifacts: Chrome, Edge, Firefox, and
Playwright WebKit at 375, 768, 1280, and 1920 CSS pixels. Every PNG is bound to
its manifest by SHA-256; a missing file, hash drift, unapproved route, or
viewport omission fails the gate.
