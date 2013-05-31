//     Atlases.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Tabletop.io.Gui.Editor {
    public class Atlases {
        const int TextureSize = 4096;

        public static IEnumerable<Object> BuildAtlases (string path) {
            var dir = new DirectoryInfo(path);
            foreach (var sub in dir.GetDirectories()) {
                var inputPath = Path.Combine(path, sub.Name);
                var textures = (from file in sub.GetFiles()
                                where file.Extension.Equals(".png", System.StringComparison.OrdinalIgnoreCase)
                                || file.Extension.Equals(".jpg", System.StringComparison.OrdinalIgnoreCase)
                                let assetPath = Path.Combine(inputPath, file.Name)
                                select AssetDatabase.LoadMainAssetAtPath(assetPath))
                               .OfType<Texture2D>().ToArray();

                // read atlas configuration
                bool isCompressed = false;
                bool useMipMap = false;
                FilterMode filterMode = FilterMode.Trilinear;
                var configPath = Path.Combine(sub.FullName, "atlas.txt");
                if (File.Exists(configPath)) {
                    var parts = File.ReadAllText(configPath).Split(' ');
                    if (parts.Length > 0)
                        bool.TryParse(parts[0].Trim(), out isCompressed);
                    if (parts.Length > 1)
                        bool.TryParse(parts[1].Trim(), out useMipMap);
                    if (parts.Length > 2) {
                        try {
                            filterMode = (FilterMode)System.Enum.Parse(typeof(FilterMode), parts[2].Trim());
                        }
                        catch {
                            Debug.Log("Invalid FilterMode of " + parts[2]);
                        }
                    }
                }
                Debug.Log(string.Format("Building atlas {0}, compressed={1}, mipmap={2}, filter={3}", sub.Name, isCompressed, useMipMap, filterMode));

                // verify importer settings
                TextureImporter importer;
                for (var i = 0; i < textures.Length; i++) {
                    var importPath = AssetDatabase.GetAssetPath(textures[i]);
                    importer = (TextureImporter)TextureImporter.GetAtPath(importPath);
                    if (!importer.isReadable || importer.npotScale != TextureImporterNPOTScale.None ||
                        importer.textureFormat != TextureImporterFormat.ARGB32) {
                        importer.isReadable = true;
                        importer.npotScale = TextureImporterNPOTScale.None;
                        importer.textureFormat = TextureImporterFormat.ARGB32;
                        AssetDatabase.ImportAsset(importPath, ImportAssetOptions.ForceSynchronousImport);
                    }
                }

                // build the actual atlas
                var tex = new Texture2D(TextureSize, TextureSize);
                var rects = tex.PackTextures(textures, 0, TextureSize);
                for (var i = 0; i < textures.Length; i++) {
                    var w = rects[i].width * tex.width;
                    var h = rects[i].height * tex.height;
                    if (w < textures[i].width || h < textures[i].height) {
                        Debug.Log(string.Format("WARNING: Atlas {0} has resized components, try splitting it up.", sub.Name));
                        break;
                    }
                }

                // if necessary, make atlas square
                if (isCompressed && tex.width != tex.height) {
                    int size = Mathf.Max(tex.width, tex.height);
                    var temp = (Texture2D)Object.Instantiate(tex);
                    temp.Resize(size, size, TextureFormat.ARGB32, false);
                    temp.SetPixels(0, 0, tex.width, tex.height, tex.GetPixels(0), 0);
                    temp.Apply(false);
                    for (var j = 0; j < rects.Length; ++j) {
                        if (tex.width > tex.height) {
                            rects[j].y *= 0.5f;
                            rects[j].yMax = rects[j].y + (rects[j].height * 0.5f);
                        } else {
                            rects[j].x *= 0.5f;
                            rects[j].xMax = rects[j].x + (rects[j].width * 0.5f);
                        }
                    }
                    if (tex != temp) {
                        temp.name = tex.name;
                        Object.DestroyImmediate(tex);
                    }
                    tex = temp;
                    Resources.UnloadUnusedAssets();
                }
                Debug.Log(string.Format("Atlas {0} is {1}x{2}", sub.Name, tex.width, tex.height));
                var texelWidth = 1f / (float)tex.width;
                var texelHeight = 1f / (float)tex.height;

                // build and save the atlas map
                var map = new StringBuilder();
                map.Append("atlas\n");
                for (var i = 0; i < rects.Length; i++) {
                    map.Append(textures[i].name).Append('\t')
                        .Append(rects[i].xMin).Append('\t').Append(rects[i].yMin).Append('\t')
                        .Append(rects[i].xMax).Append('\t').Append(rects[i].yMax);
                    var spriteData = AssetDatabase.LoadMainAssetAtPath(Path.Combine(inputPath, Path.ChangeExtension(textures[i].name, ".txt"))) as TextAsset;
                    if (spriteData != null) {
                        var lines = spriteData.text.Split('\n');
                        for (var j = 0; j < lines.Length; j++) {
                            var data = lines[j].Trim();
                            if (string.IsNullOrEmpty(data))
                                continue;
                            var parts = data.Split(' ');
                            var border = Vector4.zero;
                            var b = (j == 0 ? 0 : 5);
                            if (parts.Length >= b + 4) {
                                float.TryParse(parts[b], out border.x);
                                float.TryParse(parts[b + 1], out border.w);
                                float.TryParse(parts[b + 2], out border.z);
                                float.TryParse(parts[b + 3], out border.y);
                            }

                            bool isHollow = false;
                            if (parts.Length >= b + 5) {
                                bool.TryParse(parts[b + 4], out isHollow);
                            }

                            bool htile = false, vtile = false;
                            if (parts.Length >= 7) {
                                bool.TryParse(parts[b + 5], out htile);
                                bool.TryParse(parts[b + 6], out vtile);
                            }

                            Vector4 coords;
                            if (j > 0 &&
                                float.TryParse(parts[1], out coords.x) &&
                                float.TryParse(parts[2], out coords.y) &&
                                float.TryParse(parts[3], out coords.z) &&
                                float.TryParse(parts[4], out coords.w)) {
                                coords.x = rects[i].xMin + coords.x * texelWidth;
                                coords.y = rects[i].yMin + coords.y * texelHeight;
                                coords.z = coords.x + coords.z * texelWidth;
                                coords.w = coords.y + coords.w * texelHeight;
                                map.Append(string.Format("{0}/{1}", textures[i].name, parts[0])).Append('\t')
                                    .Append(coords.x).Append('\t').Append(coords.y).Append('\t')
                                    .Append(coords.z).Append('\t').Append(coords.w);
                            }

                            map.Append('\t').Append(border.x).Append('\t').Append(border.y).Append('\t').Append(border.z).Append('\t').Append(border.w)
                                .Append('\t').Append(isHollow.ToString().ToLower())
                                .Append('\t').Append(htile.ToString().ToLower()).Append('\t').Append(vtile.ToString().ToLower()).Append("\n");
                        }
                    }
                    else
                        map.Append("\n");
                }
                string mapPath = Path.Combine(path, string.Format("{0}.txt", sub.Name));
                string texPath = Path.Combine(path, string.Format("{0}.png", sub.Name));
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                File.WriteAllText(mapPath, map.ToString());
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport);
                importer = (TextureImporter)TextureImporter.GetAtPath(texPath);
                importer.textureFormat = isCompressed ? TextureImporterFormat.AutomaticCompressed : TextureImporterFormat.AutomaticTruecolor;
                importer.mipmapEnabled = useMipMap;
                importer.maxTextureSize = System.Math.Max(tex.width, tex.height);
                importer.filterMode = filterMode;
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(mapPath, ImportAssetOptions.ForceSynchronousImport);
                yield return AssetDatabase.LoadMainAssetAtPath(texPath);
                yield return AssetDatabase.LoadMainAssetAtPath(mapPath);
            }
        }

        public static void CleanupAssets (string path) {
            var dir = new DirectoryInfo(path);
            foreach (var name in dir.GetDirectories().Select(d => d.Name)) {
                AssetDatabase.DeleteAsset(Path.Combine(path, string.Format("{0}.txt", name)));
                AssetDatabase.DeleteAsset(Path.Combine(path, string.Format("{0}.png", name)));
            }
        }
    }
}