//     IControl.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using UnityEngine;

namespace Tabletop.io.Gui {
    public interface IControl : IDisposable {
        Vector3 Position { get; set; }
		Vector3 Center { get; set; }
        Vector2 Size { get; set; }
        Color Color { get; set; }
        Visual Visual { get; }
        Rect Bounds { get; }
        void SetVisible (bool isVisible);
        void SetActive (bool isActive);
    }
}
