//     Layer.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Layer {
        Camera _cam;

        public string Name { get; private set; }
        public int Index { get; private set; }
        public GuiManager Manager { get; private set; }

        public Camera Camera
        {
            get {
                return this.Name == "main" ? Camera.main : _cam;
            }
            internal set {
                _cam = value;
            }
        }

        internal Layer (GuiManager manager, string name, int index, Camera camera) {
            this.Manager = manager;
            this.Name = name;
            this.Index = index;
            _cam = camera;
        }
    }
}
