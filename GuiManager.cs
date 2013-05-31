//     GuiManager.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using Tabletop.io.Linq;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class GuiManager : MonoBehaviour {
        const int MaxTouchFingers = 5;

        Dictionary<string, Layer> _layers;
        Dictionary<string, Atlas> _atlases;
        Dictionary<string, BitmapFont> _fonts;
        Dictionary<string, Shader> _shaders;
        StringCache _strings;
        IKeyboardInput _focus;
        Image[] _cursors;
        Vector3[] _offsets;
        HashSet<IUpdate> _updateHooks;
        List<IUpdate> _tmpHooks;
        List<object> _suppressInputTags;
        bool _isInputEnabled;
        SpriteVisual[] _touches;

        internal GuiManager () {
            _layers = new Dictionary<string, Layer>();
            _atlases = new Dictionary<string, Atlas>();
            _fonts = new Dictionary<string, BitmapFont>();
            _shaders = new Dictionary<string, Shader>();
            _strings = new StringCache();
            _cursors = new Image[3];
            _offsets = new Vector3[3];
            _updateHooks = new HashSet<IUpdate>();
            _tmpHooks = new List<IUpdate>();
            _suppressInputTags = new List<object>();
            _touches = new SpriteVisual[MaxTouchFingers];
            this.IsInputEnabled = true;
        }

        public StringCache Strings { get { return _strings; } }

        public bool IsInputEnabled {
            get {
                if (_isInputEnabled) {
                    foreach (var tag in _suppressInputTags)
                        if (Transition.Any(tag)) {
                            return false;
                        }
                    return true;
                }
                return false;
            }
            set {
                _isInputEnabled = value;
            }
        }

        public bool IsMouseSimulated { get; set; }

        public void AddResources (AssetBundle bundle) {
            var textures = bundle.LoadAll(typeof(Texture2D)).OfType<Texture2D>().ToDictionary(t => t.name);

            foreach (var asset in bundle.LoadAll(typeof(TextAsset)).OfType<TextAsset>()) {
                // this is really the best we can do for now
                if (asset.text.StartsWith("atlas\n") || asset.text.StartsWith("atlas\r\n"))
                    _atlases[asset.name] = new Atlas(asset, textures);
                else if (asset.text.StartsWith("info "))
                    _fonts[asset.name] = new BitmapFont(asset, textures);
                else if (asset.text.StartsWith("strings\n") || asset.text.StartsWith("strings\r\n"))
                    _strings.AddStrings(asset.text);
            }

            foreach (var asset in bundle.LoadAll(typeof(Shader)).OfType<Shader>()) {
                _shaders.Add(asset.name, asset);
            }
        }

        public int AddLayer (string name, Camera camera) {
            if (_layers.ContainsKey(name)) {
                _layers[name].Camera = camera;
                return _layers[name].Index;
            }

            for (int index = 8; index < 32; index++) {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(index)) && !_layers.Any(l => l.Value.Index == index)) {
                    var layer = new Layer(this, name, index, camera);
                    _layers.Add(name, layer);
                    return layer.Index;
                }
            }
            throw new InvalidOperationException("There are no available layers.");
        }

        internal Layer GetLayer (string name) {
            Layer layer;
            if (!_layers.TryGetValue(name, out layer)) {
                throw new ArgumentException(string.Format("Layer not found: {0}", name));
            }
            return layer;
        }

        internal Atlas GetAtlas (string name) {
            Atlas atlas;
            if (!_atlases.TryGetValue(name, out atlas))
                throw new ArgumentException(string.Format("Atlas not found: {0}", name));
            return atlas;
        }

        internal BitmapFont GetFont (string name) {
            BitmapFont font;
            if (!_fonts.TryGetValue(name, out font))
                throw new ArgumentException(string.Format("Font not found: {0}", name));
            return font;
        }

        internal Sprite GetSprite (string spritePath) {
            var parts = spritePath.Split(new[] {'/'}, 2);
            if (parts.Length < 2)
                throw new ArgumentException("Invalid sprite path.");
            var atlas = this.GetAtlas(parts[0]);
            return atlas.GetSprite(parts[1]);
        }

        internal Shader GetShader (string name) {
            Shader shader;
            if (!_shaders.TryGetValue(name, out shader))
                throw new ArgumentException(string.Format("Shader not found: {0}", name));
            return shader;
        }

        public void SetFocus (IKeyboardInput control) {
            if (_focus == control)
                return;
            if (_focus != null)
                _focus.OnLostFocus();
            _focus = control;
            if (_focus != null)
                _focus.OnGotFocus();
        }

        public void SetCursor (CursorPriority priority, Cursor cursor) {
            var i = (int)priority;
            if (cursor == null) {
                if (_cursors[i] != null) {
                    _cursors[i].Dispose();
                    _cursors[i] = null;
                }
                var next = _cursors.FirstOrDefault(c => c != null);
                if (next != null)
                    next.Visual.gameObject.active = true;
                else
                    Screen.showCursor = true;
            } else {
                var sprite = this.GetSprite(cursor.Sprite);
                var img = new Image(
                    Vector3.zero,
                    sprite.Size,
                    cursor.Sprite,
                    new VisualOptions {
                        Name = "default_cursor",
                        PixelAlign = true
                    });
                Vector2 offset = cursor.Offset;
                offset.y -= img.Size.y;
                _offsets[i] = new Vector3(offset.x, offset.y, 0f);
                foreach (var c in _cursors)
                    if (c != null)
                        c.Visual.gameObject.active = false;
                _cursors[i] = img;
                Screen.showCursor = false;
            }
        }

        public void AddUpdateHook (IUpdate hook) {
            _updateHooks.Add(hook);
        }

        public bool RemoveUpdateHook (IUpdate hook) {
            return _updateHooks.Remove(hook);
        }

        public void SuppressInput (object tag) {
            _suppressInputTags.Add(tag);
        }

        void Awake () {
            float hw = Screen.width / 2f, hh = Screen.height / 2f;
            var cam = this.gameObject.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = hh;
            cam.clearFlags = CameraClearFlags.Depth;
            cam.transform.position = new Vector3(hw, hh, -1);
            this.AddLayer("gui", cam);
            var layer = this.GetLayer("gui");
            cam.cullingMask = 1 << layer.Index;
            _layers.Add("main", new Layer(this, "main", LayerMask.NameToLayer("Default"), Camera.main));
        }

        void OnLevelWasLoaded (int level) {
            _updateHooks.Clear();
        }

        void Update () {
            // custom cursors - use highest priority one
            for (var i = 0; i < _cursors.Length; i++) {
                if (_cursors[i] != null) {
                    var pos = Input.mousePosition;
                    _cursors[i].Visual.transform.position = pos + _offsets[i];
                }
            }

            // process input if it's not disabled or suppressed
            if (this.IsInputEnabled) {
                // handle keyboard input
                if (_focus != null) {
                    foreach (char ch in Input.inputString) {
                        _focus.OnKeyPress(ch);
                    }
                }

                // mouse wheel
                var wheel = Input.GetAxis("Mouse ScrollWheel");
                if (wheel != 0f) {
                    var visual = this.GetVisualAtPoint(Input.mousePosition);
                    if (visual != null)
                        visual.OnMouseWheel(wheel);
                }

                // simulate mouse events using touch
                if (this.IsMouseSimulated && Input.touchCount > 0) {
                    foreach (var touch in Input.touches)
                        this.HandleTouch(touch);
                }
            }

            // process update hooks / transitions
            _tmpHooks.AddRange(_updateHooks);
            foreach (var hook in _tmpHooks)
                hook.Update(Time.deltaTime);
            _tmpHooks.Clear();
        }
        
        void OnGUI () {
            if (Event.current.type == UnityEngine.EventType.KeyDown && _focus != null)
                _focus.OnKeyPress(Event.current.keyCode);
        }

        // simulate mouse events using the touch api
        void HandleTouch (Touch touch) {
            // no change, no care
            // also, don't track more than a certain number of fingers
            if (touch.phase == TouchPhase.Stationary || touch.fingerId >= _touches.Length)
                return;

            SpriteVisual visual;
            switch (touch.phase) {
            case TouchPhase.Began: // mousedown: associate new touch with the visual
                Vector3 hitPoint;
                visual = GetVisualAtPoint(touch.position, out hitPoint);
                if (visual != null) {
                    _touches[touch.fingerId] = visual;
                    visual.OnMouseDown(hitPoint);
                }
                break;
            case TouchPhase.Ended:
            case TouchPhase.Canceled: // mouseup: delete association
                visual = _touches[touch.fingerId];
                if (visual != null) {
                    visual.OnMouseUp();
                    _touches[touch.fingerId] = null;
                }
                break;
            case TouchPhase.Moved: // mousedrag
                visual = _touches[touch.fingerId];
                if(visual != null)
                    visual.OnMouseDrag(touch.deltaPosition);
                break;
            }
        }

        SpriteVisual GetVisualAtPoint (Vector2 pos, out Vector3 hitPoint) {
            SpriteVisual visual = null;
            hitPoint = Vector3.zero;

            foreach (var layer in _layers.Values) {
                if (layer.Camera != null) {
                    var ray = camera.ScreenPointToRay(pos);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit) && hit.collider != null) {
                        visual = hit.collider.GetComponent<SpriteVisual>();
                        if (visual != null) {
                            hitPoint = hit.point;
                            break;
                        }
                    }
                }
            }
            return visual;
        }

        SpriteVisual GetVisualAtPoint (Vector2 pos) {
            Vector3 dummy;
            return this.GetVisualAtPoint(pos, out dummy);
        }
    }
}
