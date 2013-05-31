//     ScrollView.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class ScrollView : Image, IContainer, IMouseInput, ICustomHitBox {
        ScrollViewOptions _opts;
        Rect _clip, _extents;
        Image _vScroll, _hScroll;
        Button _vKnob, _hKnob;
        float _grab, _delta;
        bool _vAutoSize, _hAutoSize;

        public ScrollView (Vector3 position, Vector2 size, ScrollViewOptions opts)
            : base(position, size, opts.Sprite, opts) {
            _clip = new Rect(
                opts.Border.x,
                opts.Border.y + opts.HScrollBarHeight,
                size.x - opts.Border.x - opts.Border.z - opts.VScrollBarWidth,
                size.y - opts.Border.y - opts.Border.w - opts.HScrollBarHeight
                );
            _extents = _clip;
            _opts = opts;

            var clip = this.Clip;
            // vertical scroll bar
            if (opts.VScrollBarWidth > 0f) {
                _vScroll = new Image(
                    new Rect(clip.xMax, clip.yMin, opts.VScrollBarWidth, clip.height),
                    position.z - 0.1f,
                    opts.VScrollBarSprite,
                    new VisualOptions {
                        Name = "vscroll",
                        Parent = this,
                        PixelAlign = true
                    });
                var knobSize = opts.VKnobSize;
                if (knobSize.x <= 0f)
                    knobSize.x = opts.VScrollBarWidth;
                if (knobSize.y <= 0f) {
                    _vAutoSize = true;
                    knobSize.y = clip.height;
                }
                var x = (clip.xMax + opts.VScrollBarWidth / 2f) - knobSize.x / 2f;
                _vKnob = new Button(
                    new Rect(x, clip.yMin, knobSize.x, knobSize.y),
                    position.z - 0.9f,
                    new ButtonOptions {
                        NormalSprite = opts.VKnobSprite,
                        OverSprite = opts.VKnobOverSprite,
                        PressedSprite = opts.VKnobPressedSprite,
                        Name = "vscrollknob",
                        Parent = this,
                        PixelAlign = !_vAutoSize
                    });
            }
            // horizontal scroll bar
            if (opts.HScrollBarHeight > 0f) {
                _hScroll = new Image(
                    new Rect(clip.xMin, clip.yMin - opts.HScrollBarHeight, clip.width, opts.HScrollBarHeight),
                    position.z - 0.1f,
                    opts.HScrollBarSprite,
                    new VisualOptions {
                        Name = "hscroll",
                        Parent = this,
                        PixelAlign = false
                    });
                var knobSize = opts.VKnobSize;
                if (knobSize.y <= 0f)
                    knobSize.y = opts.HScrollBarHeight;
                if (knobSize.x <= 0f) {
                    _hAutoSize = true;
                    knobSize.x = clip.width;
                }
                var y = (clip.yMin - opts.HScrollBarHeight / 2f) - knobSize.y / 2f;
                _hKnob = new Button(
                    new Rect(clip.xMin, y, knobSize.x, knobSize.y),
                    position.z - 0.9f,
                    new ButtonOptions {
                        NormalSprite = opts.HKnobSprite,
                        OverSprite = opts.HKnobOverSprite,
                        PressedSprite = opts.HKnobPressedSprite,
                        Name = "hscrollknob",
                        Parent = this,
                        PixelAlign = !_hAutoSize
                    });
            }
        }

        public ScrollView (Rect bounds, float depth, ScrollViewOptions opts) :
            this(new Vector3(bounds.x, bounds.y, depth), new Vector2(bounds.width, bounds.height), opts) {
        }

        public float VScrollPosition {
            get {
                if (_extents.height <= _clip.height)
                    return 0f;
                else
                    return 1f - ((_clip.yMin - _extents.yMin) / (_extents.height - _clip.height));
            }
            set {
                this.SetVPosition(value);
                this.SetKnobPosition();
            }
        }

        public float HScrollPosition {
            get {
                return (_clip.xMin - _extents.xMin) / (_extents.width - _clip.width);
            }
            set {
                this.SetHPosition(1f - value);
                this.SetKnobPosition();
            }
        }

        public Rect Extents {
            get {
                return _extents.Shift(this.Position);
            }
            protected set {
                _extents = value.Shift(-this.Position);
                // adjust size of the scroll knobs
                if (_vKnob != null && _vAutoSize) {
                    var ratio = _clip.height / _extents.height;
                    _vKnob.Size = new Vector2(_vKnob.Size.x, Mathf.Max(_opts.VScrollBarWidth, ratio * _clip.height));
                }
                if (_hKnob != null && _hAutoSize) {
                    var ratio = _clip.width / _extents.width;
                    _hKnob.Size = new Vector2(Mathf.Max(_opts.HScrollBarHeight, ratio * _clip.width), _hKnob.Size.y);
                }
                this.SetKnobPosition();
            }
        }

        public Rect Clip {
            get {
                return _clip.Shift(this.Position);
            }
        }
        
        void ExpandExtents (IControl child) {
            var pos = child.Position;
            var size = child.Size;
            var extents = this.Extents;
            extents.xMin = Mathf.Min(extents.xMin, pos.x);
            extents.yMin = Mathf.Min(extents.yMin, pos.y);
            extents.xMax = Mathf.Max(extents.xMax, pos.x + size.x);
            extents.yMax = Mathf.Max(extents.yMax, pos.y + size.y);
            this.Extents = extents;
        }

        void SetKnobPosition () {
            var clip = this.Clip;
            if (_vKnob != null) {
                var pos = _vKnob.Position;
                _vKnob.Position = new Vector3(pos.x, clip.yMin + (1f - this.VScrollPosition) * (clip.height - _vKnob.Size.y), pos.z);
            }
            if (_hKnob != null) {
                var pos = _hKnob.Position;
                _hKnob.Position = new Vector3(clip.xMin + this.HScrollPosition * (clip.width - _hKnob.Size.x), pos.y, pos.z);
            }
        }

        void SetVPosition (float pos) {
            if (!float.IsNaN(pos)) {
                pos = pos * (_extents.height - _clip.height) + (_clip.yMax - _extents.height);
                pos = Mathf.Clamp(pos, _clip.yMax - _extents.height, _clip.yMin) - _extents.y;
                _extents.y += pos;
                foreach (var child in this.GetChildren())
                    if (child != _vScroll && child != _vKnob && child != _hScroll && child != _hKnob)
                        child.Position += new Vector3(0f, pos, 0f);
            }
        }

        void SetHPosition (float pos) {
            if (!float.IsNaN(pos)) {
                pos = pos * (_extents.width - _clip.width) + (_clip.xMax - _extents.width);
                pos = Mathf.Clamp(pos, _clip.xMax - _extents.width, _clip.xMin) - _extents.x;
                _extents.x += pos;
                foreach (var child in this.GetChildren())
                    if (child != _vScroll && child != _vKnob && child != _hScroll && child != _hKnob)
                        child.Position += new Vector3(pos, 0f, 0f);
            }
        }

        void IContainer.OnChildAdded (IControl child) {
            if(_opts != null)
                this.ExpandExtents(child);
        }

        void IContainer.OnChildRemoved (IControl child) {
            _extents = _clip;
            foreach (var item in this.GetDescendants())
                this.ExpandExtents(item);
        }

        Rect IContainer.GetClipRect (IControl child) {
            if (!_opts.Clipped || child == _vScroll || child == _vKnob || child == _hScroll || child == _hKnob)
                return new Rect();
            else
                return this.Clip;
        }

        Rect ICustomHitBox.HitBox {
            get {
                return this.Clip;
            }
        }

        void IMouseInput.OnMouseEnter (IControl src) {
        }

        void IMouseInput.OnMouseExit (IControl src) {
        }

        void IMouseInput.OnMouseDown (IControl src, Vector3 point) {
            if (src == _vKnob) {
                _grab = _vKnob.Position.y;
                _delta = 0f;
            } else if (src == _hKnob) {
                _grab = _hKnob.Position.x;
                _delta = 0f;
            }
        }

        void IMouseInput.OnMouseUp (IControl src) {
        }

        void IMouseInput.OnMouseDrag (IControl src, Vector2 delta) {
            var clip = this.Clip;
            if (src == _vKnob) {
                _delta += delta.y;
                var pos = _vKnob.Position;
                var d = Mathf.Clamp(_grab + _delta, clip.yMin, clip.yMax - _vKnob.Size.y);
                _vKnob.Position = new Vector3(pos.x, d, pos.z);
                this.SetVPosition(1f - ((_vKnob.Position.y - clip.yMin) / (clip.height - _vKnob.Size.y)));
            } else if (src == _hKnob) {
                _delta += delta.x;
                var pos = _hKnob.Position;
                var d = Mathf.Clamp(_grab + _delta, clip.xMin, clip.xMax - _hKnob.Size.x);
                _hKnob.Position = new Vector3(d, pos.y, pos.z);
                this.SetHPosition(1f - ((_hKnob.Position.x - clip.xMin) / (clip.width - _hKnob.Size.x)));
            } else if(_opts.HAllowDrag || _opts.VAllowDrag) {
                var d = Vector2.zero;
                if (_opts.HAllowDrag)
                    d.x = Mathf.Clamp(delta.x, _clip.xMax - _extents.xMax, _clip.xMin - _extents.xMin);
                if (_opts.VAllowDrag)
                    d.y = Mathf.Clamp(delta.y, _clip.yMax - _extents.yMax, _clip.yMin - _extents.yMin);
                _extents.x += d.x;
                _extents.y += d.y;
                foreach (var child in this.GetChildren())
                    if (child != _vScroll && child != _vKnob && child != _hScroll && child != _hKnob)
                        child.Position += new Vector3(d.x, d.y, 0f);
                this.SetKnobPosition();
            }
        }

        void IMouseInput.OnMouseWheel (IControl src, float delta) {
            if (_extents.height > _clip.height) {
                delta *= 10f * _opts.ScrollSpeed / (_extents.height - _clip.height);
                this.VScrollPosition -= delta;
            }
        }
    }

    public class ScrollViewOptions : VisualOptions {
        public string Sprite;
		public bool Clipped = true;
        public bool VAllowDrag = false;
        public bool HAllowDrag = false;
        public Vector4 Border;
        public float HScrollBarHeight;
        public float VScrollBarWidth;
        public float ScrollSpeed = 10f;
        public string VScrollBarSprite;
        public string VKnobSprite;
        public string VKnobOverSprite;
        public string VKnobPressedSprite;
        public Vector2 VKnobSize;
        public string HScrollBarSprite;
        public string HKnobSprite;
        public string HKnobOverSprite;
        public string HKnobPressedSprite;
        public Vector2 HKnobSize;

        public string ScrollBarSprite {
            set {
                this.VScrollBarSprite = this.HScrollBarSprite = value;
            }
        }

        public string KnobSprite {
            set {
                this.VKnobSprite = this.HKnobSprite = value;
            }
        }

        public string KnobOverSprite {
            set {
                this.VKnobOverSprite = this.HKnobOverSprite = value;
            }
        }

        public string KnobPressedSprite {
            set {
                this.VKnobPressedSprite = this.HKnobPressedSprite = value;
            }
        }
    }
}
