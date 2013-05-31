//     Button.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class Button : Control<SpriteVisual>, IMouseInput, ISpriteContent {
        ButtonState _state;
        Label _label;
        ButtonOptions _opts;

        public Button (Vector3 position, Vector3 size, ButtonOptions opts) :
            base(position, size, opts) {
            _opts = opts;
            if (_opts.Group != null)
                _opts.Group.Add(this);
        }

        public Button (Rect bounds, float depth, ButtonOptions opts)
            : this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), opts) {
        }

        public ButtonState State {
            get {
                return _state;
            }
            protected set {
                if (_state != value) {
                    var oldValue = _state;
                    _state = value;
                    if (this.StateChanged != null)
                        this.StateChanged(this, new ButtonStateChangedEventArgs(value, oldValue));
                    this.Invalidate();
                }
            }
        }

        public bool IsEnabled {
            get {
                return this.State != ButtonState.Disabled;
            }
            set {
                if(value != (this.State != ButtonState.Disabled))
                    this.State = value ? ButtonState.Normal : ButtonState.Disabled;
            }
        }

        public bool IsActive {
            get {
                return this.State == ButtonState.Active || this.State == ButtonState.ActivePressed || this.State == ButtonState.ActiveClickOut;
            }
            set {
                this.State = value ? ButtonState.Active : ButtonState.Normal;
            }
        }

        public Label Label { get { return _label; } }

        public EventHandler<ButtonStateChangedEventArgs> StateChanged;
        public EventHandler Clicked;

        public void SetLabel (string text, string font, Color color, HorizontalAlignment hAlign) {
            if (_label != null)
                _label.Dispose();
            _label = new Label(
                this.Position + new Vector3(0f, 0f, -0.1f),
                this.Size,
                text,
                new LabelOptions() {
                    Font = font,
                    HorizontalAlignment = hAlign,
                    VerticalAlignment = VerticalAlignment.Center,
                    Color = color,
                    Parent = this,
                    Name = "label",
                    AnchorTo = AnchorTo.Center
                });
        }

        public void SetLabel (string text, string font, Color color) {
            this.SetLabel(text, font, color, HorizontalAlignment.Center);
        }

        public void SetLabel (string text, string font) {
            this.SetLabel(text, font, Color.white, HorizontalAlignment.Center);
        }

        string ISpriteContent.Sprite {
            get {
                switch (this.State) {
                case ButtonState.Disabled:
                    return _opts.DisabledSprite ?? _opts.NormalSprite;
                case ButtonState.Pressed:
                    return (_opts.PressedSprite ?? _opts.OverSprite) ?? _opts.NormalSprite;
                case ButtonState.Active:
                case ButtonState.ActiveClickOut:
                    return _opts.ActiveSprite ?? _opts.NormalSprite;
                case ButtonState.ActivePressed:
                    return ((_opts.ActivePressedSprite ?? _opts.PressedSprite) ?? _opts.ActiveSprite) ?? _opts.NormalSprite;
                case ButtonState.Over:
                    return _opts.OverSprite ?? _opts.NormalSprite;
                default:
                    return _opts.NormalSprite;
                }
            }
        }

        void IMouseInput.OnMouseEnter (IControl src) {
            if (src != this)
                return;

            switch (this.State) {
            case ButtonState.Active:
                break;
            case ButtonState.Normal:
                this.State = ButtonState.Over;
                break;
            case ButtonState.ClickOut:
                this.State = ButtonState.Pressed;
                break;
            case ButtonState.ActiveClickOut:
                this.State = ButtonState.ActivePressed;
                break;
            }
        }

        void IMouseInput.OnMouseExit (IControl src) {
            if (src != this)
                return;

            switch (this.State) {
            case ButtonState.Over:
                this.State = ButtonState.Normal;
                break;
            case ButtonState.Pressed:
                this.State = ButtonState.ClickOut;
                break;
            case ButtonState.ActivePressed:
                this.State = ButtonState.ActiveClickOut;
                break;
            }
        }

        void IMouseInput.OnMouseDown (IControl src, Vector3 point) {
            if (src != this)
                return;

            switch (this.State) {
            case ButtonState.Disabled:
                break;
            case ButtonState.Active:
                this.State = ButtonState.ActivePressed;
                break;
            default:
                this.State = ButtonState.Pressed;
                break;
            }
        }

        void IMouseInput.OnMouseUp (IControl src) {
            if (src != this)
                return;

            switch (this.State) {
            case ButtonState.ClickOut:
                this.State = ButtonState.Normal;
                break;
            case ButtonState.Pressed:
                this.State = _opts.IsToggle || _opts.Group != null ? ButtonState.Active : ButtonState.Over;
                this.OnClicked();
                break;
            case ButtonState.ActivePressed:
                this.State = ButtonState.Over;
                this.OnClicked();
                break;
            }
        }

        void IMouseInput.OnMouseDrag (IControl src, Vector2 delta) {
        }

        void IMouseInput.OnMouseWheel (IControl src, float delta) {
        }

        void OnClicked () {
            if (this.Clicked != null)
                this.Clicked(this, EventArgs.Empty);
        }

        public override void Dispose () {
            if (_label != null)
                _label.Dispose();
            if (_opts.Group != null)
                _opts.Group.Remove(this);

            base.Dispose();
        }
    }

    public enum ButtonState {
        Normal,
        Over,
        Pressed,
        Active,
        ActivePressed,
        Disabled,
        ClickOut,
        ActiveClickOut
    }

    public class ButtonOptions : VisualOptions {
        public string NormalSprite;
        public string OverSprite;
        public string PressedSprite;
        public string ActiveSprite;
        public string ActivePressedSprite;
        public string DisabledSprite;
        public bool IsToggle;
        public RadioGroup Group;
    }

    public class ButtonStateChangedEventArgs : EventArgs {
        public ButtonState State { get; private set; }
        public ButtonState OldState { get; private set; }

        internal ButtonStateChangedEventArgs (ButtonState state, ButtonState oldState) {
            this.State = state;
            this.OldState = oldState;
        }
    }
}
