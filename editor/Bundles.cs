//     Bundles.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using DateTime = System.DateTime;

namespace Tabletop.io.Gui.Editor {
    public class Bundles {
        const string BundlePath = "../build/bundles";
        const string BundlePathIOS = "../build/bundles/ios";
        const BuildAssetBundleOptions AssetBundleOptions = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;

        static void DoBuildBundles (string relativePath, bool forceBuild, bool isBundlesDir, BuildTarget target) {
            var dir = new DirectoryInfo(relativePath);
            foreach (var sub in dir.GetDirectories()) {
                if (isBundlesDir) {
                    var inputPath = Path.Combine(relativePath, sub.Name);
                    var outputPath = Path.Combine(GetBundlePath(target), sub.Name + ".unity3d");

                    if (File.Exists(outputPath)) {
                        if(!forceBuild && !IsNewerRecursive(new DirectoryInfo(inputPath), File.GetLastWriteTimeUtc(outputPath))) {
                            Debug.Log("Skipping bundle: " + inputPath);
                            continue;
                        } else
                            File.Delete(outputPath);
                    }

                    Debug.Log("Building bundle from: " + inputPath);
                    var objects = from file in sub.GetFiles()
                                   let assetPath = Path.Combine(inputPath, file.Name)
                                   select AssetDatabase.LoadMainAssetAtPath(assetPath);

                    var atlasesPath = Path.Combine(inputPath, "Atlases");
                    if (Directory.Exists(atlasesPath))
                        objects = objects.Union(Atlases.BuildAtlases(atlasesPath));

                    var fontsPath = Path.Combine(inputPath, "Fonts");
                    if (Directory.Exists(fontsPath))
                        objects = objects.Union(Fonts.BuildFonts(fontsPath));

                    BuildPipeline.BuildAssetBundle(null, objects.ToArray(), outputPath, BuildAssetBundleOptions.CollectDependencies, target);

                    if (Directory.Exists(atlasesPath))
                        Atlases.CleanupAssets(atlasesPath);
                } else
                    DoBuildBundles(Path.Combine(relativePath, sub.Name), forceBuild, sub.Name == "Bundles", target);
            }
        }

        static bool IsNewerRecursive (DirectoryInfo dir, DateTime compareTo) {
            if (dir.Name != "Atlases" && dir.LastWriteTimeUtc > compareTo)
                return true;

            foreach (var file in dir.GetFiles()) {
                if (file.LastWriteTimeUtc > compareTo)
                    return true;
            }

            foreach (var sub in dir.GetDirectories()) {
                if (IsNewerRecursive(sub, compareTo))
                    return true;
            }
            return false;
        }

        internal static void DoBuildBundles (string outPath, bool forceBuild, BuildTarget target) {
            var bundlePath = GetBundlePath(target);
            if (!Directory.Exists(bundlePath)) {
                Directory.CreateDirectory(bundlePath);
            }

            DoBuildBundles("Assets", forceBuild, false, target);

            if (outPath != null) {
                if (!Directory.Exists(outPath)) {
                    Directory.CreateDirectory(outPath);
                }
                var dir = new DirectoryInfo(bundlePath);
                foreach (var file in dir.GetFiles("*.unity3d")) {
                    var targetPath = Path.Combine(outPath, file.Name);
                    if (!File.Exists(targetPath) ||
                       file.LastWriteTimeUtc > File.GetLastWriteTimeUtc(targetPath)) {
                           file.CopyTo(targetPath, true);
                    }
                }
            }
        }

        static string GetBundlePath (BuildTarget target) {
            switch (target) {
            case BuildTarget.iPhone:
                return BundlePathIOS;
            default:
                return BundlePath;
            }
        }

        [MenuItem("Tabletop/Build (Asset Bundles)")]
        public static void BuildBundles () {
            Bundles.DoBuildBundles(null, false, BuildTarget.WebPlayer);
        }

        [MenuItem("Tabletop/Rebuild (Asset Bundles)")]
        public static void RebuildBundles () {
            Bundles.DoBuildBundles(null, true, BuildTarget.WebPlayer);
        }
    }
}