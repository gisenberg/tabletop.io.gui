//     TextVisual.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class TextVisual : Visual {
        Rect[] _charBoxes;
        ITextContent _itext;
        Rect _bounds;

        public override Rect Bounds {
            get {
                var pos = this.Position;
                return new Rect(_bounds.x + pos.x, _bounds.y + pos.y, _bounds.width, _bounds.height);
            }
        }

        public int GetCharacterAt (Vector3 pos) {
            if (_charBoxes == null)
                this.Invalidate();
            else if (_charBoxes.Length == 0)
                return -1;

            pos -= this.Position;
            if (_charBoxes == null || _charBoxes.Length < 1 || pos.y > _charBoxes[0].yMax || pos.y < _charBoxes[_charBoxes.Length - 1].yMin)
                return -1;
            int i, spill = -1;
            for (i = 0; i < _charBoxes.Length; i++) {
                if (_charBoxes[i].width <= 0)
                    continue;
                if (pos.y >= _charBoxes[i].yMin && pos.y <= _charBoxes[i].yMax) {
                    if (pos.x < _charBoxes[i].xMin)
                        break;
                    spill = i;
                    if (pos.x >= _charBoxes[i].xMin && pos.x <= _charBoxes[i].xMax)
                        break;
                }
            }
            if (i >= _charBoxes.Length)
                i = spill;
            return i;
        }

        public Rect GetCharacterBounds (int index) {
            if (_charBoxes == null)
                this.Invalidate();
            
            if (_charBoxes.Length == 0 || index < 0 || index >= _charBoxes.Length)
                return new Rect();
            else {
                var r = _charBoxes[index];
                var pos = this.Position;
                r.x += pos.x;
                r.y += pos.y;
                return r;
            }
        }

        protected override void Attach (IControl control) {
            _itext = control as ITextContent;
        }

        protected override void Clear () {
            foreach (var run in this.GetComponentsInChildren<MeshRenderer>()) {
                var filter = run.GetComponent<MeshFilter>();
                filter.mesh.Clear();
                GameObject.Destroy(filter.mesh);
                GameObject.Destroy(run);
            }
        }

        protected override void Build (bool isClipped, Rect clipRect) {
            if (_itext == null)
                return;

            var text = _itext.Text ?? "";
            var size = this.Size;

            int i = 0;
            TextRun run = null;
            var lines = new List<List<TextRun>>();
            var line = new List<TextRun>();
            var nestedRuns = new Stack<TextRun>();
            var nestedTags = new Stack<string>();
            var font = Gui.Instance.GetFont(_itext.Font);
            lines.Add(line);
            run = new TextRun(font, i, _itext.Kerning, this.Color);
            line.Add(run);

            while (i < text.Length) {
                if (char.IsLowSurrogate(text, i)) {
                    i++;
                    continue;
                }

                var c = char.ConvertToUtf32(text, i++);
                if (c == '\n') {
                    run.End = i;
                    line = new List<TextRun>();
                    lines.Add(line);
                    run = new TextRun(run, i);
                    line.Add(run);
                    continue;
                } else if (c < 32)
                    continue;
                else if (c == '[') {
                    var tagEnd = text.IndexOf(']', i);
                    if (tagEnd > 0 && tagEnd > i) {
                        var tag = text.Substring(i, tagEnd - i).ToUpper();
                        switch (tag) {
                        case "B":
                            nestedTags.Push(tag);
                            nestedRuns.Push(run);
                            run = new TextRun(run, tagEnd + 1, Gui.Instance.GetFont(_itext.BoldFont ?? _itext.Font));
                            line.Add(run);
                            i = tagEnd + 1;
                            continue;
                        case "/B":
                            if (nestedTags.Count > 0 && nestedTags.Peek() == "B") {
                                run = new TextRun(nestedRuns.Pop(), tagEnd + 1);
                                line.Add(run);
                                nestedTags.Pop();
                                i = tagEnd + 1;
                                continue;
                            }
                            break;
                        }
                    }
                }

                var ch = run.Font.GetCharacter(c);
                run.Push(ch);
                if (_itext.Overflow == Overflow.Wrap && line.Sum(tr => tr.Width) + (lines.Count > 1 ? _itext.Indent : 0f) > size.x) {
                    var back = run.Break();
                    if (i - back <= run.Start) {
                        if (line.Count > 1) {
                            line.RemoveAt(line.Count - 1);
                            line = new List<TextRun>();
                            lines.Add(line);
                            line.Add(run);
                            continue;
                        } else
                            back = 1;
                    } else
                        back--;
                    i -= back;
                    run.Pop(back);
                    line = new List<TextRun>();
                    lines.Add(line);
                    run = new TextRun(run, i);
                    line.Add(run);
                } else if (_itext.Overflow == Overflow.Hidden && line.Sum(tr => tr.Width) > size.x) {
                    run.Pop(1);
                    break;
                }
            }
            run.End = i;

            var actualWidth = lines.Max(l => l.Sum(tr => tr.Width));
            var actualHeight = lines.Sum(l => l.Max(tr => tr.Height) + _itext.Leading);
            var y = 0f;
            switch (_itext.VerticalAlignment) {
            case VerticalAlignment.Top:
                y = size.y;
                break;
            case VerticalAlignment.Center:
                y = Mathf.Round(size.y - (size.y - actualHeight) / 2f);
                break;
            case VerticalAlignment.Bottom:
                y = actualHeight;
                break;
            }
            _bounds = new Rect(float.MaxValue, y - actualHeight, actualWidth, actualHeight);

            var hitTest = _itext.IsHitTestable;
            if (hitTest)
                _charBoxes = new Rect[text.Length];
            else
                _charBoxes = new Rect[0];

            var vertexOffset = Vector3.zero;
            if (this.RenderOptions.AnchorTo == AnchorTo.Center) {
                vertexOffset = new Vector3(-size.x / 2f, -size.y / 2f, 0);
                clipRect.x -= size.x / 2f;
                clipRect.y -= size.y / 2f;
            }

            for (i = 0; i < lines.Count; i++) {
                var tl = lines[i];
                var x = 0f + (i > 0 ? _itext.Indent : 0f);
                switch (_itext.HorizontalAlignment) {
                case HorizontalAlignment.Center:
                    x = Mathf.Round((size.x - tl.Sum(tr => tr.Width)) / 2f);
                    break;
                case HorizontalAlignment.Right:
                    x = size.x - tl.Sum(tr => tr.Width);
                    break;
                }
                if (x < _bounds.x)
                    _bounds.x = x;
                y -= tl.Max(tr => tr.Height) + _itext.Leading;
                foreach(var tr in tl) {
                    tr.Build(gameObject, new Vector2(x, y - (font.Base - tr.Font.Base)), isClipped, clipRect, hitTest ? _charBoxes : null, vertexOffset);
                    x += tr.Width;
                }
            }
        }

        protected override void UpdateColorFast (Color c) {
            foreach (var mf in this.GetComponentsInChildren<MeshFilter>(true)) {
                if (mf.gameObject.name == "run") {
                    var colors = new Color[mf.mesh.vertices.Length];
                    for (var i = 0; i < colors.Length; i++)
                        colors[i] = c;
                    mf.mesh.colors = colors;
                }
            }
        }
    }
}
