//     Cursor.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Cursor {
        public string Sprite { get; private set; }
        public Vector2 Offset { get; private set; }

        public Cursor (string sprite, Vector2 offset) {
            this.Sprite = sprite;
            this.Offset = offset;
        }
    }

    public enum CursorPriority {
        Override = 0,
        Context = 1,
        Default = 2
    }
}
