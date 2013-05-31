//     BitmapFont.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class BitmapFont {
        int _lineHeight, _base, _outline;

        Material[] _pages;
        Dictionary<int, BitmapChar> _chars;

        internal BitmapFont (TextAsset map, IDictionary<string, Texture2D> textures) {
            _chars = new Dictionary<int, BitmapChar>();
            this.Parse(map.text, textures);
        }

        public float LineHeight { get { return _lineHeight; } }
        public float Base { get { return _base; } }
        public float Outline { get { return _outline; } }

        public Material GetMaterial(int id) {
            if (id < 0 || id >= _pages.Length)
                throw new ArgumentException(string.Format("Invalid font page {0}.", id));
            return _pages[id];
        }

        public BitmapChar GetCharacter(int id) {
            BitmapChar ch;
            if (!_chars.TryGetValue(id, out ch) && !_chars.TryGetValue((int)'?', out ch))
                throw new ArgumentException(string.Format("Invalid character {0}.", id));
            return ch;
        }

        void Parse(string spec, IDictionary<string, Texture2D> textures) {
            float texelWidth = -1f, texelHeight = -1f;

            foreach (var line in spec.Split('\n')) {
                var parts = line.Trim().Split(' ');
                var args = from arg in parts.Skip(1)
                           let argParts = arg.Split('=')
                           where argParts.Length == 2
                           select new KeyValuePair<string, string>(argParts[0], argParts[1]);
                int id;
                switch (parts[0]) {
                case "info":
                    foreach (var kvp in args) {
                        switch (kvp.Key) {
                        case "outline":
                            int.TryParse(kvp.Value, out _outline);
                            break;
                        }
                    }
                    break;
                case "common":
                    foreach (var kvp in args) {
                        switch (kvp.Key) {
                        case "lineHeight":
                            int.TryParse(kvp.Value, out _lineHeight);
                            break;
                        case "base":
                            int.TryParse(kvp.Value, out _base);
                            break;
                        case "scaleW":
                            int texWidth;
                            int.TryParse(kvp.Value, out texWidth);
                            texelWidth = 1f / (float)texWidth;
                            break;
                        case "scaleH":
                            int texHeight;
                            int.TryParse(kvp.Value, out texHeight);
                            texelHeight = 1f / (float)texHeight;
                            break;
                        case "pages":
                            int pages;
                            if (int.TryParse(kvp.Value, out pages))
                                _pages = new Material[pages];
                            break;
                        }
                    }
                    break;
                case "page":
                    id = -1;
                    string pageName = null;
                    foreach (var kvp in args) {
                        switch (kvp.Key) {
                        case "id":
                            int.TryParse(kvp.Value, out id);
                            break;
                        case "file":
                            pageName = System.IO.Path.GetFileNameWithoutExtension(ParseString(kvp.Value));
                            break;
                        }
                    }
                    if(_pages == null || id < 0 || id >= _pages.Length || pageName == null)
                        throw new ArgumentException("Invalid font specification.");
                    Texture2D tex;
                    if (!textures.TryGetValue(pageName, out tex))
                        throw new ArgumentException(string.Format("Could not locate font page {0}.", pageName));
                    var mat = BuiltinShader.CreateSpriteVertexColored();
                    mat.mainTexture = tex;
                    _pages[id] = mat;
                    break;
                case "char":
                    id = -1;
                    int x = -1, y = -1, w = -1, h = -1, xoffset = int.MinValue, yoffset = int.MinValue, xadvance = -1, page = -1;
                    foreach (var kvp in args) {
                        switch (kvp.Key) {
                        case "id":
                            int.TryParse(kvp.Value, out id);
                            break;
                        case "x":
                            int.TryParse(kvp.Value, out x);
                            break;
                        case "y":
                            int.TryParse(kvp.Value, out y);
                            break;
                        case "width":
                            int.TryParse(kvp.Value, out w);
                            break;
                        case "height":
                            int.TryParse(kvp.Value, out h);
                            break;
                        case "xoffset":
                            int.TryParse(kvp.Value, out xoffset);
                            break;
                        case "yoffset":
                            int.TryParse(kvp.Value, out yoffset);
                            break;
                        case "xadvance":
                            int.TryParse(kvp.Value, out xadvance);
                            break;
                        case "page":
                            int.TryParse(kvp.Value, out page);
                            break;
                        }
                    }

                    if (x < 0 || y < 0 || w < 0 || h < 0 || xoffset == int.MinValue || yoffset == int.MinValue || xadvance < 0 || page < 0 || page >= _pages.Length)
                        throw new ArgumentException("Invalid char specification.");

                    _chars.Add(id, new BitmapChar(
                        id,
                        new Rect(x * texelWidth, 1f - ((y+h) * texelHeight), w * texelWidth, h * texelHeight),
                        new Vector2(w, h),
                        new Vector2(xoffset, (_lineHeight - h) - yoffset),
                        xadvance,
                        page));
                    break;
                case "kerning":
                    int first = -1, second = -1, amount = int.MinValue;
                    foreach (var kvp in args) {
                        switch (kvp.Key) {
                        case "first":
                            int.TryParse(kvp.Value, out first);
                            break;
                        case "second":
                            int.TryParse(kvp.Value, out second);
                            break;
                        case "amount":
                            int.TryParse(kvp.Value, out amount);
                            break;
                        }
                    }
                    if (first < 0 || second < 0 || amount == int.MinValue || !_chars.ContainsKey(first) || !_chars.ContainsKey(second))
                        throw new ArgumentException("Invalid kerning specification.");
                    _chars[second].SetKerning(first, amount);
                    break;
                }
            }
        }

        string ParseString(string source) {
            if(source[0] == '"' && source[source.Length-1] == '"')
                return source.Substring(1, source.Length-2);
            else
                return source;
        }
    }
}
