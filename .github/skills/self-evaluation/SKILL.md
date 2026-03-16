````skill
---
name: self-evaluation
description: End-of-session self-evaluation for continuous improvement. Run before finalizing any session to capture lessons learned, identify skill updates, and improve agent effectiveness. Use when completing a task, reviewing work quality, or improving project skills.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
  category: meta
  project: OpenCookbook
---

# Self-Evaluation

## How This Skill is Invoked

This skill is **mandatory** — `copilot-instructions.md` § Session Lifecycle requires it before every final message. You will also be invoked:
- When explicitly asked: "Run self-evaluation", "What did you learn?", "Improve skills"
- After addressing review feedback that reveals a recurring pattern

---

## Core Principle: Learn From Every Session

Every session produces insights that can improve future agent effectiveness. Capture these systematically — don't let knowledge evaporate between sessions. This is how skills stay current.

---

## Step 1: Review the Session

Examine what happened during this session:

1. **User corrections received** — Did the user correct you? What was wrong?
2. **Mistakes made and self-corrected** — Did you catch your own errors? How?
3. **Patterns discovered** — Did a reusable pattern emerge from the work?
4. **Rules you almost broke** — Did you catch a near-miss on one of the 5 Rules or a Working Principle?
5. **Review/PR feedback** — Were there comments on a PR that reveal a gap?

---

## Step 2: Categorize Lessons

Classify each lesson by where it belongs:

| Category | Examples | Update Target |
|----------|----------|---------------|
| **Recipe format** | Missing `volume_alt`, wrong units, missing field | `recipe-documentation` skill |
| **Recipe validation** | New error pattern, missing checklist item | `recipe-validation` skill |
| **Recipe creation** | Conversation flow issue, gathering gaps | `recipe-creation` skill |
| **Versioning** | Commit format, version bump rules | `recipe-versioning` skill |
| **CI/CD** | Workflow structure, deploy issues | `ci-workflows` skill |
| **PR process** | Title format, description gaps | `pull-request` skill |
| **Rendering** | Layout rules, data mapping issues | `recipe-rendering` skill |
| **User stories** | INVEST gaps, AC format | `user-stories` skill |
| **Execution process** | Planning, verification, iteration | `execution` skill |
| **Cross-cutting** | Applies to all tasks | `copilot-instructions.md` Working Principles |

---

## Step 3: Check Against Existing Knowledge

Before proposing updates, verify the lesson is not already documented:

1. Check `copilot-instructions.md` — Is this rule already in the 5 Rules or Working Principles?
2. Check the relevant skill's `SKILL.md` — Is this rule already stated?
3. Check [lessons.md](../../lessons.md) — Has this mistake been logged before?

**Only propose additions for genuinely new or underemphasized patterns.**

---

## Step 4: Apply Updates

For each new lesson:

### 4a. Log in lessons.md

Add an entry at the **top** of the Log section in [lessons.md](../../lessons.md):

```
### YYYY-MM-DD — Short title

**What happened:** What went wrong or what was discovered.
**Absorbed into:** File or section where the rule now lives.
```

### 4b. Update the relevant skill or instructions

- **High priority** (caused broken output, wrong data, or rework): Update the skill immediately. Add a concrete prevention rule.
- **Medium priority** (improved quality or caught a gap): Update the skill. Add to a checklist or guidelines section.
- **Low priority** (style preference or minor improvement): Log in lessons.md only. Note for future reference.

### 4c. Bump skill version

If you updated a skill's `SKILL.md`, increment the version in the YAML frontmatter:
- Minor fix/addition: `"1.0"` → `"1.1"`
- Major restructure: `"1.1"` → `"2.0"`

---

## Step 5: Generate Session Summary

Include this block in your **final message** to the user:

```markdown
### Session Self-Evaluation
Lessons: [count] | Skills updated: [list or "None"] | Compacted: [files or "None"]
```

**Always include this block**, even if there's nothing to report:

```markdown
### Session Self-Evaluation
Lessons: 0 | Skills updated: None | Compacted: None
```

This ensures the behavior is habitual, not optional.

---

## Anti-Patterns to Avoid

1. **Don't update skills for one-off situations** — Only add patterns that are likely to recur
2. **Don't duplicate across skills** — Each lesson goes in exactly one place
3. **Don't restructure existing skills during self-evaluation** — Add to existing sections, don't reorganize
4. **Don't add lessons that are standard practice** — Focus on OpenCookbook-specific patterns
5. **Don't forget to check existing docs first** — Avoid adding what's already covered
6. **Don't skip the summary block** — Even zero-lesson sessions get it

---

## Reference

For examples of how lessons are captured and absorbed, see [lessons.md](../../lessons.md). Each entry shows the pattern: what happened → where the rule now lives.

````
