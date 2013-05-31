//     TextBox.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class TextBox : Control<SpriteVisual>, ISpriteContent, IMouseInput, IKeyboardInput, ICustomHitBox, ICustomCursor, IContainer
#if PLATFORM_IOS
        , IUpdate
#endif
    {
        TextBoxState _state;
        TextBoxOptions _opts;
        Label _text;
        Image _caret;
        Cursor _cursor;
        Rect _clip;
        int _caretIndex;
        Transition _blink;
#if PLATFORM_IOS
        TouchScreenKeyboard _keyboard;
#endif

        public TextBox (Vector3 position, Vector2 size, TextBoxOptions opts)
            : base(position, size, opts) {
            _opts = opts;
            _clip = new Rect(
                opts.Border.x,
                opts.Border.y,
                size.x - opts.Border.x - opts.Border.z,
                size.y - opts.Border.y - opts.Border.w
                );

            var clip = this.GetClipRect();
            _text = new Label(
                new Rect(clip.x, clip.y, clip.width * 1000f, clip.height),
                position.z - 0.1f,
                "",
                new LabelOptions {
                    Font = opts.Font,
                    VerticalAlignment = VerticalAlignment.Center,
                    Name = "text",
                    Parent = this,
                    IsHitTestable = true
                });

            _caret = new Image(
                _text.Position + new Vector3(0f, (_text.Size.y - opts.CaretSize.y) / 2f, -0.1f),
                opts.CaretSize,
                opts.CaretSprite,
                new VisualOptions {
                    Name = "caret",
                    Parent = this,
                    PixelAlign = false
                });
            _caret.SetActive(false);
            _cursor = opts.Cursor;
        }

        public TextBox (Rect bounds, float depth, TextBoxOptions opts)
            : this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), opts) {
        }

        public TextBoxState State {
            get {
                return _state;
            }
            protected set {
                if (_state != value) {
                    _state = value;
                    this.Invalidate();
                    _caret.SetActive(value == TextBoxState.Active && this.Visual != null && this.Visual.gameObject.active);
                }
            }
        }

        public bool IsEnabled {
            get {
                return this.State != TextBoxState.Disabled;
            }
            set {
                this.State = value ? TextBoxState.Normal : TextBoxState.Disabled;
            }
        }

        public string Text {
            get {
                return _text.Text;
            }
            set {
                _text.Text = value;
                if (this.TextChanged != null)
                    this.TextChanged(this, EventArgs.Empty);
                this.CaretIndex = _caretIndex;
            }
        }

        public int CaretIndex {
            get {
                return _caretIndex;
            }
            set {
                _caretIndex = Math.Max(0, Math.Min(this.Text.Length, value));
                float x = _text.Position.x;
                if (_caretIndex > 0)
                    x = _text.Visual.GetCharacterBounds(_caretIndex - 1).xMax;

                // make sure caret is visible at all times
                var pos = _text.Position;
                var clip = this.GetClipRect();
                if (x < clip.xMin) {
                    var dx = clip.xMin - x;
                    _text.Position = new Vector3(pos.x + dx, pos.y, pos.z);
                    x += dx;
                } else if (x +_opts.CaretSize.x > clip.xMax) {
                    var dx = (x + _opts.CaretSize.x) - clip.xMax;
                    _text.Position = new Vector3(pos.x - dx, pos.y, pos.z);
                    x -= dx;
                }

                pos = _caret.Position;
                _caret.Position = new Vector3(x, pos.y, pos.z);
            }
        }

        public EventHandler TextChanged;
        public EventHandler Committed;

        public override void Dispose () {
            if (_blink != null)
                _blink.Dispose();
            if (this.State == TextBoxState.Active)
                Gui.SetFocus(null);
            base.Dispose();
        }

        string ISpriteContent.Sprite {
            get {
                switch (this.State) {
                case TextBoxState.Active:
                    return _opts.ActiveSprite ?? _opts.NormalSprite;
                case TextBoxState.Over:
                    return _opts.OverSprite ?? _opts.NormalSprite;
                case TextBoxState.Disabled:
                    return _opts.DisabledSprite ?? _opts.NormalSprite;
                default:
                    return _opts.NormalSprite;
                }
            }
        }

        void IMouseInput.OnMouseEnter (IControl src) {
        }

        void IMouseInput.OnMouseExit (IControl src) {
        }

        void IMouseInput.OnMouseDown (IControl src, Vector3 point) {
            if (this.State != TextBoxState.Disabled) {
#if !PLATFORM_IOS
                var i = _text.Visual.GetCharacterAt(point);
                if (i >= 0) {
                    var r = _text.Visual.GetCharacterBounds(i);
                    this.CaretIndex = i + ((r.xMax - point.x < point.x - r.xMin) ? 1 : 0);
                }
#endif
                Gui.SetFocus(this);
                _blink.Repeat();
            }
        }

        void IMouseInput.OnMouseUp (IControl src) {
        }

        void IMouseInput.OnMouseDrag (IControl src, Vector2 delta) {
        }

        void IMouseInput.OnMouseWheel (IControl src, float delta) {
        }

        void IKeyboardInput.OnGotFocus () {
            this.State = TextBoxState.Active;
            _blink = Transition.New(_opts.CaretBlinkRate, 0f, 1f,
                (from, to, value) => value,
                (value) => _caret.SetActive(value < 0.5f && this.Visual.gameObject.active)).Uses(this).Repeat();
#if PLATFORM_IOS
            if (_keyboard != null) {
                _keyboard.active = true;
                this.CaretIndex = this.Text.Length;
            } else {
                TouchScreenKeyboard.hideInput = true;
                _keyboard = TouchScreenKeyboard.Open(this.Text, TouchScreenKeyboardType.ASCIICapable);
            }
#endif
        }

        void IKeyboardInput.OnLostFocus () {
            this.State = TextBoxState.Normal;
            _blink.Dispose();
        }

        void IKeyboardInput.OnKeyPress (char ch) {
            if (this.State == TextBoxState.Disabled)
                return;

            var text = _text.Text;
            var idx = _caretIndex - 1;

            if(ch == '\b' && _caretIndex > 0) {
                var r = _text.Visual.GetCharacterBounds(idx);
                this.Text = text.Substring(0, idx) + text.Substring(idx + 1);
                // if possible, scroll in from the left to fill the space instead of moving the caret
                var clip = this.GetClipRect();
                if (_text.Position.x < clip.xMin)
                    _text.Position += new Vector3(Mathf.Min(r.width, clip.xMin - _text.Position.x), 0f, 0f);
                this.CaretIndex = idx;
            } else if (ch == '\n' || ch == '\r') {
                if (this.Committed != null)
                    this.Committed(this, EventArgs.Empty);
            } else if (ch >= 32 && (_opts.MaxLength <= 0 || _text.Text.Length < _opts.MaxLength)) {
                this.Text = text.Substring(0, idx + 1) + ch + text.Substring(idx + 1);
                this.CaretIndex = idx + 2;
            }
            _blink.Repeat();
        }

        void IKeyboardInput.OnKeyPress (KeyCode key) {
            if(this.State == TextBoxState.Disabled)
                return;

            var idx = _caretIndex - 1;
            var clip = this.GetClipRect();

            switch (key) {
            case KeyCode.LeftArrow:
                // make the text "jump" when the caret is arrowed past the end
                if (idx >= 0 && _caret.Position.x - _text.Visual.GetCharacterBounds(idx).width < clip.xMin)
                    _text.Position += new Vector3(Mathf.Min(clip.xMin - _text.Position.x, clip.width * 0.25f), 0f, 0f);
                this.CaretIndex--;
                break;
            case KeyCode.RightArrow:
                // the jump thing again
                if (_caretIndex < _text.Text.Length && _caret.Position.x + _text.Visual.GetCharacterBounds(_caretIndex).width > clip.xMax)
                    _text.Position -= new Vector3(Mathf.Min(_text.Position.x + _text.Visual.Bounds.width - clip.xMax, clip.width * 0.25f), 0f, 0f);
                this.CaretIndex++;
                break;
            case KeyCode.Home:
                this.CaretIndex = 0;
                break;
            case KeyCode.End:
                this.CaretIndex = int.MaxValue;
                break;
            case KeyCode.Delete:
                var text = this.Text;
                if(_caretIndex < text.Length)
                    this.Text = text.Substring(0, _caretIndex) + text.Substring(_caretIndex + 1);
                break;
            }
            _blink.Repeat();
        }

