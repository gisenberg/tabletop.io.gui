//     Label.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class Label : Control<TextVisual>, ITextContent {
        string _text;
        LabelOptions _opts;

        public Label (Vector3 position, Vector2 size, string text, LabelOptions opts)
            : base(position, size, opts) {
            _text = text;
            _opts = opts;
        }

        public Label (Rect bounds, float depth, string text, LabelOptions opts)
            : this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), text, opts) {
        }

        public string Text {
            get {
                return _text;
            }
            set {
                _text = value;
                this.Invalidate();
            }
        }

        string ITextContent.Font {
            get {
                return _opts.Font;
            }
        }

        string ITextContent.BoldFont {
            get {
                return _opts.BoldFont;
            }
        }

        HorizontalAlignment ITextContent.HorizontalAlignment {
            get {
                return _opts.HorizontalAlignment;
            }
        }

        VerticalAlignment ITextContent.VerticalAlignment {
            get {
                return _opts.VerticalAlignment;
            }
        }

        Overflow ITextContent.Overflow {
            get {
                return _opts.Overflow;
            }
        }

        float ITextContent.Indent {
            get {
                return _opts.Indent;
            }
        }

        float ITextContent.Kerning {
            get {
                return _opts.Kerning;
            }
        }

        float ITextContent.Leading {
            get {
                return _opts.Leading;
            }
        }

        bool ITextContent.IsHitTestable {
            get {
                return _opts.IsHitTestable;
            }
        }

        bool ITextContent.UseBBCode {
            get {
                return _opts.UseBBCode;
            }
        }
    }

    public class LabelOptions : VisualOptions {
        public string Font;
        public string BoldFont;
        public HorizontalAlignment HorizontalAlignment;
        public VerticalAlignment VerticalAlignment;
        public Overflow Overflow;
        public float Indent;
        public float Kerning;
        public float Leading;
        public bool IsHitTestable;
        public bool UseBBCode;
    }
}
