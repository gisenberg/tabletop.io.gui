//     IKeyboardInput.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public interface IKeyboardInput {
        void OnGotFocus ();
        void OnLostFocus ();
        void OnKeyPress (char ch);
        void OnKeyPress (KeyCode key);
    }
}
