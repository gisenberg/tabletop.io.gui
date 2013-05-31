//     BitmapChar.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.Collections.Generic;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class BitmapChar {
        Dictionary<int, float> _kerning;

        public int Id { get; private set; }
        public Rect UV { get; private set; }
        public Vector2 Size { get; private set; }
        public Vector2 Offset { get; private set; }
        public float Advance { get; private set; }
        public int Page { get; private set; }

        internal BitmapChar (int id, Rect uv, Vector2 size, Vector2 offset, float advance, int page) {
            _kerning = new Dictionary<int, float>();
            this.Id = id;
            this.UV = uv;
            this.Size = size;
            this.Offset = offset;
            this.Advance = advance;
            this.Page = page;
        }

        internal void SetKerning(int previous, float amount) {
            _kerning[previous] = amount;
        }

        public float GetKerning (int previous) {
            float amount;
            _kerning.TryGetValue(previous, out amount);
            return amount;
        }
    }
}
