//     WebView.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Tabletop.io.Client {
    public class WebView : MonoBehaviour, IDisposable {
        IntPtr _instance;

        public Action<string> MessageReceived;

        public void SetVisibility (bool isVisible) {
            _WebView_SetVisibility(_instance, isVisible);
        }

        public void LoadUrl (string url) {
            _WebView_LoadUrl(_instance, url);
        }

        public void Evaluate (string script) {
            _WebView_EvaluateJS(_instance, script);
        }

        public void Dispose () {
            GameObject.Destroy(gameObject);
        }

        void Initialize (Rect frame) {
            _instance = _WebView_Create(gameObject.name, Mathf.RoundToInt(frame.x), Mathf.RoundToInt(frame.y), Mathf.RoundToInt(frame.width), Mathf.RoundToInt(frame.height));
        }

        void OnMessage (string message) {
            if (this.MessageReceived != null)
                this.MessageReceived(message);
        }

        void OnDestroy () {
            _WebView_Destroy(_instance);
        }

        public void SetFrame (Rect frame) {
            _WebView_SetFrame(_instance, Mathf.RoundToInt(frame.x), Mathf.RoundToInt(frame.y), Mathf.RoundToInt(frame.width), Mathf.RoundToInt(frame.height));
        }

        public void SetTransparentBackground (bool transparent) {
            _WebView_SetTransparentBackground(_instance, transparent);
        }

        public void SetOpenLinksInNewWindow (bool setting) {
            _WebView_SetOpenLinksInNewWindow(_instance, setting);
        }

        public static WebView New (Rect frame) {
            var go = new GameObject();
            go.name = "WebView_" + go.GetHashCode().ToString();
            GameObject.DontDestroyOnLoad(go);
            var webView = go.AddComponent<WebView>();
            webView.Initialize(frame);
            return webView;
        }

        [DllImport("__Internal")]
        static extern IntPtr _WebView_Create (string gameObjectName, int x, int y, int width, int height);

        [DllImport("__Internal")]
        static extern void _WebView_Destroy (IntPtr instance);

        [DllImport("__Internal")]
        static extern void _WebView_SetVisibility (IntPtr instance, bool isVisible);

        [DllImport("__Internal")]
        static extern void _WebView_LoadUrl (IntPtr instance, string url);

        [DllImport("__Internal")]
        static extern void _WebView_EvaluateJS (IntPtr instance, string js);

        [DllImport("__Internal")]
        static extern void _WebView_SetFrame (IntPtr instance, int x, int y, int width, int height);

        [DllImport("__Internal")]
        static extern void _WebView_SetTransparentBackground (IntPtr instance, bool transparent);

        [DllImport("__Internal")]
        static extern void _WebView_SetOpenLinksInNewWindow (IntPtr instance, bool setting);
    }
}
