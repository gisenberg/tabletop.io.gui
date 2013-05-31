//     UVRect.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    struct UVRect {
        public static implicit operator UVRect (Rect rect) {
            return new UVRect(rect, rect, RotateFlip.None);
        }

        Rect _fullUVRect;
        RotateFlip _rotateFlip;
        Rect _subRect;
        Rect _correctedRect;

        public UVRect (Rect fullUVRect, Rect subRect, RotateFlip rotateFlip) {
            _fullUVRect = fullUVRect;
            _subRect = subRect;
            _rotateFlip = rotateFlip;
            _correctedRect = new Rect();

            CalcCorrectedRect();
        }

        void CalcCorrectedRect () {
            if (_rotateFlip == RotateFlip.None) {
                _correctedRect = _subRect;
                return;
            }

            Vector2 c = new Vector2((_fullUVRect.xMin + _fullUVRect.xMax) / 2, (_fullUVRect.yMin + _fullUVRect.yMax) / 2);
            _correctedRect = _subRect;

            if ((_rotateFlip & RotateFlip.FlipH) != RotateFlip.None)
                _correctedRect = Rect.MinMaxRect(
                    c.x - (_correctedRect.xMax - c.x),
                    _correctedRect.yMin,
                    c.x + (c.x - _correctedRect.xMin),
                    _correctedRect.yMax
               );
            if ((_rotateFlip & RotateFlip.FlipV) != RotateFlip.None)
                _correctedRect = Rect.MinMaxRect(
                    _correctedRect.xMin,
                    c.y - (_correctedRect.yMax - c.y),
                    _correctedRect.xMax,
                    c.y + (c.y - _correctedRect.yMin)
               );

            if ((_rotateFlip & RotateFlip.Rotate90CW) != RotateFlip.None)
                _correctedRect = Rect.MinMaxRect(
                    _fullUVRect.xMin + ((_fullUVRect.yMax - _correctedRect.yMax) / _fullUVRect.height) * _fullUVRect.width,
                    _fullUVRect.yMin + ((_correctedRect.xMin - _fullUVRect.xMin) / _fullUVRect.width) * _fullUVRect.height,
                    _fullUVRect.xMax - ((_correctedRect.yMin - _fullUVRect.yMin) / _fullUVRect.height) * _fullUVRect.width,
                    _fullUVRect.yMax - ((_fullUVRect.xMax - _correctedRect.xMax) / _fullUVRect.width) * _fullUVRect.height
                );
            else if ((_rotateFlip & RotateFlip.Rotate180) != RotateFlip.None)
                _correctedRect = Rect.MinMaxRect(
                    c.x - (_correctedRect.xMax - c.x),
                    c.y - (_correctedRect.yMax - c.y),
                    c.x + (c.x - _correctedRect.xMin),
                    c.y + (c.y - _correctedRect.yMin)
               );
            else if ((_rotateFlip & RotateFlip.Rotate270CW) != RotateFlip.None)
                _correctedRect = Rect.MinMaxRect(
                    _fullUVRect.xMin + ((_correctedRect.yMin - _fullUVRect.yMin) / _fullUVRect.height) * _fullUVRect.width,
                    _fullUVRect.yMin + ((_fullUVRect.xMax - _correctedRect.xMax) / _fullUVRect.width) * _fullUVRect.height,
                    _fullUVRect.xMax - ((_fullUVRect.yMax - _correctedRect.yMax) / _fullUVRect.height) * _fullUVRect.width,
                    _fullUVRect.yMax - ((_correctedRect.xMin - _fullUVRect.xMin) / _fullUVRect.width) * _fullUVRect.height
                );
        }

        Vector2 RotateAndFlip (bool x, bool y) {
            if (_rotateFlip == RotateFlip.None)
                return new Vector2(x ? _subRect.xMax : _subRect.xMin, y ? _subRect.yMax : _subRect.yMin);

            if ((_rotateFlip & RotateFlip.Rotate90CW) != RotateFlip.None) {
                if (x == y)
                    x = !x;
                else
                    y = !y;
            } else if ((_rotateFlip & RotateFlip.Rotate180) != RotateFlip.None) {
                x = !x;
                y = !y;
            } else if ((_rotateFlip & RotateFlip.Rotate270CW) != RotateFlip.None) {
                if (x == y)
                    y = !y;
                else
                    x = !x;
            }

            if ((_rotateFlip & RotateFlip.FlipH) != RotateFlip.None)
                x = !x;
            if ((_rotateFlip & RotateFlip.FlipV) != RotateFlip.None)
                y = !y;

            return new Vector2(x ? _correctedRect.xMax : _correctedRect.xMin, y ? _correctedRect.yMax : _correctedRect.yMin);
        }

        public Rect FullUVRect {
            get { return _fullUVRect; }
            set {
                _fullUVRect = value;
                CalcCorrectedRect();
            }
        }
        public Rect SubRect {
            get { return _subRect; }
            set {
                _subRect = value;
                CalcCorrectedRect();
            }
        }
        public RotateFlip RotateFlip {
            get { return _rotateFlip; }
            set {
                _rotateFlip = value;
                CalcCorrectedRect();
            }
        }

        public Vector2 XMinYMin {
            get {
                return RotateAndFlip(false, false);
            }
        }
        public Vector2 XMinYMax {
            get {
                return RotateAndFlip(false, true);
            }
        }
        public Vector2 XMaxYMin {
            get {
                return RotateAndFlip(true, false);
            }
        }
        public Vector2 XMaxYMax {
            get {
                return RotateAndFlip(true, true);
            }
        }
    }
}
