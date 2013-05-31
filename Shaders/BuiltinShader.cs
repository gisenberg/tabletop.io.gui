//     BuiltInShader.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System.IO;
using UnityEngine;

namespace Tabletop.io.Gui {
    public static class BuiltinShader {
        static Material _spriteVertexColored;

        public static Material CreateSpriteVertexColored () {
            if (_spriteVertexColored == null)
                _spriteVertexColored = new Material(GetSource("SpriteVertexColored"));
            return new Material(_spriteVertexColored);
        }

        public static string GetSource (string shaderName) {
            var stream = typeof(BuiltinShader).Assembly.GetManifestResourceStream(typeof(BuiltinShader), string.Format("Shaders.{0}.txt", shaderName));
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
