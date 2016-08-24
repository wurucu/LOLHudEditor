using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFLolHUDEditor
{
    public partial class WElement : Image
    { 
        public enum ETip
        {
            Icon,
            Text
        }
        public ETip Tip { get; set; }
        public int Size { get; set; }
        public string ElementName { get; set; }
        public string TColor { get; set; }
 
        public int ElementID { get; set; }

        protected override void OnRender(DrawingContext dc)
        {
            if (Selected)
                dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Red, 1), new Rect(0, 0, this.Width, this.Height));
            base.OnRender(dc);
        }

        public bool Selected { get; set; }

        public void setSelected(bool v)
        {
            this.Selected = v;
            this.InvalidateVisual();
        }

        
    }
}
