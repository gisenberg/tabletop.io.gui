//     ListItem.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using UnityEngine;

namespace Tabletop.io.Gui {
    public class ListItem<T> : Button {
        internal ListItem (Rect bounds, IControl parent, RadioGroup group, T data,
            string normalSprite, string overSprite, string activeSprite, bool pixelAlign) :
            base(bounds, Gui.ZIndex(0.5f, parent), new ButtonOptions {
                NormalSprite = normalSprite,
                OverSprite = overSprite,
                ActiveSprite = activeSprite,
                Group = group,
                Name = "list_item",
                PixelAlign = pixelAlign,
                Parent = parent
            }) {
            this.Data = data;
        }

        public new T Data { get; set; }
    }
}
