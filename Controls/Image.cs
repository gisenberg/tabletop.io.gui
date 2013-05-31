//     Image.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Image : Control<SpriteVisual>, ISpriteContent {
        string _sprite;
        Transition<float> _tran;

        public Image (Vector3 position, Vector2 size, string sprite, VisualOptions opts)
            : base(position, size, opts) {
            _sprite = sprite;
        }

        public Image (Rect bounds, float depth, string sprite, VisualOptions opts) :
            this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), sprite, opts) {
        }

        public string Sprite {
            get {
                return _sprite;
            }
            set {
                if (_sprite != value) {
                    _sprite = value;
                    this.Invalidate();
                }
            }
        }

        public Transition<float> Animate (float duration, string[] sprites) {
            if (_tran != null)
                _tran.Dispose();

            _tran = Transition<float>.New(duration, 0f, 1f, Tween.Lerp, v => this.OnAnimate(sprites, v));
            return _tran;
        }

        void OnAnimate (string[] sprites, float val) {
            var idx = System.Math.Min(sprites.Length - 1, (int)((float)sprites.Length * val));
            this.Sprite = sprites[idx];
        }
    }
}
