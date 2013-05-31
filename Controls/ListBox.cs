//     ListBox.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class ListBox<T> : ScrollView {
        ListBoxOptions _opts;
        List<ListItem<T>> _items;
        IList<ListItem<T>> _readItems;
        RadioGroup _group;

        public ListBox (Vector3 position, Vector2 size, ListBoxOptions opts) :
            base(position, size, opts) {
            _items = new List<ListItem<T>>();
            _readItems = _items.AsReadOnly();
            _opts = opts;
            _group = new RadioGroup();
            _group.Changed += (sender, e) => {
                if (this.SelectionChanged != null)
                    this.SelectionChanged(this, e);
            };
        }

        public ListBox (Rect bounds, float depth, ListBoxOptions opts) :
            this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), opts) {
        }

        public IList<ListItem<T>> Items { get { return _readItems; } }

        public ListItem<T> SelectedItem {
            get {
                return (ListItem<T>)_group.Active;
            }
            set {
                if (value == null)
                    _group.Clear();
                else
                    value.IsActive = true;
            }
        }

        public int SelectedIndex {
            get {
                return _items.Contains(this.SelectedItem) ? _items.IndexOf(this.SelectedItem) : -1;
            }
            set {
                if (value < 0)
                    _group.Clear();
                else
                    _items[value].IsActive = true;
            }
        }

        public T Selected {
            get {
                return this.SelectedItem != null ? this.SelectedItem.Data : default(T);
            }
        }

        public EventHandler<RadioGroupEventArgs> SelectionChanged;

        public ListItem<T> AddItem (T data) {
            return this.AddItemAt(_items.Count, data);
        }

        public ListItem<T> AddItemAt (int idx, T data) {
            if (idx < 0 || idx > _items.Count)
                throw new System.IndexOutOfRangeException();

            var extents = this.Extents;
            float y;
            if (_items.Count < 1 || idx == 0)
                y = extents.yMax - _opts.ItemHeight;
            else if (idx == _items.Count)
                y = _items[idx - 1].Position.y - _opts.ItemHeight;
            else
                y = _items[idx].Position.y;
            for (var i = idx; i < _items.Count; i++)
                _items[i].Position += new Vector3(0f, -_opts.ItemHeight, 0f);

            var item = new ListItem<T>(
                new Rect(extents.x, y, _opts.ItemWidth > 0f ? _opts.ItemWidth : this.Clip.width, _opts.ItemHeight),
                this, _group, data, _opts.ItemSprite, _opts.OverSprite, _opts.SelectedSprite, false);
            _items.Insert(idx, item);
            return item;
        }

        public bool RemoveItem (ListItem<T> item) {
            var idx = _items.IndexOf(item);
            if (idx < 0)
                return false;
            this.RemoveItemAt(idx);
            return true;
        }

        public void RemoveItemAt (int idx) {
            for (var i = idx + 1; i < _items.Count; i++)
                _items[i].Position += new Vector3(0f, _opts.ItemHeight, 0f);

            var item = _items[idx];
            if (this.SelectedItem == item)
                this.SelectedItem = null;
            _items.RemoveAt(idx);
            item.Dispose();

            var extents = this.Extents;
            extents.yMin = Mathf.Min(extents.yMax - this.Clip.height, extents.yMin + _opts.ItemHeight);
            this.Extents = extents;
        }
    }

    public class ListBoxOptions : ScrollViewOptions {
        public float ItemHeight;
        public float ItemWidth;
        public string ItemSprite;
        public string OverSprite;
        public string SelectedSprite;
    }
}
