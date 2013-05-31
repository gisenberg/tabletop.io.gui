//     Extensions.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public static class Extensions {
        public static Rect Shift (this Rect from, Vector2 offset) {
            return new Rect(from.x + offset.x, from.y + offset.y, from.width, from.height);
        }

        public static Rect ScreenTopLeft (this Rect from) {
            return new Rect(from.x, Gui.ScreenY - from.yMax, from.width, from.height);
        }

        public static Rect ScreenTopLeft (this Rect from, Vector2 screenSize) {
            var offset = new Vector2(Mathf.Round((Gui.ScreenX - screenSize.x) / 2f), Mathf.Round((Gui.ScreenY - screenSize.y) / 2f));
            return ScreenTopLeft(new Rect(from.x + offset.x, from.y + offset.y, from.width, from.height));
        }

        public static Vector3 ScreenTopLeft (this Vector3 from) {
            return new Vector3(from.x, Gui.ScreenY - from.y, from.z);
        }

        public static Vector3 ScreenTopLeft (this Vector3 from, Vector2 screenSize) {
            var offset = new Vector2(Mathf.Round((Gui.ScreenX - screenSize.x) / 2f), Mathf.Round((Gui.ScreenY - screenSize.y) / 2f));
            return ScreenTopLeft(new Vector3(from.x + offset.x, from.y + offset.y, from.z));
        }

        public static Rect FromTopLeft (this Rect from, IControl parent) {
            var pPos = parent.Position;
            var pSize = parent.Size;
            return new Rect(from.xMin + pPos.x, (pPos.y + pSize.y) - from.yMax, from.width, from.height);
        }
    }
}
