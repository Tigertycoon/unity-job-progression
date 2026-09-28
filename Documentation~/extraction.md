# Extraction from the RPG prototype

The source project used the working title `Fantasy Life`. Its project-specific folder contained 17 C# scripts alongside data assets. The complete Unity project also contained many third-party art, animation, controller, shader and editor packages.

This repository publishes the small progression subsystem that can stand on its own.

## Retained and adapted

| Original project script | Public role |
| --- | --- |
| CharacterStat, LifeType | Stat and job categories |
| LifeDefinition, RankDefinition, RankStatBonus | ScriptableObject definitions and bonus queries |
| StatFormulas | Derived values and damage calculation |
| LifeProgressionManager | Job selection, counters, ranks, character leveling and events |
| LifeDebugUI | Optional read-only debug overlay |

Twelve job definitions and nine rank definitions were retained as importable samples. Source GUIDs for these assets and the retained scripts were preserved, and serialized class identifiers were updated for the package assembly. Sample assets have no art references.

## Publication preparation

- Changed the namespace from `FantasyLife` to `JobProgression` and moved the code into a Unity package assembly.
- Removed optional Odin annotations and the Malbers `startingEquipment` field. The package has no inventory coupling.
- Initialized an Inspector-assigned active job even when absent from the available-job list; explicit rewards also register their target job.
- Released the singleton reference on destruction and added null-safe definition/progress queries.
- Protected counters against integer wraparound, bounded chance modifiers and enforced a positive increasing XP curve even for invalid old configuration.
- Made exposed progress properties read-only to consuming assemblies; removed routine reward logging.
- Added regression tests, real sample-import verification and package/reference checks.

The original project was not edited. This extraction is not intended as a drop-in migration of its scenes or prefabs.

## Excluded integrations

The original CharacterStatCalculator, EnemySetup, CombatRewardHandler, LifeMasterNPC and LifeXPReward scripts depended on Malbers Stats, Inventory or Quest Forge. They remain in the original project. Enemy/drop definitions were also coupled to those inventory types. SimpleInteractTrigger and the broad data generator were not needed for this package.

No third-party implementation, artwork, music, controller, terrain or shader packages are distributed here. The public API can be connected to those systems by a consuming game through progression events and reward calls.

## Current boundaries

This is a prototype-derived gameplay component. It has no persistence, multiplayer authority, undo, reward deduplication or quest state. The caller decides when a reward is earned and prevents repeat grants. Rank tiers must start at zero and be contiguous; the package does not infer missing ranks. Stat-bonus entries are incremental, and their values are not applied automatically.

The manager supports the normal Unity scene lifecycle and uses one persistent root GameObject. Disabling both domain reload and scene reload is not part of the verified configuration.
