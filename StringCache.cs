//     StringCache.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Tabletop.io.Gui {
    public class StringCache {
        const string AssetName = "strings";
        const string MissingFormat = "Missing: {0}";
        static readonly Regex _separator = new Regex(@"\s+", RegexOptions.Singleline);

        Dictionary<string, string> _strings;

        internal StringCache () {
            _strings = new Dictionary<string, string>();
        }

        public string this[string key] {
            get {
                string value;
                if (!_strings.TryGetValue(key, out value)) {
                    return string.Format(MissingFormat, key);
                } else
                    return value;
            }
        }

        public void AddStrings (string map) {
            var sb = new StringBuilder();
            var lines = map.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for(var j = 0; j < lines.Length; j++) {
                var parts = _separator.Split(lines[j], 2);
                if (parts.Length < 2)
                    continue;
                var key = parts[0].Trim();
                if (_strings.ContainsKey(key)) {
                    Debug.Log(string.Format("Duplicate string key: {0}", parts[0]));
                    continue;
                }
                sb.Length = 0;
                bool esc = false;
                for (var i = 0; i < parts[1].Length; i++) {
                    var c = parts[1][i];
                    if (c == '\\') {
                        esc = true;
                        continue;
                    } else if (esc) {
                        switch (c) {
                        case 'n':
                            sb.Append("\n");
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case 'u':
                            int code;
                            if (i > parts[1].Length - 5 ||
                                !int.TryParse(parts[1].Substring(i + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                                throw new ArgumentException("Invalid escape sequence on line " + j);
                            sb.Append((char)code);
                            i += 4;
                            break;
                        case 'U':
                            int utf32code;
                            if(i > parts[1].Length - 9 ||
                                !int.TryParse(parts[1].Substring(i + 1, 8), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out utf32code))
                                throw new ArgumentException("Invalid escape sequence on line " + j);
                            sb.Append(char.ConvertFromUtf32(utf32code));
                            i += 8;
                            break;
                        default:
                            throw new ArgumentException("Invalid escape sequence on line " + j);
                        }
                        esc = false;
                    } else
                        sb.Append(c);
                }
                _strings.Add(key, sb.ToString().Trim());
            }
        }
    }
}
