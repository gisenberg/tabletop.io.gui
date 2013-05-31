//     Gui.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public static class Gui {
        static GuiManager s_gui;
        public static int ScreenX, ScreenY;

        static Gui () {
            ScreenX = Screen.width;
            ScreenY = Screen.height;
            if (s_gui == null) {
                var go = new GameObject("Gui Manager");
                GameObject.DontDestroyOnLoad(go);
                s_gui = go.AddComponent<GuiManager>();
            }
        }

        public static StringCache Strings { get { return s_gui.Strings; } }

        public static bool IsInputEnabled { get { return s_gui.IsInputEnabled; } set { s_gui.IsInputEnabled = value; } }

        public static bool IsMouseSimulated { get { return s_gui.IsMouseSimulated; } set { s_gui.IsMouseSimulated = value; } }

        internal static GuiManager Instance { get { return s_gui; } }

        public static void AddResources (AssetBundle bundle) {
            s_gui.AddResources(bundle);
        }

        public static int AddLayer (string name, Camera camera) {
            return s_gui.AddLayer(name, camera);
        }

        public static Camera GetCameraFromLayer (string name) {
            return s_gui.GetLayer(name).Camera;
        }

        public static int GetLayerIndex (string name) {
            return s_gui.GetLayer(name).Index;
        }

		public static Layer GetLayer (string name) {
			return s_gui.GetLayer(name);
		}
		
		public static Atlas GetAtlas (string name) {
			return s_gui.GetAtlas(name);
		}
		
		public static BitmapFont GetFont (string name) {
			return s_gui.GetFont(name);
		}
		
		public static Sprite GetSprite (string spritePath) {
			return s_gui.GetSprite(spritePath);
		}

        public static Shader GetShader (string name) {
            return s_gui.GetShader(name);
        }

        public static void SetFocus (IKeyboardInput control) {
            s_gui.SetFocus(control);
        }

        public static void SetCursor (CursorPriority priority, Cursor cursor) {
            s_gui.SetCursor(priority, cursor);
        }

        public static void AddUpdateHook (IUpdate hook) {
            s_gui.AddUpdateHook(hook);
        }

        public static bool RemoveUpdateHook (IUpdate hook) {
            return s_gui.RemoveUpdateHook(hook);
        }

        public static float ZIndex (float zIndex) {
            return 500f - zIndex;
        }

        public static float ZIndex (float zIndex, IControl relativeTo) {
            return relativeTo.Position.z - zIndex;
        }

        public static Vector2 GetSpriteSize (string path) {
            var s = s_gui.GetSprite(path);
            return s.Size;
        }

        public static void SuppressInput (object transitionTag) {
            s_gui.SuppressInput(transitionTag);
        }
    }
}
