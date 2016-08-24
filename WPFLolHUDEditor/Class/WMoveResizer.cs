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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFLolHUDEditor
{
    class WMoveResizer
    {
        private static bool DragInProgress = false;
        private static Point LastPoint;
        static HitType MouseHitType = HitType.None;
        public static List<UIElement> controls = new List<UIElement>();
        public static Canvas canvas1;

        public static void Add(WElement ct)
        {
            controls.Add(ct);
            ct.MouseDown += Ct_MouseDown;
            ct.MouseMove += Ct_MouseMove;
            ct.MouseUp += Ct_MouseUp;
        }

        private static void Ct_MouseUp(object sender, MouseButtonEventArgs e)
        {
            DragInProgress = false;
        }

        private static void Ct_MouseMove(object sender, MouseEventArgs e)
        {
            WElement rectangle1 = sender as WElement;
            if (!DragInProgress)
            {
                MouseHitType = SetHitType(rectangle1, Mouse.GetPosition(canvas1));
                SetMouseCursor();
            }
            else
            {
                // See how much the mouse has moved.
                Point point = Mouse.GetPosition(rectangle1);
                double offset_x = point.X - LastPoint.X;
                double offset_y = point.Y - LastPoint.Y;

                // Get the rectangle's current position.
                double new_x = Canvas.GetLeft(rectangle1);
                double new_y = Canvas.GetTop(rectangle1);
                double new_width = rectangle1.Width;
                double new_height = rectangle1.Height;

                // Update the rectangle.
                switch (MouseHitType)
                {
                    case HitType.Body:
                        new_x += offset_x;
                        new_y += offset_y;
                        break;
                    case HitType.UL:
                        new_x += offset_x;
                        new_y += offset_y;
                        new_width -= offset_x;
                        new_height -= offset_y;
                        break;
                    case HitType.UR:
                        new_y += offset_y;
                        new_width += offset_x;
                        new_height -= offset_y;
                        break;
                    case HitType.LR:
                        new_width += offset_x;
                        new_height += offset_y;
                        break;
                    case HitType.LL:
                        new_x += offset_x;
                        new_width -= offset_x;
                        new_height += offset_y;
                        break;
                    case HitType.L:
                        new_x += offset_x;
                        new_width -= offset_x;
                        break;
                    case HitType.R:
                        new_width += offset_x;
                        break;
                    case HitType.B:
                        new_height += offset_y;
                        break;
                    case HitType.T:
                        new_y += offset_y;
                        new_height -= offset_y;
                        break;
                }

                // Don't use negative width or height.
                if ((new_width > 0) && (new_height > 0))
                {
                    // Update the rectangle.
                    Canvas.SetLeft(rectangle1, new_x);
                    Canvas.SetTop(rectangle1, new_y);
                    rectangle1.Width = new_width;
                    rectangle1.Height = new_height;

                    // Save the mouse's new location.
                    LastPoint = point;
                }
            }
        }

        private static void Ct_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseHitType = SetHitType(sender as WElement, Mouse.GetPosition(sender as WElement));
            SetMouseCursor();
            if (MouseHitType == HitType.None) return;

            LastPoint = Mouse.GetPosition(sender as WElement);
            DragInProgress = true;
        }

        private enum HitType
        {
            None, Body, UL, UR, LR, LL, L, R, T, B
        };

 

        // Return a HitType value to indicate what is at the point.
        private static HitType SetHitType(WElement ct, Point point)
        {
            point.X += canvas1.Margin.Left;
            point.Y += canvas1.Margin.Top;
            double left = ct.Margin.Left + canvas1.Margin.Left;
            double top = ct.Margin.Top + canvas1.Margin.Top;
            double right = left + ct.Width;
            double bottom = top + ct.Height;
            if (point.X < left) return HitType.None;
            if (point.X > right) return HitType.None;
            if (point.Y < top) return HitType.None;
            if (point.Y > bottom) return HitType.None;

            const double GAP = 10;
            if (point.X - left < GAP)
            {
                // Left edge.
                if (point.Y - top < GAP) return HitType.UL;
                if (bottom - point.Y < GAP) return HitType.LL;
                return HitType.L;
            }
            if (right - point.X < GAP)
            {
                // Right edge.
                if (point.Y - top < GAP) return HitType.UR;
                if (bottom - point.Y < GAP) return HitType.LR;
                return HitType.R;
            }
            if (point.Y - top < GAP) return HitType.T;
            if (bottom - point.Y < GAP) return HitType.B;
            return HitType.Body;
        }

        // Set a mouse cursor appropriate for the current hit type.
        private static void SetMouseCursor()
        { 
            Cursor desired_cursor = Cursors.Arrow;
            switch (MouseHitType)
            {
                case HitType.None:
                    desired_cursor = Cursors.Arrow;
                    break;
                case HitType.Body:
                    desired_cursor = Cursors.ScrollAll;
                    break;
                case HitType.UL:
                case HitType.LR:
                    desired_cursor = Cursors.SizeNWSE;
                    break;
                case HitType.LL:
                case HitType.UR:
                    desired_cursor = Cursors.SizeNESW;
                    break;
                case HitType.T:
                case HitType.B:
                    desired_cursor = Cursors.SizeNS;
                    break;
                case HitType.L:
                case HitType.R:
                    desired_cursor = Cursors.SizeWE;
                    break;
            } 
            if (Statik.frm1.Cursor != desired_cursor) Statik.frm1.Cursor = desired_cursor;
        }
    }
}
