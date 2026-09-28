"""Check the published source boundary, metadata and sample references (stdlib only)."""
from pathlib import Path
import json
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
files = [Path(p) for p in subprocess.check_output(
    ['git', 'ls-files'], cwd=ROOT, text=True).splitlines()]
assert files, 'No tracked files to validate'
package = json.loads((ROOT/'package.json').read_text(encoding='utf-8'))
assert package['name'] == 'com.tigertycoon.job-progression'
assert package['dependencies'] == {'com.unity.modules.imgui': '1.0.0'}
manifest = json.loads((ROOT/'TestProject~/Packages/manifest.json').read_text(encoding='utf-8'))
assert manifest['dependencies'][package['name']] == 'file:../..'
assert manifest['testables'] == [package['name']]

for path in files:
    assert not any(p.lower() in {'library','logs','temp','obj','node_modules','usersettings'} for p in path.parts), path
    assert path.suffix.lower() not in {'.dll','.exe','.pfx','.zip','.unitypackage','.mp4','.fbx'}, path
    assert (ROOT/path).stat().st_size < 1_000_000, path

guids = {}
for path in files:
    if path.suffix != '.meta':
        continue
    target = ROOT/str(path)[:-5]
    assert target.exists(), f'Orphan metadata: {path}'
    text = (ROOT/path).read_text(encoding='utf-8-sig')
    match = re.search(r'^guid: ([0-9a-f]{32})$', text, re.M)
    assert match, path
    assert match[1] not in guids, f'Duplicate GUID: {path} and {guids.get(match[1])}'
    guids[match[1]] = path

runtime = [p for p in files if p.parts[0] == 'Runtime' and p.suffix == '.cs']
assert len(runtime) == 8
for path in files:
    if path.parts[0] not in {'Runtime','Tests','Samples~'} or path.suffix == '.meta':
        continue
    assert Path(str(path)+'.meta') in files, f'Missing metadata: {path}'
    if path.suffix == '.cs':
        source = (ROOT/path).read_text(encoding='utf-8-sig')
        assert not re.search(r'\b(MalbersAnimations|Sirenix|FantasyLife|UnityEditor)\b', source) or path.parts[0] == 'Tests', path

assets = [p for p in files if p.parts[0] == 'Samples~' and p.suffix == '.asset']
jobs = [p for p in assets if p.parent.name == 'Lives']
ranks = [p for p in assets if p.parent.name == 'Ranks']
assert len(jobs) == 12 and len(ranks) == 9
references = 0
for path in assets:
    text = (ROOT/path).read_text(encoding='utf-8-sig')
    for guid in re.findall(r'guid: ([0-9a-f]{32})', text):
        assert guid in guids, f'Unresolved reference in {path}: {guid}'
        references += 1
    assert 'Tigertycoon.JobProgression::JobProgression.Data.' in text, path
    assert 'startingEquipment' not in text, path

tiers = {}
for path in ranks:
    text = (ROOT/path).read_text(encoding='utf-8-sig')
    tier = int(re.search(r'^  rankTier: (\d+)$', text, re.M)[1])
    stars = int(re.search(r'^  requiredStars: (\d+)$', text, re.M)[1])
    assert tier not in tiers, f'Duplicate rank tier: {path}'
    tiers[tier] = stars
assert sorted(tiers) == list(range(9))
assert [tiers[t] for t in range(9)] == sorted(tiers.values())
for path in jobs:
    text = (ROOT/path).read_text(encoding='utf-8-sig')
    assert '  icon: {fileID: 0}' in text, path
    assert len(re.findall(r'^  - \{fileID: 11400000, guid:', text, re.M)) == 9, path
    assert sorted(map(int, re.findall(r'^  - rankTier: (\d+)$', text, re.M))) == list(range(9)), path

print(f'PASS: {len(files)} tracked files, {len(runtime)} runtime scripts, {len(guids)} GUIDs, '
      f'{len(jobs)} jobs, {len(ranks)} ranks, {references} resolved references.')
