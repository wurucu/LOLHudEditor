using AndBurn.DDSReader;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gdi = System.Drawing;
namespace WPFLolHUDEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public string SelectedFile = "";

        public class WorkPath
        {
            public string Name;
            public string Path;
            public bool OverrideOldTexture = false; // eski dosyaları test ederken eski texture dosyasını kullan
            public WorkPath(string nm, string path, bool oldtext = false)
            {
                this.Name = nm;
                this.Path = path;
                this.OverrideOldTexture = oldtext;
            }
            public override string ToString()
            {
                return this.Name;
            }
        }
        public class WKeyVal
        {
            public string DisplayName;
            public object Val;
            public WKeyVal(string name, object val)
            {
                this.DisplayName = name;
                this.Val = val;
            }
            public override string ToString()
            {
                return DisplayName;
            }
        }

        CElementCollection elements = new CElementCollection();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
             
            comboBox1.Items.Clear(); 
            comboBox1.Items.Add(new WorkPath("ClarityHUD", System.AppDomain.CurrentDomain.BaseDirectory  + @"\works\Clarity"));
            comboBox1.Items.Add(new WorkPath("OLD", System.AppDomain.CurrentDomain.BaseDirectory + @"\works\Normal",true)); 
            Statik.frm1 = this;
            foreach (var fl in Directory.GetFiles(System.AppDomain.CurrentDomain.BaseDirectory + "\\Textures\\", "*.dds"))
            {
                try
                {
                    ImageSourceConverter c = new ImageSourceConverter();
                    DDSImage dds = new DDSImage(File.ReadAllBytes(fl), true);
                    BitmapSource src = Statik.ToWpfBitmap(dds.BitmapImage);

                    FileInfo fi = new FileInfo(fl);
                    Statik.textures.Add(new Ctexture() { Name = fi.Name.Replace(fi.Extension, ""), img = src });
                }
                catch (Exception exx)
                {
                    MessageBox.Show(exx.Message);
                }
            }

            panel1.PreviewMouseLeftButtonDown += Panel1_PreviewMouseLeftButtonDown;
            panel1.OnChangeElement += Panel1_OnChangeElement;
            var window = Window.GetWindow(this);
            window.KeyDown += Panel1_KeyDown;


        }

        private void Panel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown && e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                CElement el = getSelectedElement();
                if (el != null)
                {
                    CElement nw = el.Clone();
                    nw.ID = UniqueID++;
                    nw.setLeft(nw.getLeft() + 10);
                    nw.setTop(nw.getTop() + 10);
                    elements.Add(nw);
                    WElement cnw = addElementControl(nw);
                    Panel.SetZIndex(cnw, panel1.zindex++);
                    SetSelectedElement(cnw.ElementID);

                    lwElementList.Items.Add(nw);

                    for (int i = 0; i < lwElementList.Items.Count; i++)
                    {
                        if (((CElement)lwElementList.Items[i]).ID == cnw.ElementID)
                        {
                            lwElementList.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }

        private void Panel1_OnChangeElement(int ID, Point rec1, Point rec2)
        {
            CElement el = elements.getID(ID);
            if (el != null)
            {
                el.setRect(rec1, rec2);
                _propertyGrid.Update();
            }
        }

        private void Panel1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is WElement)
            {
                if (e.Source is WElement)
                {
                    for (int i = 0; i < lwElementList.Items.Count; i++)
                    {
                        if (((CElement)lwElementList.Items[i]).ID == ((WElement)e.Source).ElementID)
                        {
                            lwElementList.SelectedIndex = i;
                            break;
                        }
                    }
                    SetSelectedElement(getSelectedElement().ID);
                }
                e.Handled = true;
            }

            if (e.Source == panel1)
            {
                lwElementList.SelectedIndex = -1;
                SetSelectedElement(-1);
            }

        }

        internal void onPropertyChanged(CustomProperty _owner, object value)
        {
            WElement wc = getSelectedElementControl();
            CElement el = getSelectedElement();
            if (wc == null)
                return;

            if (_owner.Name.Trim().ToLower() == "visible" && _owner.WProp)
            {
                wc.Visibility = (bool)value == true ? Visibility.Visible : Visibility.Hidden;
            }

            if (_owner.WProp && _owner.Category.Trim().ToLower() == "rect")
            {
                if (_owner.Name.Trim() == "Width")
                    el.setWidth((int)value);
                if (_owner.Name.Trim() == "Height")
                    el.setHeight((int)value);
                if (_owner.Name.Trim() == "Left")
                    el.setLeft((int)value);
                if (_owner.Name.Trim() == "Top")
                    el.setTop((int)value);

                updateElementControl(el.ID);
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lwFiles.Items.Clear();
            WorkPath wp = ((WorkPath)comboBox1.SelectedItem);
            foreach (var item in Directory.GetFiles(wp.Path).OrderBy(x => x))
            {
                FileInfo fi = new FileInfo(item);
                lwFiles.Items.Add(new WKeyVal(fi.Name, fi.FullName));
            }
        }

        private void lwFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lwFiles.SelectedItems.Count == 1)
            {
                FileInfo fi = new FileInfo((lwFiles.SelectedItems[0] as WKeyVal).Val.ToString());
                if (fi.Extension.ToLower() == ".ini")
                {
                    SelectedFile = fi.FullName;
                    LoadElements(File.ReadAllText(fi.FullName));
                }
            }
        }

        int UniqueID = 1;
        public void LoadElements(string inicontent)
        {
            elements.Clear();
            string[] lns = inicontent.Split(new string[] { "\r\n" }, StringSplitOptions.None);
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
                        el.ID = UniqueID++;
                        el.Type = val.Trim();

                        if (((WorkPath)comboBox1.SelectedItem).OverrideOldTexture)
                            el.OLDTexture = true;
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
                        el.Properties.Add(new CustomProperty() { Name = "Visible", Category = "Rect", Type = typeof(bool), Value = true, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Left", Category = "Rect", Type = typeof(int), Value = x1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Top", Category = "Rect", Type = typeof(int), Value = y1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Width", Category = "Rect", Type = typeof(int), Value = x2 - x1, WProp = true });
                        el.Properties.Add(new CustomProperty() { Name = "Height", Category = "Rect", Type = typeof(int), Value = y2 - y1, WProp = true });
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
                        elements.Add(el);
                        el = null;
                    }
                }
            }

            lwElementList.Items.Clear();
            foreach (var item in elements)
            {
                lwElementList.Items.Add(item);
            }

            RenderElements();
        }

        public WElement addElementControl(CElement item)
        {
            WElement pc = new WElement();
            pc.ElementID = item.ID;
            Canvas.SetLeft(pc, item.getLeft());
            Canvas.SetTop(pc, item.getTop());
            pc.Stretch = Stretch.Fill;

            try
            { 
                pc.Width = item.getWidth();
                pc.Height = item.getHeight();
            }
            catch (Exception)
            {
                 
            }
            pc.Source = item.getImage();
            pc.ElementName = item.Name;
            pc.Tip = item.Type == "Text" ? WElement.ETip.Text : WElement.ETip.Icon;
            if (item.Properties.exist("Size"))
                pc.Size = Convert.ToInt32(item.Properties.getVal("Size").ToString());
            else
                pc.Size = 10;
            if (item.Properties.exist("Color"))
                pc.TColor = item.Properties.getVal("Color").ToString();

            panel1.Children.Add(pc);

            return pc;
        }

        public void RenderElements()
        {
            panel1.Children.Clear();
            foreach (var item in elements)
            {
                addElementControl(item);
            }
        }


        int ZIndex = 999;
        public void SetSelectedElement(int ID)
        {
            if (ID != -1)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    elements[i].Selected = false;
                    getElementControl(elements[i].ID).setSelected(false);
                }

                CElement el = elements.getID(ID);
                el.Selected = true;

                pictureBox1.Source = el.getImage();
                getSelectedElementControl().BringIntoView();
                _propertyGrid.SelectedObject = getSelectedElement().Properties.Properties;

                getSelectedElementControl().setSelected(true);
                Panel.SetZIndex(getSelectedElementControl(), ZIndex++);
            }
            else
            { // seçimi kaldır
                for (int i = 0; i < elements.Count; i++)
                {
                    elements[i].Selected = false;
                    getElementControl(elements[i].ID).setSelected(false);
                }
            }
        }

        public CElement getSelectedElement()
        {
            if (lwElementList.SelectedItems.Count == 1)
            {
                int id = ((CElement)lwElementList.SelectedItems[0]).ID;
                return elements.getID(id);
            }
            return null;
        }

        public WElement getSelectedElementControl()
        {
            CElement el = getSelectedElement();
            if (el == null)
                return null;
            for (int i = 0; i < panel1.Children.Count; i++)
            {
                if (((WElement)panel1.Children[i]).ElementID == el.ID)
                    return ((WElement)panel1.Children[i]);
            }
            return null;
        }

        public void updateElementControl(int ID)
        {
            WElement ct = getElementControl(ID);
            CElement el = elements.getID(ID);

            if (ct == null || el == null)
                return;

            ct.Width = el.getWidth();
            ct.Height = el.getHeight();
            Canvas.SetLeft(ct, el.getLeft());
            Canvas.SetTop(ct, el.getTop());
            ct.Source = el.getImage();
        }

        public WElement getElementControl(int ID)
        {
            for (int i = 0; i < panel1.Children.Count; i++)
            {
                if (((WElement)panel1.Children[i]).ElementID == ID)
                    return ((WElement)panel1.Children[i]);
            }
            return null;
        }

        private void lwElementList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lwElementList.SelectedItems.Count == 1)
            {
                CElement el = getSelectedElement();
                SetSelectedElement(el.ID);
            }
        }

        private void lwFiles_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                if (((TextBlock)e.OriginalSource).Background == Brushes.Red)
                {
                    ((TextBlock)e.OriginalSource).Background = Brushes.Transparent;
                    HitTestResult result = VisualTreeHelper.HitTest(lwFiles, e.GetPosition(lwFiles));
                    ListBoxItem lbi = Statik.FindParent<ListBoxItem>(result.VisualHit);
                    WKeyVal vl = lbi.DataContext as WKeyVal;
                    WorkPath wp = ((WorkPath)comboBox1.SelectedItem);

                    panel1.removeBack(vl.Val.ToString());
                }
                else
                {
                    ((TextBlock)e.OriginalSource).Background = Brushes.Red;

                    HitTestResult result = VisualTreeHelper.HitTest(lwFiles, e.GetPosition(lwFiles));
                    ListBoxItem lbi = Statik.FindParent<ListBoxItem>(result.VisualHit);
                    WKeyVal vl = lbi.DataContext as WKeyVal;
                    WorkPath wp = ((WorkPath)comboBox1.SelectedItem);

                    panel1.addBack(vl.Val.ToString(), wp.OverrideOldTexture);
                }

            }
            e.Handled = true;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            CElement el = getSelectedElement();
            WElement ct = getSelectedElementControl();
            if (el != null)
            {
                CropTexture Frmct = new CropTexture(el);
                if (Frmct.ShowDialog() == true)
                {
                    int x = Convert.ToInt32(Canvas.GetLeft(Frmct.rect));
                    int y = Convert.ToInt32(Canvas.GetTop(Frmct.rect));
                    int x1 = Convert.ToInt32(x + Frmct.rect.Width);
                    int y1 = Convert.ToInt32(y + Frmct.rect.Height);
                    WPoint uv1 = new WPoint(x, y);
                    WPoint uv2 = new WPoint(x1, y1);
                    el.SetUV(uv1, uv2);
                    el.Texture = Frmct.SelectedTexture.Name;
                    ct.Source = el.getImage();
                    pictureBox1.Source = ct.Source;
                    _propertyGrid.SelectedObject = el.Properties.Properties;
                    _propertyGrid.Update();
                }
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        public void save()
        {
            string pt = "BaseOffset: 0,0" + Environment.NewLine +
                "//////////////////////////////////////////" + Environment.NewLine;

            //foreach (var item in elements)
            //{
            //    for (int i = 0; i < item.props.Count; i++)
            //    {
            //        pt += item.props.GetKey(i).Trim() + ": " + item.props[i].Trim() + Environment.NewLine;
            //    }
            //    pt += "//////////////////////////////////////////" + Environment.NewLine;
            //} 

            foreach (var item in elements)
            {
                foreach (var prop in item.Properties.Properties.Where(x => x.WProp == false))
                {
                    pt += prop.Name + ": " + prop.Value.ToString().Trim() + Environment.NewLine;
                }
                pt += "//////////////////////////////////////////" + Environment.NewLine;
            }

            if (lwFiles.SelectedItems.Count == 1)
            {
                FileInfo fi = new FileInfo(this.SelectedFile);
                File.WriteAllText(fi.FullName, pt);
                MessageBox.Show("Kayıt edildi");
            }
        }

        private void addBackImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                BitmapImage bi = new BitmapImage(new Uri(dlg.FileName));
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = bi;
                panel1.Background = ib;
            }
        }

        private void removeBackImage_Click(object sender, RoutedEventArgs e)
        {
            panel1.Background = null;
            gdi.Bitmap img = new gdi.Bitmap(1024, 768);

            using (var g = gdi.Graphics.FromImage(img))
            {
                gdi.Brush sl = new gdi.SolidBrush(gdi.Color.FromArgb(234, 234, 234));
                g.FillRectangle(sl, 0, 0, 1024, 768);
            }
            ImageBrush ib = new ImageBrush(Statik.ToWpfBitmap(img));
            panel1.Background = ib;
        }
    }
}
