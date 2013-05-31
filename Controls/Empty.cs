//     Empty.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Empty : Control<NullVisual> {
        public Empty ()
            : base(Vector3.zero, Vector2.zero, new VisualOptions {}) {
        }

        public Empty (Vector3 position, VisualOptions opts)
            : base(position, Vector3.zero, opts) {
        }

        public Empty (Vector3 position, Vector2 size, VisualOptions opts)
            : base(position, size, opts) {
        }

        public Empty (Rect bounds, float depth, VisualOptions opts)
            : this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), opts) {
        }
    }
}
