# Creating New Agent Skills

Guidelines for creating new skills that follow the agentskills.io specification and work with GitHub Copilot.

---

## Quick Checklist

Before creating a skill, ensure you can answer:
- [ ] **What does this skill do?** (one sentence)
- [ ] **When should it activate?** (specific triggers/keywords)
- [ ] **What problem does it solve?** (user need)
- [ ] **Is this better as a skill vs inline instructions?** (reusable pattern?)

---

## Skill Structure

```
.github/skills/
└── your-skill-name/
    ├── SKILL.md                    # Required: Main instructions for agent
    └── references/                 # Optional: Additional context loaded on-demand
        ├── REFERENCE.md
        └── EXAMPLES.md
```

**Do NOT create:**
- `scripts/` directory
- `assets/` directory (unless absolutely necessary)
- Multiple skill files (one `SKILL.md` per skill)

---

## SKILL.md Format

### Frontmatter (Required)

```yaml
---
name: your-skill-name
description: >
  Brief description of what the skill does and when to use it.
  Include specific keywords that trigger this skill. Max 1024 characters.
license: CC0-1.0
metadata:
  author: JPEGtheDev
  version: "1.0"
  category: [e.g., recipe, validation, meta, ci]
  project: OpenCookbook
---
```

**Name Rules:**
- Lowercase only, 1–64 characters
- Use hyphens (`-`) not underscores or spaces
- No leading/trailing or consecutive hyphens
- Must match parent directory name

**Description Rules:**
- 1–1024 characters
- Include "what" and "when"
- Use keywords agents will search for

### Body Content

**Key Principles:**
1. **Write for the AI agent, not humans** — use imperative: "Do this", "Check that"
2. **Be conversational first** — instruct the agent to ask questions before acting
3. **Provide clear workflows** — numbered steps or clear sections
4. **Include examples** — show expected behavior and edge cases

**Recommended Sections:**

```markdown
# Instructions for Agent

## How This Skill is Invoked
[How users activate this skill]

## When to Use This Skill
[Specific triggers and contexts]

## Step 1: [First Action]
[Detailed instructions]

## Step 2: [Next Action]
[More instructions]

## Common Edge Cases
[How to handle vague requests, errors, etc.]
```

---

## Progressive Disclosure

Skills should load context efficiently:

| Layer | Token Budget | What Goes Here |
|-------|-------------|----------------|
| Frontmatter | ~100 tokens | Name, description, keywords |
| SKILL.md body | <5000 tokens | Core workflow, examples, key rules |
| References | On-demand | Detailed frameworks, lengthy examples, project-specific data |

**Keep SKILL.md under 500 lines.** Move detailed reference material to `references/`.

---

## Writing Agent Instructions

### ✅ Good: Imperative, Agent-Focused
```markdown
## Step 1: Gather Requirements

Ask the user clarifying questions conversationally:

> "I'd love to help with that! A few quick questions:
> 1. What problem are you trying to solve?
> 2. What's the scope — small fix or larger restructure?"

**Allow them to skip questions** — use sensible defaults if they say "just go".
```

### ❌ Bad: User-Facing Documentation
```markdown
## How to Use This Skill

To use this skill, provide the following information to Copilot:
1. Your requirements
2. The scope of work

The skill will then generate output for you.
```

---

## Anti-Patterns

| ❌ Don't | ✅ Do Instead |
|---|---|
| Collect 10 inputs via a form | Start a conversation, ask 2–3 questions |
| Generate with rigid template | Generate based on context, allow overrides |
| Over-specify implementation | Focus on outcomes, let agent choose approach |
| Include all references in SKILL.md | Link to `references/`, load on-demand |
| Duplicate content across skills | Cross-reference other skills |

---

## Version Management

When updating skills:

1. **Increment version** in frontmatter metadata
2. **Document the reason** — what changed and why
3. **Keep backward compatible** when possible

Version bumps:
- Minor fix/addition: `"1.0"` → `"1.1"`
- Major restructure: `"1.1"` → `"2.0"`

---

## Registering a New Skill

After creating a skill:

1. **Add it to the Skills Directory** in [copilot-instructions.md](copilot-instructions.md)
2. **Add it to the Minimum Skill Loads table** if it applies to specific task types
3. **Add a description entry** in the VS Code skills configuration (`.github/copilot-instructions.md` skills section)

---

## Summary: The Perfect Skill

✅ **Clear activation triggers** in description
✅ **Conversational approach** — asks before generating
✅ **Allows overrides** — user control at every step
✅ **Progressive disclosure** — uses references efficiently
✅ **Under 500 lines** in SKILL.md
✅ **Focused and specific** — does ONE thing well
✅ **Agent-focused instructions** — not user docs
✅ **Versioned** — frontmatter tracks changes