#if PLATFORM_IOS
        void IUpdate.Update (float deltaTime) {
            if (_keyboard != null) {
                if (this.Text != _keyboard.text) {
                    this.Text = _keyboard.text;
                    this.CaretIndex = this.Text.Length;
                }
                if ((_keyboard.done || !_keyboard.active) && this.State == TextBoxState.Active)
                    Gui.SetFocus(null);
                if (_keyboard.done) {
                    if (this.Committed != null)
                        this.Committed(this, EventArgs.Empty);
                    _keyboard = null;
                } 
            }
        }
#endif

        Rect ICustomHitBox.HitBox {
            get {
                return this.GetClipRect();
            }
        }

        Cursor ICustomCursor.Cursor {
            get {
                return _cursor;
            }
        }

        Rect IContainer.GetClipRect (IControl child) {
            return this.GetClipRect();
        }

        void IContainer.OnChildAdded (IControl child) {
        }

        void IContainer.OnChildRemoved (IControl child) {
        }

        Rect GetClipRect () {
            return _clip.Shift(this.Position);
        }
    }

    public enum TextBoxState {
        Normal,
        Over,
        Active,
        Disabled
    }

    public class TextBoxOptions : VisualOptions {
        public string Font;
        public string NormalSprite;
        public string OverSprite;
        public string ActiveSprite;
        public string DisabledSprite;
        public string CaretSprite;
        public Vector2 CaretSize;
        public Vector4 Border;
        public Cursor Cursor;
        public float CaretBlinkRate = 1f;
        public int MaxLength;
    }
}
