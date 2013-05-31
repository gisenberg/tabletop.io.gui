//     IMouseInput.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public interface IMouseInput {
        void OnMouseEnter (IControl src);
        void OnMouseExit (IControl src);
        void OnMouseDown (IControl src, Vector3 point);
        void OnMouseUp (IControl src);
        void OnMouseDrag (IControl src, Vector2 delta);
        void OnMouseWheel (IControl src, float delta);
    }
}
