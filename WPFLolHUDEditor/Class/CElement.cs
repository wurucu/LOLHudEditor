using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPlug1;

namespace WPFLolHUDEditor
{

    public class CElementCollection : List<CElement>
    {
        public CElement getID(int id)
        {
            return this.Where(x => x.ID == id).FirstOrDefault();
        }
    }

    public class WPoint
    {
        public WPoint()
        {
        }

        public WPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }
    public class Rectangle
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle(int r1, int r2, int r3, int r4)
        {
            this.Left = r1;
            this.Right = r2;
            this.Width = r3;
            this.Height = r4;
        }
    }

    public class CElement
    {
        public int ID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Texture { get; set; }
        public string Group { get; set; }
        public float Layer { get; set; }
        public WPoint UV1 { get; set; }
        public WPoint UV2 { get; set; }
        public WPoint Rect1 { get; set; }
        public WPoint Rect2 { get; set; }
        public bool Selected { get; set; }
        public string FileName { get; set; }
        //public NameValueCollection props { get; set; }

        public ComplexObject Properties { get; set; }
        public bool OLDTexture = false;

        public CElement Clone()
        {
            CElement don = new CElement();
            don.ID = ID;
            don.Type = Type;
            don.Name = Name;
            don.Texture = Texture;
            don.Group = Group;
            don.Layer = Layer;
            don.UV1 = UV1;
            don.UV2 = UV2;
            don.Rect1 = Rect1;
            don.Rect2 = Rect2;
            don.Selected = Selected;
            don.FileName = FileName;
            don.Properties = Properties;
            don.OLDTexture = OLDTexture;

            return don;
        }

        public CElement()
        {
            //this.props = new NameValueCollection();
            this.Properties = new ComplexObject();
            this.UV1 = new WPoint();
            this.UV2 = new WPoint();
            this.Rect1 = new WPoint();
            this.Rect2 = new WPoint();
        }

        public void SetUV(WPoint uv1, WPoint uv2)
        {
            //this.props["UV"] = txt.Replace("UV: ", "");
            this.UV1 = uv1;
            this.UV2 = uv2;
            this.Properties.setVal("UV", uv1.X + "," + uv1.Y + " - " + uv2.X + "," + UV2.Y + " / 1024x1024");

            this.Properties.setVal("Image-Width", this.UV2.X - this.UV1.X);
            this.Properties.setVal("Image-Height", this.UV2.Y - this.UV1.Y);
        }

        public void SetUV(string txt)
        {
            this.Properties.setVal("UV", txt.Replace("UV: ", ""));
            //"Image-Width",
            //"Image-Height"
            this.Properties.setVal("Image-Width", this.UV2.X - this.UV1.X);
            this.Properties.setVal("Image-Height", this.UV2.Y - this.UV1.Y);
        }

        public void setRect(WPoint R1, WPoint R2)
        {
            this.Rect1 = R1;
            this.Rect2 = R2;
            this.Properties.setVal("Rect", R1.X + ", " + R1.Y + " - " + R2.X + "," + R2.Y + " / 1024x768");
            this.Properties.setVal("Left", R1.X);
            this.Properties.setVal("Top", R1.Y);
            this.Properties.setVal("Width", R2.X - R1.X);
            this.Properties.setVal("Height", R2.Y - R1.Y);
        }

        public void setLeft(int x)
        {
            Point p1 = new Point(x, this.Rect1.Y);
            Point p2 = new Point(x + getWidth(), this.Rect2.Y);
            setRect(p1, p2);
        }
        public void setTop(int y)
        {
            Point p1 = new Point(this.Rect1.X, y);
            Point p2 = new Point(this.Rect2.X, y + getHeight());
            setRect(p1, p2);
        }
         
        public void setWidth(int x)
        {
            Point p1 = new Point(this.Rect1.X, this.Rect1.Y);
            Point p2 = new Point(this.Rect1.X + x, this.Rect2.Y);
            setRect(p1, p2);
        }

        public void setHeight(int y)
        {
            Point p1 = new Point(this.Rect1.X, this.Rect1.Y);
            Point p2 = new Point(this.Rect2.X, this.Rect1.Y + y);
            setRect(p1, p2);
        }

        public void setRect(Point R1, Point R2)
        {
            this.Rect1 = new WPoint(Convert.ToInt32(R1.X), Convert.ToInt32(R1.Y));
            this.Rect2 = new WPoint(Convert.ToInt32(R2.X), Convert.ToInt32(R2.Y));
            this.Properties.setVal("Rect", Rect1.X + ", " + Rect1.Y + " - " + Rect2.X + "," + Rect2.Y + " / 1024x768");
            this.Properties.setVal("Left", Rect1.X);
            this.Properties.setVal("Top", Rect1.Y);
            this.Properties.setVal("Width", Rect2.X - Rect1.X);
            this.Properties.setVal("Height", Rect2.Y - Rect1.Y);
        }

        public int getWidth()
        {
            return Rect2.X - Rect1.X;
        }

        public int getHeight()
        {
            return Rect2.Y - Rect1.Y;
        }

        public int getLeft()
        {
            return Rect1.X;
        }

        public int getTop()
        {
            return Rect1.Y;
        }

        public ImageSource getImage()
        {
            if (this.UV1 == null || this.UV2 == null)
                return null;
            if (this.UV2.X - this.UV1.X == 0 || this.UV2.Y - this.UV1.Y == 0)
                return null;

            try
            {
                //System.Drawing.Image img = WPlug1.WCrop.getImage(this.Texture.Trim(), this.UV1.X, this.UV1.Y, this.UV2.X - this.UV1.X, this.UV2.Y - this.UV1.Y, this.OLDTexture, false);
                //return BitmapToImageSource(new System.Drawing.Bitmap(img));
                CroppedBitmap cp = new CroppedBitmap(getTexture().img as BitmapImage, new Int32Rect(this.UV1.X, this.UV1.Y, this.UV2.X - this.UV1.X, this.UV2.Y - this.UV1.Y));
                return cp;
            }
            catch (Exception)
            {

            }

            return null;
        }

        BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }


        public Ctexture getTexture()
        {
            string tname = this.Texture;
            if (this.OLDTexture && tname.Trim().ToLower() == "HUDAtlas".Trim().ToLower())
                tname = "HUDAtlase";
            Ctexture texture = Statik.textures.Where(x => x.Name.Trim().ToLower() == tname.Trim().ToLower()).FirstOrDefault();
            return texture;
        }
    }




    public class Ctexture
    {
        public string Name;
        public ImageSource img;
        public override string ToString()
        {
            return this.Name;
        }
    }



}
