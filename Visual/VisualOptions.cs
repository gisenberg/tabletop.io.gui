//     VisualOptions.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class VisualOptions {
        public string Name;
        public string Layer = "gui";
        public string Shader;
        public IControl Parent;
        public bool HPixelAlign = true;
        public bool VPixelAlign = true;
        public Vector2 TileOffset;
        public Color Color = Color.white;
        public RotateFlip RotateFlip;
        public AnchorTo AnchorTo;
        public BillboardType Billboard;
        public float BillboardDepth;

        public bool PixelAlign {
            set {
                this.HPixelAlign = this.VPixelAlign = value;
            }
        }

        public VisualOptions () {
        }
    }
}
