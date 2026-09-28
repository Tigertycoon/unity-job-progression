using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JobProgression.Data;
using JobProgression.Systems;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace JobProgression.Tests
{
    public class ProgressionTests
    {
        private readonly List<Object> _assets = new();
        private GameObject _host;
        private LifeProgressionManager _manager;
        private LifeDefinition _warrior;
        private LifeDefinition _miner;

        private T Asset<T>() where T : ScriptableObject
        {
            var value = ScriptableObject.CreateInstance<T>();
            _assets.Add(value);
            return value;
        }

        private void SetPrivate(string field, object value) =>
            typeof(LifeProgressionManager).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_manager, value);

        [SetUp]
        public void SetUp()
        {
            Assert.That(LifeProgressionManager.Instance == null, Is.True);
            _host = new GameObject("Progression Test");
            _host.SetActive(false);
            _manager = _host.AddComponent<LifeProgressionManager>();
            _warrior = Asset<LifeDefinition>();
            _warrior.lifeName = "Warrior";
            _miner = Asset<LifeDefinition>();
            _miner.lifeName = "Miner";
            for (int i = 0; i < 3; i++)
            {
                var rank = Asset<RankDefinition>();
                rank.rankTier = i;
                rank.requiredStars = i == 2 ? 15 : i * 5;
                _warrior.ranks.Add(rank);
                _miner.ranks.Add(rank);
            }
            _manager.availableLives.Add(_warrior);
            _manager.availableLives.Add(_miner);
        }

        private IEnumerator StartManager()
        {
            _host.SetActive(true);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_host != null) Object.Destroy(_host);
            foreach (var asset in _assets) Object.Destroy(asset);
            _assets.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator InspectorAssignedJobInitializesWithoutAvailableList()
        {
            _manager.availableLives = null;
            SetPrivate("_activeLife", _warrior);
            yield return StartManager();
            _manager.AddLifeXP(12);
            Assert.That(_manager.GetActiveProgress().lifeXP, Is.EqualTo(12));
            Assert.That(_manager.ActiveLife, Is.SameAs(_warrior));
        }

        [UnityTest]
        public IEnumerator SwitchingJobsPreservesTheirIndependentProgress()
        {
            yield return StartManager();
            int changed = 0;
            _manager.OnLifeChanged += _ => changed++;
            _manager.SetActiveLife(_warrior);
            _manager.AddLifeXP(30);
            _manager.AddStars(5);
            _manager.SetActiveLife(_miner);
            _manager.AddLifeXP(10);
            _manager.SetActiveLife(_warrior);
            _manager.SetActiveLife(_warrior);
            Assert.That(changed, Is.EqualTo(3));
            Assert.That(_manager.GetProgress(_warrior).lifeXP, Is.EqualTo(30));
            Assert.That(_manager.GetProgress(_miner).lifeXP, Is.EqualTo(10));
            Assert.That(_manager.GetTotalStars(_warrior), Is.EqualTo(5));
        }

        [UnityTest]
        public IEnumerator ExplicitRewardsRegisterAnInactiveJob()
        {
            _manager.availableLives.Clear();
            yield return StartManager();
            _manager.AddLifeXP(_miner, 10);
            _manager.AddStars(_miner, 5);
            Assert.That(_manager.ActiveLife, Is.Null);
            Assert.That(_manager.GetProgress(_miner).lifeXP, Is.EqualTo(10));
            Assert.That(_manager.TryRankUp(_miner), Is.True);
        }

        [UnityTest]
        public IEnumerator RankUpRequiresStarsAndAdvancesOneTierWithoutSpendingThem()
        {
            yield return StartManager();
            _manager.SetActiveLife(_warrior);
            var tiers = new List<int>();
            _manager.OnRankUp += (_, rank) => tiers.Add(rank.rankTier);
            _manager.AddLifeXP(999);
            Assert.That(_manager.TryRankUp(), Is.False);
            _manager.AddStars(4);
            Assert.That(_manager.TryRankUp(), Is.False);
            _manager.AddStars(11);
            Assert.That(_manager.TryRankUp(), Is.True);
            Assert.That(_manager.GetCurrentRankTier(_warrior), Is.EqualTo(1));
            Assert.That(_manager.TryRankUp(), Is.True);
            Assert.That(_manager.TryRankUp(), Is.False);
            Assert.That(tiers, Is.EqualTo(new[] { 1, 2 }));
            Assert.That(_manager.GetTotalStars(_warrior), Is.EqualTo(15));
        }

        [UnityTest]
        public IEnumerator CharacterXpCarriesRemainderAcrossMultipleLevels()
        {
            yield return StartManager();
            var levels = new List<int>();
            _manager.OnCharacterLevelUp += level => levels.Add(level);
            _manager.AddCharacterXP(400);
            Assert.That(_manager.CharacterLevel, Is.EqualTo(3));
            Assert.That(_manager.CharacterXP, Is.EqualTo(50));
            Assert.That(_manager.GetXPForNextLevel(), Is.EqualTo(250));
            Assert.That(levels, Is.EqualTo(new[] { 2, 3 }));
        }

        [UnityTest]
        public IEnumerator InvalidRewardsAndNullQueriesDoNotChangeState()
        {
            yield return StartManager();
            _manager.AddLifeXP(5);
            _manager.AddStars(5);
            _manager.AddLifeXP(null, 5);
            _manager.AddStars(null, 5);
            _manager.AddCharacterXP(-1);
            _manager.SetActiveLife(_warrior);
            _manager.AddLifeXP(0);
            _manager.AddStars(-1);
            Assert.That(_manager.GetActiveProgress().lifeXP, Is.Zero);
            Assert.That(_manager.GetTotalStars(_warrior), Is.Zero);
            Assert.That(_manager.GetProgress(null), Is.Null);
            Assert.That(_manager.TryRankUp(null), Is.False);
            Assert.That(_manager.CharacterXP, Is.Zero);
        }

        [UnityTest]
        public IEnumerator CountersSaturateAndEventsReportOnlyActuallyAddedPoints()
        {
            yield return StartManager();
            int starEvents = 0, xpEvents = 0, starDelta = 0, xpDelta = 0;
            _manager.OnStarsGained += (_, delta, __) => { starEvents++; starDelta = delta; };
            _manager.OnLifeXPGained += (_, delta, __) => { xpEvents++; xpDelta = delta; };
            _manager.AddStars(_warrior, int.MaxValue - 2);
            _manager.AddLifeXP(_warrior, int.MaxValue - 2);
            _manager.AddStars(_warrior, 5);
            _manager.AddLifeXP(_warrior, 5);
            _manager.AddStars(_warrior, 5);
            _manager.AddLifeXP(_warrior, 5);
            Assert.That(_manager.GetTotalStars(_warrior), Is.EqualTo(int.MaxValue));
            Assert.That(_manager.GetProgress(_warrior).lifeXP, Is.EqualTo(int.MaxValue));
            Assert.That(starEvents, Is.EqualTo(2));
            Assert.That(xpEvents, Is.EqualTo(2));
            Assert.That(starDelta, Is.EqualTo(2));
            Assert.That(xpDelta, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator InvalidSerializedXpCurveStillTerminatesAndAdvances()
        {
            yield return StartManager();
            SetPrivate("_baseXPPerLevel", 0);
            SetPrivate("_xpScalePerLevel", 0);
            _manager.AddCharacterXP(5);
            Assert.That(_manager.CharacterLevel, Is.EqualTo(3));
            Assert.That(_manager.CharacterXP, Is.Zero);
            Assert.That(_manager.GetXPForNextLevel(), Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator DuplicateManagersLeaveTheOriginalAlive()
        {
            yield return StartManager();
            var duplicate = new GameObject("Duplicate Progression Test");
            duplicate.AddComponent<LifeProgressionManager>();
            yield return null;
            Assert.That(duplicate == null, Is.True);
            Assert.That(LifeProgressionManager.Instance, Is.SameAs(_manager));
        }

        [UnityTest]
        public IEnumerator DestroyingTheManagerReleasesTheSingleton()
        {
            yield return StartManager();
            Object.Destroy(_host);
            yield return null;
            Assert.That(ReferenceEquals(LifeProgressionManager.Instance, null), Is.True);
            _host = new GameObject("Replacement Progression Test");
            var replacement = _host.AddComponent<LifeProgressionManager>();
            Assert.That(LifeProgressionManager.Instance, Is.SameAs(replacement));
        }

        [UnityTest]
        public IEnumerator SceneUnloadKeepsTheManagerAndProgress()
        {
            var temporary = SceneManager.CreateScene("ProgressionSceneTest");
            SceneManager.MoveGameObjectToScene(_host, temporary);
            yield return StartManager();
            _manager.SetActiveLife(_miner);
            _manager.AddLifeXP(17);
            yield return SceneManager.UnloadSceneAsync(temporary);
            Assert.That(LifeProgressionManager.Instance, Is.SameAs(_manager));
            Assert.That(_manager.GetActiveProgress().lifeXP, Is.EqualTo(17));
        }
    }
}
