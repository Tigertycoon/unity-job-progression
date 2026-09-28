using System;
using System.Collections.Generic;
using UnityEngine;
using JobProgression.Data;

namespace JobProgression.Systems
{
    /// <summary>
    /// Central manager for the Life/Job progression system.
    /// Tracks which Life is active, current rank, stars, and Life XP for each Life.
    /// Singleton — one per scene (or persistent via DontDestroyOnLoad).
    /// </summary>
    public class LifeProgressionManager : MonoBehaviour
    {
        public static LifeProgressionManager Instance { get; private set; }

        // ─── Events ───────────────────────────────────────────────────
        /// <summary>Fired when the player switches to a different Life.</summary>
        public event Action<LifeDefinition> OnLifeChanged;

        /// <summary>Fired when the player ranks up in any Life. Args: life, new rank.</summary>
        public event Action<LifeDefinition, RankDefinition> OnRankUp;

        /// <summary>Fired when Life XP is gained. Args: life, amount, new total.</summary>
        public event Action<LifeDefinition, int, int> OnLifeXPGained;

        /// <summary>Fired when stars are gained. Args: life, amount, new total.</summary>
        public event Action<LifeDefinition, int, int> OnStarsGained;

        // ─── Configuration ────────────────────────────────────────────
        [Tooltip("All available Lives in the game")]
        public List<LifeDefinition> availableLives = new();

        [SerializeField]
        private LifeDefinition _activeLife;

        /// <summary>The player's currently active Life/Job.</summary>
        public LifeDefinition ActiveLife => _activeLife;

        // ─── Runtime Data ─────────────────────────────────────────────
        /// <summary>Per-Life progression data stored at runtime.</summary>
        [Serializable]
        public sealed class LifeProgress
        {
            public LifeDefinition life { get; internal set; }
            public int currentRankTier { get; internal set; }
            public int totalStars { get; internal set; }
            public int lifeXP { get; internal set; }

            public RankDefinition CurrentRank => life?.GetRank(currentRankTier);
            public RankDefinition NextRank => life?.GetNextRank(currentRankTier);
        }

        private Dictionary<LifeDefinition, LifeProgress> _progressMap = new();

        // ─── Character Level ──────────────────────────────────────────
        [SerializeField] private int _characterLevel = 1;
        [SerializeField] private int _characterXP;

        public int CharacterLevel => _characterLevel;
        public int CharacterXP => _characterXP;

        [Tooltip("XP required per level = baseXP + (level * xpPerLevel)")]
        [Min(1), SerializeField] private int _baseXPPerLevel = 100;
        [Min(1), SerializeField] private int _xpScalePerLevel = 50;

        public event Action<int> OnCharacterLevelUp;

        // ─── Unity Lifecycle ──────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeProgress();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnValidate()
        {
            _baseXPPerLevel = Math.Max(1, _baseXPPerLevel);
            _xpScalePerLevel = Math.Max(1, _xpScalePerLevel);
            _characterLevel = Math.Max(1, _characterLevel);
        }

        private void InitializeProgress()
        {
            if (availableLives != null)
            {
                foreach (var life in availableLives) EnsureProgress(life);
            }
            // Inspector-assigned active jobs need state even when omitted from the list.
            EnsureProgress(_activeLife);
        }

        private LifeProgress EnsureProgress(LifeDefinition life)
        {
            if (life == null) return null;
            if (!_progressMap.TryGetValue(life, out var progress))
            {
                progress = new LifeProgress { life = life };
                _progressMap.Add(life, progress);
            }
            return progress;
        }

        private static int AddWithoutOverflow(int current, int amount)
        {
            return (int)Math.Min(int.MaxValue, (long)current + amount);
        }

        // ─── Public API ───────────────────────────────────────────────

        /// <summary>Switch the player's active Life.</summary>
        public void SetActiveLife(LifeDefinition life)
        {
            if (life == null) return;
            EnsureProgress(life);
            if (life == _activeLife) return;

            _activeLife = life;
            OnLifeChanged?.Invoke(life);
        }

