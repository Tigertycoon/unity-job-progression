using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JobProgression.Data;
using NUnit.Framework;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEngine;

namespace JobProgression.Tests
{
    public class SampleImportTests
    {
        private string _temporaryFolder;

        [Test]
        public void ExampleJobsImportWithResolvedRanksAndScriptTypes()
        {
            var package = PackageInfo.FindForAssembly(typeof(LifeDefinition).Assembly);
            Assert.That(package, Is.Not.Null);
            string source = Path.Combine(package.resolvedPath, "Samples~/Example Jobs");
            _temporaryFolder = "Assets/JobProgressionImportTest_" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(_temporaryFolder);

            // Remap only sample-asset GUIDs so an existing user import is never moved
            // or modified by Unity's duplicate-GUID handling. Script GUIDs stay intact.
            var guids = new Dictionary<string, string>();
            foreach (var meta in Directory.GetFiles(source, "*.asset.meta", SearchOption.AllDirectories))
            {
                string guid = File.ReadLines(meta).First(l => l.StartsWith("guid: ")).Substring(6);
                guids.Add(guid, Guid.NewGuid().ToString("N"));
            }
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                if (!file.EndsWith(".asset") && !file.EndsWith(".asset.meta")) continue;
                string destination = Path.Combine(_temporaryFolder, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                string text = File.ReadAllText(file);
                foreach (var pair in guids) text = text.Replace(pair.Key, pair.Value);
                File.WriteAllText(destination, text);
            }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var jobs = AssetDatabase.FindAssets("t:LifeDefinition", new[] { _temporaryFolder });
            Assert.That(jobs.Length, Is.EqualTo(12));
            Assert.That(AssetDatabase.FindAssets("t:RankDefinition", new[] { _temporaryFolder }).Length, Is.EqualTo(9));
            foreach (var guid in jobs)
            {
                var job = AssetDatabase.LoadAssetAtPath<LifeDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.That(job.ranks.Count, Is.EqualTo(9), job.name);
                for (int tier = 0; tier < 9; tier++)
                {
                    Assert.That(job.GetRank(tier), Is.Not.Null, job.name + " tier " + tier);
                    Assert.That(job.GetBonusForTier(tier), Is.Not.Null, job.name + " bonus " + tier);
                }
                Assert.That(job.icon, Is.Null, "Samples must not depend on excluded art.");
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_temporaryFolder))
                AssetDatabase.DeleteAsset(_temporaryFolder);
        }
    }
}
