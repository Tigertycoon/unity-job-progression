# Verification

On 2026-09-28, both suites passed with Unity 6000.3.19f1 using the included wrapper: **9 Edit Mode tests and 11 Play Mode tests**. The editor logs contained no C# compiler warnings or errors. These are local Unity results, separate from GitHub's repository checks.

## Reproduce

Use Unity 6000.3.19f1. `TestProject~` is a test-project template; its manifest enables this package's test assemblies. The tilde excludes the template from package import. Run the actual project outside the package root.

On Windows:

```powershell
./scripts/test-unity.ps1 -UnityPath '<path to Unity.exe>'
```

The wrapper creates a fresh project in the system temp directory, copies the template settings, and sets its local package dependency to this repository's absolute path. It starts Unity in batch mode for Edit Mode and then Play Mode, waits for each editor to exit, and requires both a successful exit and a nonempty passing NUnit report. Logs and XML are written to a new `artifacts~/tests-<id>` directory for each invocation. The temporary project is retained; its location is printed at startup.

For manual testing on another system, create a new Unity project outside this repository, add this repository as a local package, install Test Framework 1.6.0, and add `"testables": ["com.tigertycoon.job-progression"]` to the project's Packages/manifest.json. Use **Window → General → Test Runner** to run Edit Mode and Play Mode.

Do not open the nested template in place. During verification, that layout passed Edit Mode tests but failed to deserialize Unity's Play Mode test controller. A separate project using the same editor and package passed the runtime suite.

## Coverage

- Definition queries, null entries, rank ordering, cumulative stat bonuses and formula boundaries.
- Importing all 12 example jobs and nine ranks through Unity's AssetDatabase, including resolved script and rank references. Temporary test copies use remapped asset GUIDs so existing imported samples remain untouched.
- Inspector-assigned startup job, job switching and rewards for an inactive job.
- Explicit rank advancement, cumulative stars and independent Life XP.
- Multiple character levels per reward, carried XP and invalid curve configuration.
- Null/negative input handling and saturating counters with accurate event amounts.
- Duplicate managers, singleton teardown and persistence across scene unload.

GitHub Actions runs `python scripts/validate_repository.py`. It validates tracked content, package dependencies, Unity metadata, reference closure and the rank/sample data contract. It does not run Unity, require a Unity license, or substitute for the runtime suites.

No rendered scene, device build or manual visual acceptance is claimed. No gameplay balance or integration with the excluded third-party systems has been verified in this repository.
