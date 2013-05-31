//     TextRun.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    class TextRun {
        float _kerning;
        Color _color;
        Stack<BitmapChar> _chars;

        public BitmapFont Font { get; private set; }
        public int Start { get; private set; }
        public int End { get; set; }
        public float Width { get; private set; }
        public float Height { get { return this.Font.LineHeight; } }

        public TextRun (BitmapFont font, int start, float kerning, Color color) {
            this.Font = font;
            this.Start = start;
            _kerning = kerning;
            _color = color;
            _chars = new Stack<BitmapChar>();
        }

        public TextRun (TextRun other, int start)
            : this(other.Font, start, other._kerning, other._color) {
        }

        public TextRun (TextRun other, int start, BitmapFont font)
            : this(font, start, other._kerning, other._color) {
        }

        public float GetWidth (BitmapChar ch) {
            return (_chars.Count > 0 ? ch.GetKerning(_chars.Peek().Id) : 0f) + ch.Advance + _kerning;
        }

        public void Push (BitmapChar ch) {
            _chars.Push(ch);
            this.Width += this.GetWidth(ch);
        }

        public void Pop (int num) {
            for (; num >= 0; --num) {
                var ch = _chars.Pop();
                this.Width -= this.GetWidth(ch);
            }
        }

        public int Break () {
            int i = 0;
            foreach (var ch in _chars) {
                i++;
                if (char.IsWhiteSpace((char)ch.Id) && ch.Id != 0x00a0 && ch.Id != 0x202f)
                    break;
            }
            return i;
        }

        public void Build (GameObject parent, Vector2 pos, bool isClipped, Rect clip, Rect[] bounds, Vector2 vertexOffset) {
            if (this.Width == 0)
                return;

            var go = new GameObject("run");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = this.Font.GetMaterial(0);

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var chars = _chars.ToArray();
            var x = pos.x;
            var idx = this.Start;
            for (var i = chars.Length - 1; i >= 0; i--) {
                var ch = chars[i];
                x += i < chars.Length - 1 ? ch.GetKerning(chars[i + 1].Id) : 0f;
                Visual.CreateQuad(
                    new Rect(x + ch.Offset.x + vertexOffset.x, pos.y + ch.Offset.y + vertexOffset.y, ch.Size.x, ch.Size.y),
                    ch.UV,
                    isClipped, clip,
                    vertices, uvs, triangles
                    );
                if (bounds != null)
                    bounds[idx++] = new Rect(x, pos.y, this.GetWidth(ch), this.Font.LineHeight);
                x += ch.Advance + _kerning;
            }
            var m = mf.mesh;
            m.vertices = vertices.ToArray();
            m.uv = uvs.ToArray();
            m.triangles = triangles.ToArray();
            m.normals = vertices.Select(v => new Vector3(0f, 0f, -1f)).ToArray();
            m.colors = vertices.Select(v => _color).ToArray();
            m.RecalculateBounds();

            go.layer = parent.layer;
            go.transform.parent = parent.transform;
            go.transform.position = parent.transform.position;
            go.transform.localScale = Vector3.one;
        }
    }
}
