//     Album.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Album<T> : Image, IContainer {
        AlbumOptions _opts;
        Rect _clip;
        ListItem<T>[] _items;
        float _xBase;
        RadioGroup _group;
        int _index;
        Transition _slide;

        public Album (Vector3 position, Vector2 size, T[] items, AlbumOptions opts)
            : base(position, size, opts.Sprite, opts) {
            _clip = new Rect(
                position.x + opts.Border.x,
                position.y + opts.Border.y,
                size.x - opts.Border.x - opts.Border.z,
                size.y - opts.Border.y - opts.Border.w
                );
            _opts = opts;
            this.CreateItems(items, opts.SelectedIndex);
        }

        public Album (Rect bounds, float depth, T[] items, AlbumOptions opts)
            : this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), items, opts) {
        }

        public ListItem<T>[] Items { get { return _items; } }

        public int SelectedIndex {
            get {
                return _index;
            }
            set {
                _index = System.Math.Max(0, System.Math.Min(_items.Length - 1, value));
                float x = _xBase - _index * _opts.ItemSize.x - (_index > 0 ? (_index - 1) * _opts.Spacing : 0f);
                this.ScrollTo(x);
            }
        }

        public T Selected {
            get {
                return _items[_index].Data;
            }
        }

        void CreateItems (T[] data, int selectedIndex) {
            _group = new RadioGroup();
            _items = new ListItem<T>[data.Length];
            var y = Mathf.Round(_clip.yMin + (_clip.height - _opts.ItemSize.y) / 2f);
            _xBase = Mathf.Round(_clip.xMin + (_clip.width - _opts.ItemSize.x) / 2f);
            selectedIndex = System.Math.Max(0, System.Math.Min(data.Length - 1, selectedIndex));
            var x = _xBase - selectedIndex * _opts.ItemSize.x - (selectedIndex > 0 ? (selectedIndex - 1) * _opts.Spacing : 0f);
            for (var i = 0; i < data.Length; i++) {
                var idx = i;
                _items[i] = new ListItem<T>(
                    new Rect(x, y, _opts.ItemSize.x, _opts.ItemSize.y),
                    this, _group, data[i], _opts.ItemSprite, null, null, true);
                _items[i].Clicked += (sender, e) => this.SelectedIndex = idx;
                x += _opts.ItemSize.x + _opts.Spacing;
            }
            _index = selectedIndex;
        }

        void ScrollTo (float x) {
            if (_slide != null)
                _slide.Dispose();
            _slide = Transition.New(1f, _items[0].Position.x, x, Tween.EaseOutQuint, v => this.SetScrollPosition(v)).Now();
        }

        void SetScrollPosition (float x) {
            for (var i = 0; i < _items.Length; i++) {
                var pos = _items[i].Position;
                pos.x = Mathf.Round(x);
                _items[i].Position = pos;
                x += _opts.ItemSize.x + _opts.Spacing;
            }
        }

        Rect IContainer.GetClipRect (IControl child) {
            return _clip;
        }

        void IContainer.OnChildAdded (IControl child) {
        }

        void IContainer.OnChildRemoved (IControl child) {
        }

        public override void Dispose () {
            if (_slide != null)
                _slide.Dispose();
            base.Dispose();
        }
    }

    public class AlbumOptions : VisualOptions {
        public string Sprite;
        public Vector4 Border;
        public string ItemSprite;
        public Vector2 ItemSize;
        public float Spacing;
        public int SelectedIndex;
    }
}
