//     TextView.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class TextView : ScrollView {
        TextViewOptions _opts;
        float _vPos;

        public TextView (Vector3 position, Vector2 size, TextViewOptions opts) :
            base(position, size, opts) {
            _opts = opts;
            _vPos = opts.StartFromBottom ? this.Extents.height : opts.PaddingTop;
        }

        public TextView (Rect bounds, float depth, TextViewOptions opts) :
            this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), opts) {
        }

        public void AppendLine (string text, Color color) {
            var extents = this.Extents;
            var clip = this.Clip;
            var label = new Label(
                new Rect(extents.x, extents.y, clip.width, 22f),
                Gui.ZIndex(0.1f, this),
                text,
                new LabelOptions {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Font = _opts.Font,
                    Overflow = Overflow.Wrap,
                    Indent = _opts.Indent,
                    Color = color,
                    Parent = this
                });
            label.Visual.Invalidate();
            _vPos += label.Visual.Bounds.height;
            label.Position += new Vector3(0f, extents.height - _vPos, 0f);

            if (_vPos > extents.height) {
                extents.yMin = extents.yMax - _vPos;
                this.Extents = extents;
            }
            this.VScrollPosition = 1f;
        }

        public void AppendLine (string text) {
            this.AppendLine(text, Color.white);
        }
    }

    public class TextViewOptions : ScrollViewOptions {
        public string Font;
        public float Indent;
        public float PaddingTop;
        public bool StartFromBottom;
    }
}
