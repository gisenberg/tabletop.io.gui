//     Enums.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

using System;

namespace Tabletop.io.Gui {
    public enum HorizontalAlignment {
        Left,
        Right,
        Center
    }

    public enum VerticalAlignment {
        Top,
        Bottom,
        Center
    }

    public enum AnchorTo {
        Corner,
        Center
    }

    public enum BillboardType {
        None,
        Screen
    }

    [Flags]
    public enum RotateFlip {
        None = 0x0,

        Rotate90CW = 0x1,
        Rotate180 = 0x2,
        Rotate270CW = 0x4,

        Rotate90CCW = Rotate270CW,
        Rotate270CCW = Rotate90CW,

        FlipH = 0x8,
        FlipV = 0x10,

        FlipHV = Rotate180,
    }

    public enum Overflow {
        Visible,
        Hidden,
        Wrap
    }
}
