//     Build.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.IO;
using System.Linq;
using UnityEditor;

namespace Tabletop.io.Gui.Editor {
    public class Build {
        const string BuildPath = "../build";
        const string ExeName = "client";

        static void DoBuild (BuildTarget target, BuildOptions opts) {
            var scenes = (from e in EditorBuildSettings.scenes
                          where e != null && e.enabled
                          select e.path).ToArray();

            var path = Path.Combine(BuildPath, target.ToString());
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            var bundlePath = path;

            switch (target) {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                path = Path.Combine(path, ExeName + ".exe");
                bundlePath = Path.Combine(bundlePath, ExeName + "_Data");
                break;
            case BuildTarget.StandaloneOSXIntel:
                path = Path.Combine(path, ExeName + ".app");
                bundlePath = Path.Combine(path, "Contents");
                break;
            case BuildTarget.iPhone:
                bundlePath = Path.Combine(path, "Data");
                break;
            }

            BuildPipeline.BuildPlayer(scenes, path, target, opts);
            Bundles.DoBuildBundles(bundlePath, false, target);
        }

        [MenuItem("Tabletop/Build/Web Player")]
        public static void BuildWebPlayer () {
            Build.DoBuild(BuildTarget.WebPlayer, BuildOptions.None);
        }

        [MenuItem("Tabletop/Build/Standalone Windows")]
        public static void BuildStandaloneWindows () {
            Build.DoBuild(BuildTarget.StandaloneWindows, BuildOptions.None);
        }

        [MenuItem("Tabletop/Build/Standalone OSX")]
        public static void BuildStandaloneOSX () {
            Build.DoBuild(BuildTarget.StandaloneOSXIntel, BuildOptions.None);
        }

        [MenuItem("Tabletop/Build/iOS/Standard")]
        public static void BuildiOS () {
            Build.DoBuild(BuildTarget.iPhone, BuildOptions.None);
        }

        [MenuItem("Tabletop/Build/iOS/Profiler")]
        public static void BuildiOSProfiler () {
            Build.DoBuild(BuildTarget.iPhone, BuildOptions.Development | BuildOptions.ConnectWithProfiler);
        }
    }
}