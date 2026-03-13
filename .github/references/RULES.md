# Recipe Validation Rules

Full rule set for the `recipe-validation` skill. These rules are checked during review of any recipe YAML file.

---

## Rule Categories

### R1 — Structure

| ID | Rule |
|---|---|
| R1.1 | File must be valid YAML with no syntax errors. |
| R1.2 | `name` field must be present and descriptive. |
| R1.3 | `version` field must be present as a **quoted** string (e.g. `"1.0"`). |
| R1.4 | `author` field must be present. |
| R1.5 | `description` field must be present and non-empty. |
| R1.6 | `status` field must be one of: `stable`, `beta`, `draft`. |
| R1.7 | `ingredients` field must contain at least one group with at least one item. |
| R1.8 | `instructions` field must contain at least one section (unless `status: draft`). |
| R1.9 | File must use `.yaml` extension. **Never `.md`.** |
| R1.10 | Filename must use `Title_Case_With_Underscores.yaml` (no spaces, no hyphens). |
| R1.11 | File must be placed in `Recipes/`, `Recipes/Beta/`, or a named topic subfolder of `Recipes/`. |

### R2 — Ingredients

| ID | Rule |
|---|---|
| R2.1 | Every ingredient must have a `quantity` field. |
| R2.2 | Every ingredient must have a `unit` field. |
| R2.3 | Permitted units: `g`, `ml`, or count words (`cloves`, `sprigs`, `whole`). **No other units.** |
| R2.4 | No imperial units (`lbs.`, `cups`, `oz.`, `tsp.`, `tbsp.`) as primary units. Convert to `g` or `ml`. |
| R2.5 | Spice ingredients with `quantity < 10` and `unit: g` **must** include a `volume_alt` field. |
| R2.6 | Default ingredient group uses `heading: null`. Sub-groups use a descriptive string. |

### R3 — Instructions

| ID | Rule |
|---|---|
| R3.1 | Steps must be in a logical, chronological order. |
| R3.2 | Every ingredient referenced in steps must appear in the `ingredients` list. |
| R3.3 | Every instruction section must have a `type` field: `sequence` or `branch`. |
| R3.4 | Branch sections representing the same choice must share a `branch_group` value. |
| R3.5 | All temperatures must be dual-format: `°C (°F)`. Never just one. |
| R3.6 | Each branch path must be complete on its own — no branch assumes steps from another. |
| R3.7 | Every step must be explicit enough for a novice or literal interpreter — no implicit actions, no assumed knowledge. |

### R4 — Food Safety

| ID | Rule | Minimum Safe Temperature |
|---|---|---|
| R4.1 | Poultry must state a minimum internal temperature. | 74°C (165°F) |
| R4.2 | Ground poultry must state a minimum internal temperature. | 74°C (165°F) |
| R4.3 | Ground beef / pork / lamb must state a minimum internal temperature. | 71°C (160°F) |
| R4.4 | Whole cuts of beef, pork, lamb, veal must state a minimum internal temperature. | 63°C (145°F) |
| R4.5 | Past-expiration-date ingredient advice must include a safety caveat. |  |
| R4.6 | Freezing instructions must specify a maximum storage duration. |  |
| R4.7 | Flash-freeze instructions must describe the process (spacing, timing, transfer). |  |

### R5 — Cross-References

| ID | Rule |
|---|---|
| R5.1 | All `path` values in `related[]` must point to files that exist in the repository. |
| R5.2 | Paths must use `./` for same-directory and `../` for parent-directory references. |
| R5.3 | No absolute file system paths or external URLs for internal recipe references. |
| R5.4 | All `related` paths must reference `.yaml` files. Never `.md`. |

### R6 — Text Quality

| ID | Rule |
|---|---|
| R6.1 | No merged words or editing artifacts (e.g. "Fahrenheittested yet"). |
| R6.2 | No placeholder text (`TBD`, `TODO`) in `stable` or `beta` recipes. |
| R6.3 | Ingredient names must be spelled correctly. |

### R7 — Consistency

| ID | Rule |
|---|---|
| R7.1 | Units must not be mixed for the same ingredient type within a recipe. |
| R7.2 | Terminology for the same ingredient must be consistent throughout the file. |
| R7.3 | The `notes` field must only be present when `status` is `beta` or `draft`. |

---

## Severity Levels

| Level | Meaning | Action |
|---|---|---|
| **Error** | Recipe is incorrect, unsafe, or structurally broken | Must fix before promotion to stable |
| **Warning** | Recipe is inconsistent or suboptimal | Should fix; acceptable in Beta |
| **Info** | Suggestion for improvement | Optional |

Food safety rules (R4) are always **Error** severity.
Structural rules (R1) are **Error** severity.
All others default to **Warning** unless they cause incorrect behavior.

---

## Known Issues in Current Recipes

| File | Issue | Rule | Severity |
|---|---|---|---|
| Chicken_Wings.yaml | No cooking instructions — `status: draft`, pending smoke/bake/sous vide methods | R3.1 | Error |
| Chicken_Wings.yaml | Spice quantities unvalidated — may need halving or wing quantity doubled | R2.1 | Warning |
