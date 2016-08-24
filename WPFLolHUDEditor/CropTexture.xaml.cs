using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFLolHUDEditor
{
    /// <summary>
    /// Interaction logic for CropTexture.xaml
    /// </summary>
    public partial class CropTexture : Window
    {
        private Point origin;
        private Point start;

        public Ctexture SelectedTexture;

        public CropTexture(CElement el)
        { 
            InitializeComponent();
            this.image.Source = el.getTexture().img;
            Canvas.SetLeft(rect, el.UV1.X);
            Canvas.SetTop(rect, el.UV1.Y);
            rect.Width = el.UV2.X - el.UV1.X;
            rect.Height = el.UV2.Y - el.UV1.Y;
            this.SelectedTexture = el.getTexture();
            RefCropImage();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            TransformGroup group = new TransformGroup();
            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            canvas1.RenderTransform = group;

            TransformGroup group2 = new TransformGroup();
            ScaleTransform xform2 = new ScaleTransform();
            group2.Children.Add(xform2);
            TranslateTransform tt2 = new TranslateTransform();
            group2.Children.Add(tt2);
            canvas2.RenderTransform = group2;

            border1.MouseWheel += border1_MouseWheel;
            border2.MouseWheel += border2_MouseWheel;

            rect.SizeChanged += Rect_SizeChanged;
 
            border1.MouseLeftButtonDown += Border1_MouseLeftButtonDown;
            border1.MouseMove += Border1_MouseMove;
            border1.MouseLeftButtonUp += Border1_MouseLeftButtonUp;

            border2.MouseLeftButtonDown += Border2_MouseLeftButtonDown;
            border2.MouseMove += Border2_MouseMove;
            border2.MouseLeftButtonUp += Border2_MouseLeftButtonUp;

            foreach (var item in Statik.textures)
            {
                int indx = textures.Items.Add(item);
                if (item.Name == SelectedTexture.Name)
                    textures.SelectedIndex = indx;
            }
            
        }

        private void Border1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            border1.ReleaseMouseCapture();
        }

        private void Border1_MouseMove(object sender, MouseEventArgs e)
        {
            if (border1.IsMouseCaptured)
            {
                Vector st = (e.GetPosition(border1) - start);
                var tt = (TranslateTransform)((TransformGroup)canvas1.RenderTransform).Children.First(tr => tr is TranslateTransform);
                tt.X = st.X + origin.X;
                tt.Y = st.Y + origin.Y;
            } 
        }

        private void Border1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tt = (TranslateTransform)((TransformGroup)canvas1.RenderTransform).Children.First(tr => tr is TranslateTransform);
            origin = new Point(tt.X, tt.Y);
            start = e.GetPosition(border1); 
            border1.CaptureMouse(); 
        }

        private void Border2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            border2.ReleaseMouseCapture();
        }

        private void Border2_MouseMove(object sender, MouseEventArgs e)
        {
            if (border2.IsMouseCaptured)
            {
                Vector st = (e.GetPosition(border2) - start);
                var tt = (TranslateTransform)((TransformGroup)canvas2.RenderTransform).Children.First(tr => tr is TranslateTransform);
                tt.X = st.X + origin.X;
                tt.Y = st.Y + origin.Y;
            }
        }

        private void Border2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tt = (TranslateTransform)((TransformGroup)canvas2.RenderTransform).Children.First(tr => tr is TranslateTransform);
            origin = new Point(tt.X, tt.Y);
            start = e.GetPosition(border2);
            border2.CaptureMouse();
        }


        private void Rect_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefCropImage();
        }

        private void border1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)canvas1.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0]; 
            double zoom = e.Delta > 0 ? .2 : -.2;
            if ((zoom < 0 && (transform.ScaleX < 0.5 || transform.ScaleY < 0.5)))
                return;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
        }
        private void border2_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)canvas2.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0]; 
            double zoom = e.Delta > 0 ? .2 : -.2;
            if ((zoom < 0 && (transform.ScaleX < 0.5 || transform.ScaleY < 0.5)))
                return;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
        }

        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            RefCropImage();
        }

        public void RefCropImage()
        {
            try
            {
                int x = Convert.ToInt32(Canvas.GetLeft(rect));
                int y = Convert.ToInt32(Canvas.GetTop(rect));
                int x1 = Convert.ToInt32(rect.Width);
                int y1 = Convert.ToInt32(rect.Height);
                BitmapImage bi = image.Source as BitmapImage;
                CroppedBitmap a = new CroppedBitmap(bi, new Int32Rect(x, y, x1, y1));
                image2.Source = a;  
            }
            catch (Exception)
            { 
            } 
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void btnOkey_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void textures_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            image.Source = ((Ctexture)textures.SelectedItem).img;
        }
    }
}
