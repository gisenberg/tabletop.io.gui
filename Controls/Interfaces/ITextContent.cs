//     ITextContent.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.

namespace Tabletop.io.Gui {
    public interface ITextContent {
        string Text { get; }
        string Font { get; }
        string BoldFont { get; }
        HorizontalAlignment HorizontalAlignment { get; }
        VerticalAlignment VerticalAlignment { get; }
        Overflow Overflow { get; }
        float Indent { get; }
        float Kerning { get; }
        float Leading { get; }
        bool IsHitTestable { get; }
        bool UseBBCode { get; }
    }
}
