using System;
using System.Collections.Generic;
using gdi = System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFLolHUDEditor
{
    public class WPanel : Canvas
    {
        private Point origin;
        private Point start;

        CElementCollection backElements = new CElementCollection();

        //--
        AdornerLayer aLayer;
        bool _isDown;
        bool _isDragging;
        bool selected = false;
        UIElement selectedElement = null;
        Point _startPoint;
        private double _originalLeft;
        private double _originalTop;
        //--

        public delegate void ChangedElement(int ID, Point rec1, Point rec2);
        public event ChangedElement OnChangeElement;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // for resize
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(panel1_PreviewMouseLeftButtonDown);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(panel1_DragFinishedMouseHandler);
            // for drag move
            this.MouseLeftButtonDown += new MouseButtonEventHandler(panel1_MouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(panel1_DragFinishedMouseHandler);
            this.MouseMove += new MouseEventHandler(panel1_MouseMove);
            this.MouseLeave += new MouseEventHandler(panel1_MouseLeave);

            this.MouseWheel += Panel1_MouseWheel;
            TransformGroup group = new TransformGroup();
            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            this.RenderTransform = group;
        }


        private void Panel1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)this.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];
            
            double zoom = e.Delta > 0 ? .2 : -.2;
            if (transform.ScaleX < 0.5 && zoom < 0)
                return;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
        }

        void panel1_MouseLeave(object sender, MouseEventArgs e)
        {
            StopDragging();
            e.Handled = true;
        }
        void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                var tt = (TranslateTransform)((TransformGroup)this.RenderTransform).Children.First(tr => tr is TranslateTransform);
                Vector v = start - e.GetPosition((Border)this.Parent);
                tt.X = origin.X - v.X;
                tt.Y = origin.Y - v.Y;
            }

            if (_isDown)
            {
                if ((_isDragging == false) &&
                    ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
                    (Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
                    _isDragging = true;

                if (_isDragging && selectedElement != null)
                {
                    Point position = Mouse.GetPosition(this);
                    Canvas.SetTop(selectedElement, position.Y - (_startPoint.Y - _originalTop));
                    Canvas.SetLeft(selectedElement, position.X - (_startPoint.X - _originalLeft));
                    if (OnChangeElement != null && selectedElement is WElement)
                    {
                        WElement ct = selectedElement as WElement;
                        Point r1 = new Point(Canvas.GetLeft(ct), Canvas.GetTop(ct));
                        Point r2 = new Point(r1.X + ct.Width, r1.Y + ct.Height);
                        OnChangeElement(ct.ElementID, r1, r2);
                    }
                }
            }
        }

        void panel1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    selectedElement = null;
                }
            }

            this.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)this.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition((Border)this.Parent);
            origin = new Point(tt.X, tt.Y);
        }

        private void StopDragging()
        {
            if (_isDown)
            {
                _isDown = false;
                _isDragging = false;
            }
        }

        void panel1_DragFinishedMouseHandler(object sender, MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            StopDragging();
            e.Handled = true;
        }

        public int zindex = 99;
        void panel1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Remove selection on clicking anywhere the window
            if (selected)
            {
                selected = false;
                if (selectedElement != null)
                {
                    // Remove the adorner from the selected element
                    try
                    {
                        aLayer.Remove(aLayer.GetAdorners(selectedElement)[0]);
                    }
                    catch (Exception)
                    {
                    }

                    selectedElement = null;
                }
            }

            // If any element except canvas is clicked, 
            // assign the selected element and add the adorner
            if (e.Source != this)
            {
                _isDown = true;
                _startPoint = e.GetPosition(this);

                selectedElement = e.Source as UIElement;

                Panel.SetZIndex(selectedElement, zindex++);

                _originalLeft = Canvas.GetLeft(selectedElement);
                _originalTop = Canvas.GetTop(selectedElement);

                aLayer = AdornerLayer.GetAdornerLayer(selectedElement);
                ResizingAdorner rs = new ResizingAdorner(selectedElement);
                rs.onFinishResize += Rs_onFinishResize;
                aLayer.Add(rs);
                selected = true;
            }
        }

        private void Rs_onFinishResize(UIElement ui)
        {
            if (OnChangeElement != null && ui is WElement)
            {
                WElement ct = ui as WElement;
                Point r1 = new Point(Canvas.GetLeft(ct), Canvas.GetTop(ct));
                Point r2 = new Point(r1.X + ct.Width, r1.Y + ct.Height);
                OnChangeElement(ct.ElementID, r1, r2);
            }
        }

        public void removeBack(string fileName)
        { 
            foreach (var item in this.backElements.Where(x=>x.FileName.Trim().ToLower() == fileName.Trim().ToLower()).ToList())
            {
                backElements.Remove(item);
            }
            renderBackground();
        }

        public void addBack(string fileName, bool oldTexture = false)
        {
            string iniContent = File.ReadAllText(fileName);
            int ids = 1;
            string[] lns = iniContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            CElement el = null;
            foreach (var line in lns)
            {
                if (line.Split(':').Length == 2)
                {
                    string key = line.Split(':')[0];
                    string val = line.Split(':')[1];
                    if (key.Trim() == "Type")
                    {
                        el = new CElement();
                        el.ID = ids++;
                        el.Type = val.Trim();
                        el.FileName = fileName;
                        el.OLDTexture = oldTexture;
                    }

                    if (key.Trim() == "Name")
                        el.Name = val;
                    if (key.Trim() == "Group")
                        el.Group = val;
                    if (key.Trim() == "Texture")
                        el.Texture = val;
                    if (key.Trim() == "Layer")
                        el.Layer = Convert.ToSingle(val);

                    if (key.Trim() == "UV")
                    {
                        string cords1 = val.Split('/')[0].Split('-')[0];
                        string cords2 = val.Split('/')[0].Split('-')[1];
                        int x1 = Convert.ToInt32(cords1.Split(',')[0]);
                        int y1 = Convert.ToInt32(cords1.Split(',')[1]);
                        int x2 = Convert.ToInt32(cords2.Split(',')[0]);
                        int y2 = Convert.ToInt32(cords2.Split(',')[1]);
                        el.UV1 = new WPoint(x1, y1);
                        el.UV2 = new WPoint(x2, y2);

                        el.Properties.Add(new CustomProperty() { Name = "Image-Width", Category = "UV", Type = typeof(int), Value = x2 - x1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Image-Height", Category = "UV", Type = typeof(int), Value = y2 - y1, WProp = true });
                    }

                    if (key.Trim() == "Rect")
                    {
                        string cords1 = val.Split('/')[0].Split(new string[] { " - " }, StringSplitOptions.None)[0];
                        string cords2 = val.Split('/')[0].Split(new string[] { " - " }, StringSplitOptions.None)[1];
                        int x1 = Convert.ToInt32(Convert.ToSingle(cords1.Split(',')[0].Replace(".", ",")));
                        int y1 = Convert.ToInt32(Convert.ToSingle(cords1.Split(',')[1].Replace(".", ",")));
                        int x2 = Convert.ToInt32(Convert.ToSingle(cords2.Split(',')[0].Replace(".", ",")));
                        int y2 = Convert.ToInt32(Convert.ToSingle(cords2.Split(',')[1].Replace(".", ",")));
                        el.Rect1 = new WPoint(x1, y1);
                        el.Rect2 = new WPoint(x2, y2);

                        //Propertye Rect düzenlemelerini gir
                        el.Properties.Add(new CustomProperty() { Name = "Left", Category = "Rect", Type = typeof(int), Value = x1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Top", Category = "Rect", Type = typeof(int), Value = y1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Height", Category = "Rect", Type = typeof(int), Value = x2 - x1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Width", Category = "Rect", Type = typeof(int), Value = y2 - y1, WProp = true });
                    }

                    if (el != null)
                    {
                        //el.props.Add(key, val);
                        Type tip = typeof(string);
                        bool readonl = false;
                        if (key.Trim() == "UV")
                            readonl = true;
                        if (key.Trim() == "Rect")
                            readonl = true;
                        el.Properties.Add(new CustomProperty() { Name = key, ReadOnly = readonl, Category = "Other", Type = tip, Value = val });
                    }
                }
                else if (line.StartsWith("///"))
                {
                    if (el != null)
                    {
                        backElements.Add(el);
                        el = null;
                    }
                }
            }

            renderBackground();
        }

        public void renderBackground()
        {
            gdi.Bitmap img = new gdi.Bitmap(1024, 768);

            using (var g = gdi.Graphics.FromImage(img))
            {
                gdi.Brush sl = new gdi.SolidBrush(gdi.Color.FromArgb(234, 234, 234)); 
                g.FillRectangle(sl, 0, 0, 1024, 768);
                foreach (var item in backElements)
                {
                    Rect rc = new Rect(item.getLeft(), item.getTop(), item.getWidth(), item.getHeight());
                    if (rc.Width == 0 || rc.Height == 0)
                        continue;
                    ImageSource si = item.getImage();
                    if (si == null)
                        continue;

                    //dc.DrawImage(CreateResizedImage(img, item.getWidth(), item.getHeight()), rc);
                    gdi.Image gi = Statik.CroppedBitmapToBitmap(si as CroppedBitmap);

                    g.DrawImage(gi, item.getLeft(), item.getTop(), item.getWidth(), item.getHeight());

                    
                }
            }

            ImageBrush ib = new ImageBrush(Statik.ToWpfBitmap(img));
            
            this.Background = ib;
        }
    
        private ImageSource CreateResizedImage(ImageSource source, int width, int height)
        { 
            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing() { ImageSource = source });

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return resizedImage;
        }
    }
}
