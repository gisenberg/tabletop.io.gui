//     Control.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class Control<T> : IControl, IDisposable where T : Visual {
        protected Control (Vector3 position, Vector2 size, VisualOptions opts) {
            var go = new GameObject();
            var visual = go.AddComponent<T>();
            this.Visual = visual;
            visual.Attach(this, position, size, opts);
        }

        public T Visual { get; private set; }
        public Vector3 Position { get { return this.Visual.Position; } set { this.Visual.Position = value; } }
        public Vector2 Size { get { return this.Visual.Size; } set { this.Visual.Size = value; } }
        public Color Color { get { return this.Visual.Color; } set { this.Visual.Color = value; } }
        public Rect Bounds { get { return this.Visual.Bounds; } }
        public string Name { get { return this.Visual.Name; } }
        public object Data { get; set; }
        Visual IControl.Visual { get { return this.Visual; } }

        public Vector3 Center {
            get {
                var pos = this.Position;
                var size = this.Size;
                return new Vector3(pos.x + size.x / 2f, pos.y + size.y / 2f, pos.z);
            }
            set {
                var size = this.Size;
                this.Visual.Position = new Vector3(value.x - size.x / 2f, value.y - size.y / 2f, value.z);
            }
        }

        public void SetVisible (bool isVisible) {
            this.Visual.SetVisible(isVisible);
        }

        public void SetActive (bool isActive) {
            this.Visual.SetActive(isActive);
        }

        public void ClearChildren () {
            foreach (var child in this.Visual.GetDescendants().OfType<IDisposable>())
                child.Dispose();
        }

        public IEnumerable<IControl> GetDescendants () {
            return this.Visual.GetComponentsInChildren<Visual>(true).Where(v => v != this.Visual).Select(v => v.Control);
        }

        public IEnumerable<IControl> GetChildren () {
            for (var i = 0; i < this.Visual.transform.childCount; i++) {
                var child = this.Visual.transform.GetChild(i).GetComponent<Visual>();
                if (child != null)
                    yield return child.Control;
            }
        }

        public TControl FindChild<TControl> (string name) where TControl : IControl {
            return this.GetDescendants().OfType<TControl>().FirstOrDefault(c => c.Visual.Name == name);
        }

        public virtual void Dispose () {
            if (this.Visual != null)
                this.Visual.Dispose();
        }

        protected void Invalidate () {
            if(this.Visual != null)
                this.Visual.Invalidate();
        }
    }
}
