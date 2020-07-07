using System;
using System.Collections.Generic;
using System.Drawing;

namespace Parse.FrontEnd.Support.Drawing
{
    public class HighlightMapItem
    {
        public Type Type { get; }

        public Color ForegroundColor { get; set; } = Color.Transparent;
        public Color BackgroundColor { get; set; } = Color.Transparent;

        public HighlightMapItem(Type type, Color foregroundColor, Color backgroundColor)
        {
            Type = type;
            ForegroundColor = foregroundColor;
            BackgroundColor = backgroundColor;
        }
    }

    public class HighlightMap : List<HighlightMapItem>
    {
        public HighlightMapItem GetItem(Type type)
        {
            HighlightMapItem result = null;

            this.ForEach(target => 
            {
                if (target.Type == type)
                {
                    result = target;
                    return;
                }
            });

            return result;
        }
    }
}
