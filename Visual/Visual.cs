//     Visual.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    public abstract class Visual : MonoBehaviour, IDisposable {
        protected static bool s_isDirectX = Application.platform == RuntimePlatform.WindowsEditor
                || Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.WindowsWebPlayer;

        IUpdate _iupdate;
        IContainer _icontainer;
        bool _isClipped, _isBuilt, _isDisposed;
        Vector3 _position;
        Vector2 _size;
        Color _color;

        public IControl Control { get; private set; }
        public VisualOptions RenderOptions { get; private set; }
        public Layer Layer { get; private set; }
        public string Name { get { return this.RenderOptions.Name; } }
        public virtual Rect Bounds { get { return new Rect(_position.x, _position.y, _size.x, _size.y); } }

        public Vector3 Position {
            get {
                return _position;
            }
            set {
                var delta = value - _position;
                _position = value;
                if (!_isDisposed) {
                    this.UpdatePosition();
                    foreach (var child in this.GetDescendants()) {
                        child._position += delta;
                        if (child._icontainer != null)
                            child.UpdatePosition();
                    }
                }
            }
        }

        public Vector2 Size {
            get {
                return _size;
            }
            set {
                _size = value;
                _isBuilt = false;
            }
        }

        public Color Color {
            get {
                return _color;
            }
            set {
                _color = value;
                this.UpdateColorFast(value);
            }
        }

        public IEnumerable<T> GetBubbleTargets<T> () where T : class {
            var node = this.transform;
            while (node != null) {
                var visual = node.GetComponent<Visual>();
                if (visual != null) {
                    var ctrl = visual.Control as T;
                    if (ctrl != null)
                        yield return ctrl;
                }
                node = (node == null) ? null : node.parent;
            }
        }

        public IEnumerable<Visual> GetDescendants () {
            return this.GetComponentsInChildren<Visual>(true).Where(v => v != this);
        }

        public virtual void SetVisible (bool isVisible) {
            if (!_isDisposed) {
                foreach (var r in this.GetComponentsInChildren<MeshRenderer>())
                    r.enabled = isVisible;
            }
        }

        public virtual void SetActive (bool isActive) {
            if(!_isDisposed)
                gameObject.SetActiveRecursively(isActive);
        }

        public void Dispose () {
            if (!_isDisposed) {
                this.Clear();
                GameObject.Destroy(this.gameObject);
                _isDisposed = true;
            }
        }

        internal void Attach (IControl control, Vector3 pos, Vector3 size, VisualOptions opts) {
            opts = opts ?? new VisualOptions();
            this.Control = control;
            this.RenderOptions = opts;
            _position = pos;
            _size = size;
            _color = opts.Color;

            if (opts.Parent != null) {
                this.Layer = opts.Parent.Visual.Layer;
                gameObject.transform.parent = opts.Parent.Visual.gameObject.transform;
            } else
                this.Layer = Gui.Instance.GetLayer(opts.Layer);
            gameObject.layer = opts.Billboard == BillboardType.Screen ? Gui.Instance.GetLayer("gui").Index : this.Layer.Index;
            gameObject.name = string.Format("{0}:{1}:{2}", this.Layer.Name, control.GetType().Name, opts.Name ?? "?");
            this.Attach(control);
            _iupdate = control as IUpdate;
            this.UpdatePosition();
            foreach (var container in this.GetBubbleTargets<IContainer>()) {
                if (container != this.Control) {
                    _icontainer = container;
                    container.OnChildAdded(control);
                    break;
                }
            }
        }

        protected virtual void Attach (IControl control) {
        }

        protected abstract void Clear ();

        protected abstract void Build (bool isClipped, Rect clipRect);

        void UpdatePosition () {
            if (_isDisposed)
                return;

            var wasClipped = _isClipped;
            var layer = this.Layer;
            var pos = _position;

            if (this.RenderOptions.Billboard == BillboardType.Screen) {
                pos = layer.Camera.WorldToScreenPoint(pos);
                pos.z = this.RenderOptions.BillboardDepth;
                pos.x -= _size.x / 2f;
                pos.y -= _size.y / 2f;
                layer = Gui.Instance.GetLayer("gui");
            }

            bool hAlign = this.RenderOptions.HPixelAlign,
                vAlign = this.RenderOptions.VPixelAlign;

            if (hAlign || vAlign) {
                pos = new Vector3(hAlign ? Mathf.Round(pos.x) : pos.x, vAlign ? Mathf.Round(pos.y) : pos.y, pos.z);
                if (s_isDirectX)
                    pos += new Vector3(hAlign ? -0.5f : 0f, vAlign ? 0.5f : 0f, 0f);
            }

            if (this.RenderOptions.AnchorTo == AnchorTo.Center) {
                pos += new Vector3(_size.x / 2f, _size.y / 2f, 0f);
            }

            transform.position = pos;

            if (_icontainer != null) {
                var bounds = this.Bounds;
                _isClipped = false;
                var clipRect = _icontainer.GetClipRect(this.Control);
                if (clipRect.width > 0f && clipRect.height > 0f && (
                    bounds.x < clipRect.x || bounds.y < clipRect.y ||
                    bounds.xMax > clipRect.xMax || bounds.yMax > clipRect.yMax))
                    _isClipped = true;
                if (_isClipped || wasClipped)
                    _isBuilt = false;
            }
        }

        protected virtual void UpdateColorFast (Color c) {
            var mf = this.GetComponent<MeshFilter>();
            if (mf != null) {
                var colors = new Color[mf.mesh.vertices.Length];
                for (var i = 0; i < colors.Length; i++)
                    colors[i] = c;
                mf.mesh.colors = colors;
            }
        }

        public void Invalidate () {
            if (_isDisposed || this.Layer.Camera == null)
                return;

            this.Clear();
            var box = collider as BoxCollider;
            if (box != null)
                box.enabled = false;

            if (_icontainer != null) {
                var isClipped = false;
                var clipRect = _icontainer.GetClipRect(this.Control);
                if (clipRect.width > 0f && clipRect.height > 0f)
                    isClipped = true;
                var area = this.Bounds;
                if (isClipped && _isBuilt && (area.x > clipRect.xMax || area.y > clipRect.yMax ||
                    area.xMax < clipRect.xMin || area.yMax < clipRect.yMin))
                    return;
                clipRect.x -= _position.x;
                clipRect.y -= _position.y;
                this.Build(isClipped, clipRect);
            } else
                this.Build(false, new Rect());
            if (box != null) {
                if (s_isDirectX)
                    box.center += new Vector3(this.RenderOptions.HPixelAlign ? 0.5f : 0f, this.RenderOptions.VPixelAlign ? -0.5f : 0f, 0f);
                box.enabled = true;
            }
            _isBuilt = true;
        }

        void Start () {
            if (!_isBuilt)
                this.Invalidate();
        }

        void Update () {
            if (this.RenderOptions.Billboard == BillboardType.Screen)
                this.UpdatePosition();
            if (!_isBuilt)
                this.Invalidate();
            if (_iupdate != null)
                _iupdate.Update(Time.deltaTime);
        }

        void OnDestroy () {
            _isDisposed = true;
            var idisp = this.Control as IDisposable;
            if (idisp != null)
                idisp.Dispose();
        }

        internal static void CreateQuad (Rect xy, UVRect uv, bool isClipped, Rect clip, List<Vector3> vertices, List<Vector2> uvs, List<int> triangles) {
            if (isClipped) {
                Rect uvRect = uv.SubRect;

                if (clip.xMin > xy.xMax || clip.xMax < xy.xMin ||
                    clip.yMin > xy.yMax || clip.yMax < xy.yMin) {
                    return;
                }
                if (clip.xMin > xy.xMin) {
                    uvRect.xMin += uvRect.width * (clip.xMin - xy.xMin) / xy.width;
                    xy.xMin = clip.xMin;
                }
                if (clip.xMax < xy.xMax) {
                    uvRect.xMax -= uvRect.width * (xy.xMax - clip.xMax) / xy.width;
                    xy.xMax = clip.xMax;
                }
                if (clip.yMin > xy.yMin) {
                    uvRect.yMin += uvRect.height * (clip.yMin - xy.yMin) / xy.height;
                    xy.yMin = clip.yMin;
                }
                if (clip.yMax < xy.yMax) {
                    uvRect.yMax -= uvRect.height * (xy.yMax - clip.yMax) / xy.height;
                    xy.yMax = clip.yMax;
                }

                uv.SubRect = uvRect;
            }

            int i = vertices.Count;
            vertices.AddRange(new[] {
                new Vector3(xy.xMin, xy.yMin, 0f),
                new Vector3(xy.xMax, xy.yMin, 0f),
                new Vector3(xy.xMin, xy.yMax, 0f),
                new Vector3(xy.xMax, xy.yMax, 0f)
            });
            uvs.AddRange(new[] {
                uv.XMinYMin,
                uv.XMaxYMin,
                uv.XMinYMax,
                uv.XMaxYMax
            });
            triangles.AddRange(new[] { i, i + 2, i + 1, i + 3, i + 1, i + 2 });
        }
    }
}
