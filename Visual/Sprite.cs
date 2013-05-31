//     Sprite.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Sprite {
        public Atlas Atlas { get; private set; }
        public Rect UV { get; private set; }
        public Vector4 Border { get; private set; }
        public bool IsHollow { get; private set; }
        public bool TileHorizontal { get; private set; }
        public bool TileVertical { get; private set; }
        public float TexelWidth { get { return this.Atlas.TexelWidth; } }
        public float TexelHeight { get { return this.Atlas.TexelHeight; } }

        public Vector2 Size {
            get {
                return new Vector2(UV.width / this.TexelWidth, UV.height / this.TexelHeight);
            }
        }

        public Material GetMaterial (string shaderName) {
            return this.Atlas.GetMaterial(shaderName);
        }

        public Material GetMaterial () {
            return this.GetMaterial(null);
        }

        public Sprite (Atlas atlas, Rect uv, Vector4 border, bool isHollow, bool hTile, bool vTile) {
            this.Atlas = atlas;
            this.UV = uv;
            this.Border = border;
            this.IsHollow = isHollow;
            this.TileHorizontal = hTile;
            this.TileVertical = vTile;
        }
    }
}
