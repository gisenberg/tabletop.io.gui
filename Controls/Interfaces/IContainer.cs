//     IContainer.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public interface IContainer {
        Rect GetClipRect (IControl child);
        void OnChildAdded (IControl child);
        void OnChildRemoved (IControl child);
    }
}
