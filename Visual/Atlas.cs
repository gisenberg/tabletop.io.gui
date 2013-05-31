//     Atlas.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class Atlas {
        Texture2D _tex;
        Dictionary<string, Sprite> _sprites;
        Dictionary<string, Material> _materials;
        Material _defaultMaterial;

        public float TexelWidth { get; private set; }
        public float TexelHeight { get; private set; }

        internal Atlas (TextAsset map, IDictionary<string, Texture2D> textures) {
            textures.TryGetValue(map.name, out _tex);
            if (_tex == null)
                throw new ArgumentException("Could not find atlas texture.");

            this.TexelWidth = 1f / (float)_tex.width;
            this.TexelHeight = 1f / (float)_tex.height;

            _sprites = new Dictionary<string, Sprite>();
            foreach (var line in map.text.Split('\n')) {
                var parts = line.Trim().Split('\t');

                var uv = Vector4.zero;
                var border = Vector4.zero;
                bool isHollow = false, htile = false, vtile = false;

                if (parts.Length < 5 ||
                    !float.TryParse(parts[1], out uv.x) ||
                    !float.TryParse(parts[2], out uv.y) ||
                    !float.TryParse(parts[3], out uv.z) ||
                    !float.TryParse(parts[4], out uv.w)
                    )
                    continue;
                if (parts.Length >= 12 && (
                    !float.TryParse(parts[5], out border.x) ||
                    !float.TryParse(parts[6], out border.y) ||
                    !float.TryParse(parts[7], out border.z) ||
                    !float.TryParse(parts[8], out border.w) ||
                    !bool.TryParse(parts[9], out isHollow) ||
                    !bool.TryParse(parts[10], out htile) ||
                    !bool.TryParse(parts[11], out vtile)
                    ))
                    continue;

                _sprites.Add(parts[0], new Sprite(this, Rect.MinMaxRect(uv.x, uv.y, uv.z, uv.w), border, isHollow, htile, vtile));
                _defaultMaterial = BuiltinShader.CreateSpriteVertexColored();
                _defaultMaterial.mainTexture = _tex;
            }
        }

        public Sprite GetSprite (string spriteName) {
            Sprite sprite;
            if (!_sprites.TryGetValue(spriteName, out sprite))
                throw new ArgumentException(string.Format("No sprite named {0} found in atlas.", spriteName));
            return sprite;
        }

        public Material GetMaterial (string shaderName) {
            if (shaderName == null)
                return _defaultMaterial;

            Material mat;
            if (_materials == null)
                _materials = new Dictionary<string, Material>();
            if (!_materials.TryGetValue(shaderName, out mat)) {
                mat = new Material(Gui.Instance.GetShader(shaderName));
                mat.mainTexture = _tex;
                _materials.Add(shaderName, mat);
            }
            return mat;
        }
    }
}
