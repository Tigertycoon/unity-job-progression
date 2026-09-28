using System.Collections.Generic;
using JobProgression.Data;
using NUnit.Framework;
using UnityEngine;

namespace JobProgression.Tests
{
    public class DefinitionAndFormulaTests
    {
        private readonly List<Object> _created = new();

        private T Create<T>() where T : ScriptableObject
        {
            var value = ScriptableObject.CreateInstance<T>();
            _created.Add(value);
            return value;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var value in _created) Object.DestroyImmediate(value);
            _created.Clear();
        }

        [Test]
        public void RankLookupUsesTierInsteadOfListPosition()
        {
            var life = Create<LifeDefinition>();
            var novice = Create<RankDefinition>();
            var adept = Create<RankDefinition>();
            adept.rankTier = 1;
            adept.requiredStars = 5;
            life.ranks = new() { adept, null, novice };
            Assert.That(life.GetRank(0), Is.SameAs(novice));
            Assert.That(life.GetNextRank(0), Is.SameAs(adept));
            Assert.That(life.GetNextRank(1), Is.Null);
            Assert.That(life.CanReachRank(1, 4), Is.False);
            Assert.That(life.CanReachRank(1, 5), Is.True);
        }

        [Test]
        public void BonusesAccumulateOnlyEarnedTiersAndIgnoreEmptyEntries()
        {
            var life = Create<LifeDefinition>();
            life.rankStatBonuses = new()
            {
                null,
                new RankStatBonus { rankTier = 0, STR = 2, VIT = 3 },
                new RankStatBonus { rankTier = 1, STR = 4, FOC = 1 },
                new RankStatBonus { rankTier = 2, STR = 100 },
                new RankStatBonus { rankTier = -1, STR = 100 }
            };
            var bonuses = life.GetAccumulatedBonuses(1);
            Assert.That(bonuses.STR, Is.EqualTo(6));
            Assert.That(bonuses.VIT, Is.EqualTo(3));
            Assert.That(bonuses.FOC, Is.EqualTo(1));
            Assert.That(bonuses.TotalPoints, Is.EqualTo(10));
            Assert.That(life.GetBonusForTier(1).STR, Is.EqualTo(4));
            Assert.That(life.GetAccumulatedBonuses(-1).TotalPoints, Is.Zero);
        }

        [Test]
        public void UnconfiguredDefinitionsHaveSafeQueries()
        {
            var life = Create<LifeDefinition>();
            life.ranks = null;
            life.rankStatBonuses = null;
            Assert.That(life.GetRank(0), Is.Null);
            Assert.That(life.GetNextRank(0), Is.Null);
            Assert.That(life.GetAccumulatedBonuses(2).TotalPoints, Is.Zero);
            Assert.That(life.GetBonusForTier(0), Is.Null);
            Assert.That(life.CanReachRank(0, 100), Is.False);
        }

        [Test]
        public void DerivedStatsIncludeLevelAndEquipmentContributions()
        {
            Assert.That(StatFormulas.CalculateHP(10, 3), Is.EqualTo(95));
            Assert.That(StatFormulas.CalculateSP(7, 3), Is.EqualTo(53));
            Assert.That(StatFormulas.CalculateAttack(5, 4), Is.EqualTo(11));
            Assert.That(StatFormulas.CalculateDefense(5, 4), Is.EqualTo(9));
            Assert.That(StatFormulas.CalculateMagicAttack(5, 4), Is.EqualTo(11));
            Assert.That(StatFormulas.CalculateMagicDefense(5, 4), Is.EqualTo(9));
        }

        [TestCase(-10, 0f, 0f)]
        [TestCase(10, 2f, 5f)]
        [TestCase(1000, 50f, 80f)]
        public void ChanceModifiersStayWithinTheirBounds(int stat, float crit, float craft)
        {
            Assert.That(StatFormulas.CalculateCritRate(stat), Is.EqualTo(crit));
            Assert.That(StatFormulas.CalculateCraftBonus(stat), Is.EqualTo(craft));
        }

        [Test]
        public void DamageHonorsDefenseVarianceCriticalAndMinimum()
        {
            var saved = Random.state;
            try
            {
                Random.InitState(719);
                for (int i = 0; i < 100; i++)
                {
                    Assert.That(StatFormulas.CalculateDamage(100, 20), Is.InRange(85, 94));
                    Assert.That(StatFormulas.CalculateDamage(100, 20, isCritical: true), Is.InRange(128, 141));
                    Assert.That(StatFormulas.CalculateDamage(0, 9999, elementMultiplier: 0), Is.EqualTo(1));
                }
            }
            finally { Random.state = saved; }
        }
    }
}
