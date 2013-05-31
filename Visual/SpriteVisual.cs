//     SpriteVisual.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class SpriteVisual : Visual {
        ISpriteContent _isprite;
        IMouseInput _imouse;
        ICustomHitBox _ihitbox;
        ICustomCursor _icursor;
        Vector3 _oldMouse;

        protected override void Attach (IControl control) {
            _isprite = control as ISpriteContent;
            _imouse = control as IMouseInput;
            _ihitbox = control as ICustomHitBox;
            _icursor = control as ICustomCursor;
            if (_imouse != null || _ihitbox != null)
                gameObject.AddComponent<BoxCollider>();
        }

        protected override void Clear () {
            var mf = this.GetComponent<MeshFilter>();
            mf.mesh.Clear();
        }

        protected override void Build (bool isClipped, Rect clipRect) {
            var mf = this.GetComponent<MeshFilter>();
            var mr = this.GetComponent<MeshRenderer>();

            var size = this.Size;
            var hasMesh = false;
            if (_isprite != null) {
                var spritePath = _isprite.Sprite;
                if (spritePath != null) {
                    var sprite = Gui.Instance.GetSprite(spritePath);
                    CreateMesh(mf.mesh, sprite, size, this.Color, isClipped, clipRect, !this.RenderOptions.VPixelAlign, !this.RenderOptions.HPixelAlign,
                        this.RenderOptions.TileOffset, this.RenderOptions.RotateFlip, this.RenderOptions.AnchorTo);
                    mr.material = sprite.GetMaterial(this.RenderOptions.Shader);
                    hasMesh = true;
                }
            }
            var box = collider as BoxCollider;
            if (box != null) {
                if (_ihitbox != null) {
                    var r = _ihitbox.HitBox;
                    r.x -= transform.position.x;
                    r.y -= transform.position.y;
                    box.center = new Vector3(r.xMin + r.width / 2f, r.yMin + r.height / 2f, 0f);
                    box.size = new Vector3(r.width, r.height, 0f);
                } else if (hasMesh) {
                    var bounds = mf.mesh.bounds;
                    box.center = bounds.center;
                    box.size = new Vector3(
                        this.RenderOptions.HPixelAlign ? Mathf.Round(bounds.size.x) : bounds.size.x,
                        this.RenderOptions.VPixelAlign ? Mathf.Round(bounds.size.y) : bounds.size.y
                        );
                } else {
                    box.size = size;
                    if (this.RenderOptions.AnchorTo == AnchorTo.Center)
                        box.center = Vector3.zero;
                    else
                        box.center = new Vector3(
                            this.RenderOptions.HPixelAlign ? Mathf.Round(size.x / 2) : size.x / 2,
                            this.RenderOptions.VPixelAlign ? Mathf.Round(size.y / 2) : size.y / 2,
                            0);
                }
            }
        }

        void Awake () {
            this.gameObject.AddComponent<MeshFilter>();
            this.gameObject.AddComponent<MeshRenderer>();
        }

        void OnMouseEnter () {
            foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                handler.OnMouseEnter(this.Control);
            if (_icursor != null)
                Gui.SetCursor(CursorPriority.Context, _icursor.Cursor);
        }

        void OnMouseExit () {
            foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                handler.OnMouseExit(this.Control);
            if (_icursor != null)
                Gui.SetCursor(CursorPriority.Context, null);
        }

        void OnMouseDown () {
            if (Gui.IsInputEnabled) {
                RaycastHit hit;
                var ray = this.Layer.Camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                        handler.OnMouseDown(this.Control, hit.point);
                }
            }
            _oldMouse = Input.mousePosition;
        }

        internal void OnMouseDown (Vector3 hitPoint) {
            foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                handler.OnMouseDown(this.Control, hitPoint);
        }

        internal void OnMouseUp () {
            if (Gui.IsInputEnabled) {
                foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                    handler.OnMouseUp(this.Control);
            }
        }

        void OnMouseDrag () {
            if (Gui.IsInputEnabled) {
                var delta = Input.mousePosition - _oldMouse;
                foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                    handler.OnMouseDrag(this.Control, delta);
            }
            _oldMouse = Input.mousePosition;
        }

        internal void OnMouseDrag (Vector2 delta) {
            foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                handler.OnMouseDrag(this.Control, delta);
        }

        internal void OnMouseWheel (float delta) {
            if (Gui.IsInputEnabled) {
                foreach (var handler in this.GetBubbleTargets<IMouseInput>())
                    handler.OnMouseWheel(this.Control, delta);
            }
        }

        static void CreateMesh (Mesh m, Sprite sprite, Vector2 size, Color color, bool isClipped, Rect clip,
            bool hcomp, bool vcomp, Vector2 tileOffset, RotateFlip rotateFlip, AnchorTo anchorTo) {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float hbleed = hcomp ? 0.5f : 0f, vbleed = vcomp ? 0.5f : 0f;
            Vector4 border = sprite.Border;
            Vector2 texelSize = new Vector2(sprite.TexelWidth, sprite.TexelHeight);;
            bool tileH = sprite.TileHorizontal;
            bool tileV = sprite.TileVertical;

            if (anchorTo == AnchorTo.Center) {
                clip.x -= size.x / 2f;
                clip.y -= size.y / 2f;
            }

            if (rotateFlip != RotateFlip.None && (border.sqrMagnitude != 0 || tileH || tileV))
                throw new NotImplementedException("Rotation and flipping are not implemented for 9slice sprites!");

            var outer = Rect.MinMaxRect(0f, 0f, size.x, size.y);
            var inner = Rect.MinMaxRect(
                border.x,
                border.y,
                size.x - border.z,
                size.y - border.w
            );
            var outerUV = Rect.MinMaxRect(
                sprite.UV.xMin + (sprite.Border.x > 0f ? hbleed : 0f) * sprite.TexelWidth,
                sprite.UV.yMin + (sprite.Border.y > 0f ? vbleed : 0f) * sprite.TexelHeight,
                sprite.UV.xMax - (sprite.Border.z > 0f ? hbleed : 0f) * sprite.TexelWidth,
                sprite.UV.yMax - (sprite.Border.w > 0f ? vbleed : 0f) * sprite.TexelHeight
            );
            var innerUV = Rect.MinMaxRect(
                sprite.UV.xMin + (sprite.Border.x > 0f ? sprite.Border.x : hbleed) * sprite.TexelWidth,
                sprite.UV.yMin + (sprite.Border.y > 0f ? sprite.Border.y : vbleed) * sprite.TexelHeight,
                sprite.UV.xMax - (sprite.Border.z > 0f ? sprite.Border.z : hbleed) * sprite.TexelWidth,
                sprite.UV.yMax - (sprite.Border.w > 0f ? sprite.Border.w : vbleed) * sprite.TexelHeight
            );

            // lower left corner
            if (border.x > 0f && border.y > 0f) {
                CreateQuad(
                    Rect.MinMaxRect(outer.xMin, outer.yMin, inner.xMin, inner.yMin),
                    new UVRect(outerUV, Rect.MinMaxRect(outerUV.xMin, outerUV.yMin, innerUV.xMin, innerUV.yMin), rotateFlip),
                    isClipped, clip,
                    vertices, uvs, triangles
                    );
            }
            // lower right corner
            if (border.z > 0f && border.y > 0f) {
                CreateQuad(
                    Rect.MinMaxRect(inner.xMax, outer.yMin, outer.xMax, inner.yMin),
                    new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMax, outerUV.yMin, outerUV.xMax, innerUV.yMin), rotateFlip),
                    isClipped, clip,
                    vertices, uvs, triangles
                    );
            }
            // upper left corner
            if (border.x > 0f && border.w > 0f) {
                CreateQuad(
                    Rect.MinMaxRect(outer.xMin, inner.yMax, inner.xMin, outer.yMax),
                    new UVRect(outerUV, Rect.MinMaxRect(outerUV.xMin, innerUV.yMax, innerUV.xMin, outerUV.yMax), rotateFlip),
                    isClipped, clip,
                    vertices, uvs, triangles
                    );
            }
            // upper right corner
            if (border.z > 0f && border.w > 0f) {
                CreateQuad(
                    Rect.MinMaxRect(inner.xMax, inner.yMax, outer.xMax, outer.yMax),
                    new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMax, innerUV.yMax, outerUV.xMax, outerUV.yMax), rotateFlip),
                    isClipped, clip,
                    vertices, uvs, triangles
                    );
            }

            // left and right sides
            if (tileV) {
                var tHeight = innerUV.height / texelSize.y;
                var tOffset = tileOffset.y < 0f ? tHeight + tileOffset.y : tileOffset.y;

                var from = inner.yMin;
                var to = Math.Min(inner.yMin + (tHeight - tOffset), inner.yMax);
                var fromUV = innerUV.yMin + tOffset * texelSize.y;
                while (from < inner.yMax) {
                    var toUV = fromUV + (to - from) * texelSize.y;
                    // left
                    if (border.x > 0f) {
                        CreateQuad(
                            Rect.MinMaxRect(outer.xMin, from, inner.xMin, to),
                            new UVRect(outerUV, Rect.MinMaxRect(outerUV.xMin, fromUV, innerUV.xMin, toUV), rotateFlip),
                            isClipped, clip,
                            vertices, uvs, triangles
                            );
                    }
                    // right
                    if (border.z > 0f) {
                        CreateQuad(
                            Rect.MinMaxRect(inner.xMax, from, outer.xMax, to),
                            new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMax, fromUV, outerUV.xMax, toUV), rotateFlip),
                            isClipped, clip,
                            vertices, uvs, triangles
                            );
                    }
                    // middle
                    if (!sprite.IsHollow) {
                        if (!tileH) {
                            CreateQuad(
                                Rect.MinMaxRect(inner.xMin, from, inner.xMax, to),
                                new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMin, fromUV, innerUV.xMax, toUV), rotateFlip),
                                isClipped, clip,
                                vertices, uvs, triangles
                                );
                        } else {
                            // awkward nested case here, tile the middle both ways
                            var tWidth = innerUV.width / texelSize.x;
                            var tOffsetW = tileOffset.x < 0f ? tWidth + tileOffset.x : tileOffset.x;

                            var fromW = inner.xMin;
                            var toW = Math.Min(inner.xMin + (tWidth - tOffsetW), inner.xMax);
                            var fromUVW = innerUV.xMin + tOffsetW * texelSize.x;
                            while (fromW < inner.xMax) {
                                var toUVW = fromUVW + (toW - fromW) * texelSize.x;
                                CreateQuad(
                                    Rect.MinMaxRect(fromW, from, toW, to),
                                    new UVRect(outerUV, Rect.MinMaxRect(fromUVW, fromUV, toUVW, toUV), rotateFlip),
                                    isClipped, clip,
                                    vertices, uvs, triangles
                                    );
                                fromW = toW;
                                toW = Math.Min(toW + tWidth, inner.xMax);
                                fromUVW = innerUV.xMin;
                            }
                        }
                    }
                    from = to;
                    to = Math.Min(to + tHeight, inner.yMax);
                    fromUV = innerUV.yMin;
                }
            } else {
                // left
                if (border.x > 0f) {
                    CreateQuad(
                        Rect.MinMaxRect(outer.xMin, inner.yMin, inner.xMin, inner.yMax),
                        new UVRect(outerUV, Rect.MinMaxRect(outerUV.xMin, innerUV.yMin, innerUV.xMin, innerUV.yMax), rotateFlip),
                        isClipped, clip,
                        vertices, uvs, triangles
                        );
                }
                // right
                if (border.z > 0f) {
                    CreateQuad(
                        Rect.MinMaxRect(inner.xMax, inner.yMin, outer.xMax, inner.yMax),
                        new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMax, innerUV.yMin, outerUV.xMax, innerUV.yMax), rotateFlip),
                        isClipped, clip,
                        vertices, uvs, triangles
                        );
                }
            }

            // top and bottom sides
            if (tileH) {
                var tWidth = innerUV.width / texelSize.x;
                var tOffset = tileOffset.x < 0f ? tWidth + tileOffset.x : tileOffset.x;

                var from = inner.xMin;
                var to = Math.Min(inner.xMin + (tWidth - tOffset), inner.xMax);
                var fromUV = innerUV.xMin + tOffset * texelSize.x;
                while (from < inner.xMax) {
                    var toUV = fromUV + (to - from) * texelSize.x;
                    // bottom
                    if (border.y > 0f) {
                        CreateQuad(
                            Rect.MinMaxRect(from, outer.yMin, to, inner.yMin),
                            new UVRect(outerUV, Rect.MinMaxRect(fromUV, outerUV.yMin, toUV, innerUV.yMin), rotateFlip),
                            isClipped, clip,
                            vertices, uvs, triangles
                            );
                    }
                    // top
                    if (border.w > 0f) {
                        CreateQuad(
                            Rect.MinMaxRect(from, inner.yMax, to, outer.yMax),
                            new UVRect(outerUV, Rect.MinMaxRect(fromUV, innerUV.yMax, toUV, outerUV.yMax), rotateFlip),
                            isClipped, clip,
                            vertices, uvs, triangles
                            );
                    }
                    // middle
                    if (!sprite.IsHollow && !tileV) {
                        CreateQuad(
                            Rect.MinMaxRect(from, inner.yMin, to, inner.yMax),
                            new UVRect(outerUV, Rect.MinMaxRect(fromUV, innerUV.yMin, toUV, innerUV.yMax), rotateFlip),
                            isClipped, clip,
                            vertices, uvs, triangles
                            );
                    }
                    from = to;
                    to = Math.Min(to + tWidth, inner.xMax);
                    fromUV = innerUV.xMin;
                }
            } else {
                // bottom
                if (border.y > 0f) {
                    CreateQuad(
                        Rect.MinMaxRect(inner.xMin, outer.yMin, inner.xMax, inner.yMin),
                        new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMin, outerUV.yMin, innerUV.xMax, innerUV.yMin), rotateFlip),
                        isClipped, clip,
                        vertices, uvs, triangles
                        );
                }

                // top
                if (border.w > 0f) {
                    CreateQuad(
                        Rect.MinMaxRect(inner.xMin, inner.yMax, inner.xMax, outer.yMax),
                        new UVRect(outerUV, Rect.MinMaxRect(innerUV.xMin, innerUV.yMax, innerUV.xMax, outerUV.yMax), rotateFlip),
                        isClipped, clip,
                        vertices, uvs, triangles
                        );
                }
            }

            // middle
            if (!tileH && !tileV && !sprite.IsHollow) {
                CreateQuad(inner, new UVRect(outerUV, innerUV, rotateFlip), isClipped, clip, vertices, uvs, triangles);
            }

            if (anchorTo == AnchorTo.Center) {
                var extents = new Vector3(size.x / 2f, size.y / 2f, 0f);
                m.vertices = vertices.Select(v => v - extents).ToArray();
            } else
                m.vertices = vertices.ToArray();
            m.uv = uvs.ToArray();
            m.triangles = triangles.ToArray();
            m.normals = vertices.Select(v => new Vector3(0f, 0f, -1f)).ToArray();
            m.colors = vertices.Select(v => color).ToArray();

            m.RecalculateBounds();
        }
    }
}
