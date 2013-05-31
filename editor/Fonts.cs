using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tabletop.io.Gui.Editor {
    public class Fonts {
        const int TextureSize = 2048;

        public static IEnumerable<Object> BuildFonts (string path) {
            var dir = new DirectoryInfo(path);

            Debug.Log("Building fonts from: " + path);

            foreach(var asset in dir.GetFiles("*.txt")
                .Select(f => AssetDatabase.LoadMainAssetAtPath(Path.Combine(path, f.Name)))) {
                yield return asset;
            }

            foreach (var asset in dir.GetFiles("*.png")
                .Select(f => AssetDatabase.LoadMainAssetAtPath(Path.Combine(path, f.Name)))) {
                yield return asset;
            }
        }
    }
}
