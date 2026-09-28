# Unity Job Progression

A small Unity package for RPG jobs, ranks and XP, extracted from an unfinished game prototype. Jobs and ranks are ScriptableObjects; a runtime manager tracks progress and publishes events for game-specific integrations.

[![Repository checks](https://github.com/Tigertycoon/unity-job-progression/actions/workflows/checks.yml/badge.svg)](https://github.com/Tigertycoon/unity-job-progression/actions/workflows/checks.yml)

The package includes eight runtime scripts and sample data for **12 jobs and nine shared ranks**. It requires no Asset Store plugins or external artwork. This is a focused gameplay subsystem, not a complete RPG.

## Progression model

| Value | Scope | Behavior |
| --- | --- | --- |
| Life XP | Per job | Accumulates independently; available for game-specific skill systems |
| Stars | Per job | Cumulative requirement for a rank; stars are not spent |
| Character XP | Shared | Advances character level and carries the remaining XP forward |

The API retains the original term **Life** for a job. Switching jobs preserves each job's progress. Ranking up is explicit: each successful call advances one tier and emits `OnRankUp`. Character XP can advance several levels in one reward.

Stat formulas cover HP, SP, physical/magic attack and defense, critical chance, crafting modifiers and damage variance. They are separate from the manager: progression does not automatically apply stats to a character or inventory.

## Install

Tested with **Unity 6000.3.19f1**. In Package Manager, choose **Install package from git URL**:

```text
https://github.com/Tigertycoon/unity-job-progression.git#v0.1.0
```

Import **Example Jobs** from the package's Samples tab to get the job and rank assets. These are data assets; there is no scene or visual demo.

## Use

1. Add `LifeProgressionManager` to a dedicated root GameObject. It survives scene changes and rejects duplicate manager objects.
2. Assign imported job assets to `availableLives`, or register a job by selecting it or awarding it points through the API.
3. Connect rewards and UI through public methods and events. `LifeDebugUI` provides an optional read-only IMGUI overlay.

```csharp
using JobProgression.Data;
using JobProgression.Systems;
using UnityEngine;

public class JobExample : MonoBehaviour
{
    [SerializeField] private LifeDefinition job;

    private void Start()
    {
        var progression = LifeProgressionManager.Instance;
        progression.SetActiveLife(job);
        progression.AddLifeXP(20);
        progression.AddStars(10); // First sample rank requires 10 cumulative stars.
        progression.TryRankUp();
        progression.AddCharacterXP(400); // Default curve: level 3, 50 XP remaining.
    }
}
```

Use `AddLifeXP(job, amount)` and `AddStars(job, amount)` when a reward belongs to a specific job rather than whichever job is currently active. Subscribe to `OnLifeChanged`, `OnRankUp`, `OnLifeXPGained`, `OnStarsGained` and `OnCharacterLevelUp`; unsubscribe when your subscriber is destroyed.

Create custom definitions through **Assets → Create → Job Progression**. Ranks use contiguous tiers starting at zero. Required stars are cumulative totals. Each `rankStatBonuses` entry is an incremental bonus; `GetAccumulatedBonuses(tier)` sums the entries through that tier.

## Verification

The repository contains Edit Mode and Play Mode tests, including actual sample-asset import and a scene unload. Run both suites from PowerShell:

```powershell
./scripts/test-unity.ps1 -UnityPath 'C:\Program Files\Unity\Hub\Editor\6000.3.19f1\Editor\Unity.exe'
```

The wrapper copies the `TestProject~` template into a fresh directory in the system temp folder, points it at this package, and writes NUnit XML and editor logs under `artifacts~`. The temporary project is retained for inspection. See the [manual setup](Documentation~/verification.md) for other systems.

GitHub Actions checks the package manifest, source boundary, sample references and GUIDs. **The GitHub badge does not represent a Unity test run**; Unity compilation and behavior are verified locally. See [verification details](Documentation~/verification.md).

## Scope and origin

This code comes from my local **Fantasy Life** RPG prototype. The public extraction keeps the progression model and example definitions, removes plugin coupling, and adds lifecycle fixes and regression tests. [Extraction notes](Documentation~/extraction.md) distinguish the retained code from preparation work.

Progress lives in memory and is lost when the application exits. Save/load, quest completion, inventory, combat controllers, automatic stat application and a player-facing UI remain outside this package. The example jobs describe gameplay roles; their text does not implement the corresponding abilities. Sample values are prototype tuning, not validated game balance.

MIT licensed. See [LICENSE](LICENSE).
