# Lessons Learned

> **YOU MUST READ EVERY ENTRY BELOW BEFORE STARTING ANY TASK.**
> Do not skim. Do not skip. If an entry applies to your current task, follow the prevention rule.
> These lessons exist because the mistake already happened. Do not repeat it.

Every entry is a mistake that happened or a non-obvious behavior that was discovered.
Each lesson includes a prevention rule so the same mistake does not happen again.

---

## How to Add a Lesson

After ANY correction from the user, add an entry here. Format:

```
### YYYY-MM-DD — Short title

**What happened:** What went wrong or what was discovered.
**Prevention rule:** A specific, actionable rule to follow in the future.
**Rule ID:** R-number from RULES.md (if applicable), or "None".
```

Add new entries at the **TOP** of the log (newest first).

---

## Log

### 2026-02-28 — Must self-evaluate after editing instructions/skills

**What happened:** After rewriting copilot-instructions.md and all skill files, the assistant
declared the work done without running any verification pass — violating the very rules
it had just written ("Verify Before Done", "Run the validation checklist").
**Prevention rule:** After editing ANY file in `.github/` (instructions, skills, lessons, references),
re-read the file you edited and check it against the rules in copilot-instructions.md. If you wrote
rules, follow them yourself before declaring done.
**Rule ID:** None.

### 2026-02-28 — Imperial-to-metric conversion requires author confirmation

**What happened:** Converting "2 lbs" to 907g was technically correct, but the author
might prefer a round number like 900g or 1000g.
**Prevention rule:** When converting imperial to metric, always show the user the
converted value and ask them to confirm before writing it into the recipe.
**Rule ID:** None.

### 2026-02-28 — Watch for merged words from copy-paste

**What happened:** Recipe text contained "Fahrenheittested yet" instead of
"Fahrenheit tested yet" — two words merged from a copy-paste artifact.
**Prevention rule:** After pasting any text into a recipe, scan the entire text
for merged/concatenated words before saving.
**Rule ID:** R6.1.

### 2026-02-28 — volume_alt is required, not optional, for spices under 10g

**What happened:** A spice ingredient with `quantity < 10` and `unit: g` was
written without a `volume_alt` field.
**Prevention rule:** Every time you write a spice ingredient under 10g, add
`volume_alt` immediately. Do not write the ingredient and plan to add it later.
Look up the value in [SPICE_CONVERSIONS.md](skills/references/SPICE_CONVERSIONS.md).
**Rule ID:** R2.5.

### 2026-02-28 — Migrated recipes must update all internal paths

**What happened:** A recipe was migrated from `.md` to `.yaml` but cross-references
in the `related` field still pointed to `.md` files.
**Prevention rule:** When changing a recipe's file extension or location, search the
entire repo for references to the old path and update them all.
**Rule ID:** R5.4.