        /// <summary>Add Life XP to the currently active Life.</summary>
        public void AddLifeXP(int amount)
        {
            if (_activeLife == null || amount <= 0) return;
            AddLifeXP(_activeLife, amount);
        }

        /// <summary>Add Life XP to a specific Life.</summary>
        public void AddLifeXP(LifeDefinition life, int amount)
        {
            if (life == null || amount <= 0) return;
            var progress = EnsureProgress(life);
            int previous = progress.lifeXP;
            progress.lifeXP = AddWithoutOverflow(previous, amount);
            if (progress.lifeXP != previous)
                OnLifeXPGained?.Invoke(life, progress.lifeXP - previous, progress.lifeXP);
        }

        /// <summary>Add stars to a specific Life.</summary>
        public void AddStars(LifeDefinition life, int amount)
        {
            if (life == null || amount <= 0) return;
            var progress = EnsureProgress(life);
            int previous = progress.totalStars;
            progress.totalStars = AddWithoutOverflow(previous, amount);
            if (progress.totalStars != previous)
                OnStarsGained?.Invoke(life, progress.totalStars - previous, progress.totalStars);
        }

        /// <summary>Add stars to the currently active Life.</summary>
        public void AddStars(int amount)
        {
            if (_activeLife == null || amount <= 0) return;
            AddStars(_activeLife, amount);
        }

        /// <summary>
        /// Attempt to rank up the active Life at the Life Master.
        /// Returns true if rank up succeeded.
        /// </summary>
        public bool TryRankUp()
        {
            return _activeLife != null && TryRankUp(_activeLife);
        }

        /// <summary>
        /// Attempt to rank up a specific Life.
        /// Checks if the player has enough stars for the next rank.
        /// </summary>
        public bool TryRankUp(LifeDefinition life)
        {
            if (life == null) return false;
            if (!_progressMap.TryGetValue(life, out var progress)) return false;

            var nextRank = life.GetNextRank(progress.currentRankTier);
            if (nextRank == null)
            {
                return false;
            }

            if (progress.totalStars < nextRank.requiredStars)
            {
                return false;
            }

            progress.currentRankTier = nextRank.rankTier;
            OnRankUp?.Invoke(life, nextRank);
            return true;
        }

        /// <summary>Add character XP and check for level up.</summary>
        public void AddCharacterXP(int amount)
        {
            if (amount <= 0 || _characterLevel == int.MaxValue) return;
            _characterXP = AddWithoutOverflow(_characterXP, amount);

            int xpNeeded = GetXPForNextLevel();
            while (_characterXP >= xpNeeded && _characterLevel < int.MaxValue)
            {
                _characterXP -= xpNeeded;
                _characterLevel++;
                OnCharacterLevelUp?.Invoke(_characterLevel);
                xpNeeded = GetXPForNextLevel();
            }
        }

        public int GetXPForNextLevel()
        {
            // A positive increasing curve also prevents an endless level-up loop
            // if old serialized data contains a zero or negative configuration.
            long threshold = (long)Math.Max(1, _baseXPPerLevel)
                           + (long)Math.Max(1, _characterLevel) * Math.Max(1, _xpScalePerLevel);
            return (int)Math.Min(int.MaxValue, threshold);
        }

        // ─── Queries ──────────────────────────────────────────────────

        public LifeProgress GetProgress(LifeDefinition life)
        {
            return life != null && _progressMap.TryGetValue(life, out var progress) ? progress : null;
        }

        public LifeProgress GetActiveProgress()
        {
            return _activeLife != null ? GetProgress(_activeLife) : null;
        }

        public int GetCurrentRankTier(LifeDefinition life)
        {
            return GetProgress(life)?.currentRankTier ?? 0;
        }

        public int GetTotalStars(LifeDefinition life)
        {
            return GetProgress(life)?.totalStars ?? 0;
        }
    }
}
