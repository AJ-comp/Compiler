﻿using Parse.WpfControls.Common;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Parse.WpfControls.Models
{
    public class HighlightToken : FormattedText
    {
        public AppearanceInfo AppearanceInfo { get; } = new AppearanceInfo();

        public HighlightToken(string textToFormat, CultureInfo culture, FlowDirection flowDirection, Typeface typeface, double emSize, Brush foreground, double pixelsPerDip)
            : base(textToFormat, culture, flowDirection, typeface, emSize, foreground, pixelsPerDip)
        {
        }

        public override string ToString() => this.Text;
    }


    public class LineHighlightText : List<HighlightToken>
    {
        public int AbsoluteLineIndex { get; internal set; }

        public override string ToString()
        {
            string result = string.Empty;

            foreach(var item in this)
            {
                result += item.Text;
            }

            return string.Format(result);
        }
    }


    public class AppearanceInfo
    {
        public Brush BorderBrush { get; set; } = Brushes.Transparent;
        public Brush BackGroundBrush { get; set; } = Brushes.Transparent;
        public int BorderThickness { get; set; } = 1;

        public bool Selected { get; set; }
        public bool UnderLine { get; set; }
    }
}
