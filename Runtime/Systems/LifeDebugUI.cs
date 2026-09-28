using UnityEngine;
using JobProgression.Data;

namespace JobProgression.Systems
{
    /// <summary>
    /// Simple runtime debug overlay that shows the Life progression state.
    /// Uses OnGUI for zero-dependency prototyping. Replace with proper UI later.
    /// </summary>
    public class LifeDebugUI : MonoBehaviour
    {
        [Tooltip("Show the debug overlay")]
        public bool showDebugUI = true;

        private void OnGUI()
        {
            if (!showDebugUI) return;

            var mgr = LifeProgressionManager.Instance;
            if (mgr == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 320, 400));
            GUILayout.BeginVertical("box");

            // Character Level
            GUILayout.Label($"<b>Character Level {mgr.CharacterLevel}</b>  " +
                          $"XP: {mgr.CharacterXP}/{mgr.GetXPForNextLevel()}");

            GUILayout.Space(5);

            // Active Life
            var activeLife = mgr.ActiveLife;
            if (activeLife != null)
            {
                var progress = mgr.GetActiveProgress();
                var currentRank = progress?.CurrentRank;
                var nextRank = progress?.NextRank;

                GUILayout.Label($"<b>Active Life: {activeLife.lifeName}</b> ({activeLife.lifeType})");
                GUILayout.Label($"  Rank: {currentRank?.rankName ?? "None"} (Tier {progress?.currentRankTier ?? 0})");
                GUILayout.Label($"  Stars: {progress?.totalStars ?? 0}" +
                              (nextRank != null ? $" / {nextRank.requiredStars} for {nextRank.rankName}" : " (MAX)"));
                GUILayout.Label($"  Life XP: {progress?.lifeXP ?? 0}");
            }
            else
            {
                GUILayout.Label("<i>No active Life — visit a Life Master!</i>");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
