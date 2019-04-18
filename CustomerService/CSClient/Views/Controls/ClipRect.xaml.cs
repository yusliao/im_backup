using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSClient.Helper;

namespace CSClient.Views.Controls
{
    /// <summary>
    /// ClipRect.xaml 的交互逻辑
    /// </summary>
    public partial class ClipRect : UserControl
    {

        System.Drawing.Rectangle _maxRect = PrimaryScreen.FullRect;


        ClipWindow _clipWin;
        public ClipRect(ClipWindow clipWin, Rect rect)
        {
            InitializeComponent();
             
            _clipWin = clipWin;
            UpdateLayout(rect); 
        }

        private void UpdateLayout(Rect rect)
        {  
            Canvas.SetLeft(this.thumb1, rect.X);
            Canvas.SetTop(this.thumb1, rect.Y);

            Canvas.SetLeft(this.thumb2, rect.X+rect.Width*0.5);
            Canvas.SetTop(this.thumb2, rect.Y);

            Canvas.SetLeft(this.thumb3, rect.X+rect.Width);
            Canvas.SetTop(this.thumb3, rect.Y);

            Canvas.SetLeft(this.thumb4, rect.X);
            Canvas.SetTop(this.thumb4, rect.Y+rect.Height*0.5);

            Canvas.SetLeft(this.thumb5, rect.X+rect.Width);
            Canvas.SetTop(this.thumb5, rect.Y+rect.Height*0.5);

            Canvas.SetLeft(this.thumb6, rect.X);
            Canvas.SetTop(this.thumb6, rect.Y+rect.Height);

            Canvas.SetLeft(this.thumb7, rect.X+rect.Width*0.5);
            Canvas.SetTop(this.thumb7, rect.Y+rect.Height);

            Canvas.SetLeft(this.thumb8, rect.X+rect.Width);
            Canvas.SetTop(this.thumb8, rect.Y+rect.Height);

            this.thumb0.Width = rect.Width;
            this.thumb0.Height = rect.Height;

            Canvas.SetLeft(this.thumb0, rect.X);
            Canvas.SetTop(this.thumb0, rect.Y);
        }

        
        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double x1 = Canvas.GetLeft(this.thumb1);
            double x2 = Canvas.GetLeft(this.thumb8);
          
            double y1 = Canvas.GetTop(this.thumb1);
            double y2 = Canvas.GetTop(this.thumb8);
              
            Thumb thumb = e.Source as Thumb;

            switch (thumb.Uid)
            {
                case "lefttop":
                    x1 += e.HorizontalChange;
                    y1 += e.VerticalChange;
                    break;
                case "top":
                    y1 += e.VerticalChange;
                    break;
                case "righttop":
                    x2 += e.HorizontalChange;
                    y1 += e.VerticalChange;
                    break;
                case "left":
                    x1 += e.HorizontalChange;
                    break;
                case "right":
                    x2 += e.HorizontalChange;
                    break;
                case "leftbottom":
                    x1 += e.HorizontalChange;
                    y2 += e.VerticalChange;
                    break;
                case "bottom":
                    y2 += e.VerticalChange;
                    break;
                case "rightbottom":
                    x2 += e.HorizontalChange;
                    y2 += e.VerticalChange;
                    break;
                case "move":
                    double xOffset = e.HorizontalChange, yOffset = e.VerticalChange;

                    if (x1 + xOffset < _maxRect.X)
                    {
                        xOffset = _maxRect.X - x1;
                    }
                    else if (x2 + xOffset >_maxRect.X + _maxRect.Width)
                    {
                        xOffset = _maxRect.X + _maxRect.Width - x2;
                    }

                    if (y1 + yOffset < _maxRect.Y)
                    {
                        yOffset = _maxRect.Y - y1;
                    }
                    else if (y2 + yOffset > _maxRect.Y + _maxRect.Height)
                    {
                        yOffset = _maxRect.Y + _maxRect.Height - y2;
                    } 
                    x1 += xOffset;
                    y1 += yOffset;

                    x2 += xOffset;
                    y2 += yOffset;

                    break;
            }

            double xCenter = x1 + (x2 - x1) * 0.5;
            double yCenter = y1 + (y2 - y1) * 0.5;
             
            Canvas.SetLeft(this.thumb1, x1);
            Canvas.SetTop(this.thumb1, y1);

            Canvas.SetLeft(this.thumb2, xCenter);
            Canvas.SetTop(this.thumb2, y1);

            Canvas.SetLeft(this.thumb3, x2);
            Canvas.SetTop(this.thumb3, y1);

            Canvas.SetLeft(this.thumb4, x1);
            Canvas.SetTop(this.thumb4, yCenter);

            Canvas.SetLeft(this.thumb5, x2);
            Canvas.SetTop(this.thumb5, yCenter);

            Canvas.SetLeft(this.thumb6, x1);
            Canvas.SetTop(this.thumb6, y2);

            Canvas.SetLeft(this.thumb7, xCenter);
            Canvas.SetTop(this.thumb7, y2);

            Canvas.SetLeft(this.thumb8, x2);
            Canvas.SetTop(this.thumb8, y2);
              
            Rect rect= new Rect(new Point(x1, y1), new Point(x2, y2)); 
            _clipWin.rectClip.Rect = rect;
            _clipWin.UpdateMagnifierLayout();

            this.thumb0.Width = rect.Width;
            this.thumb0.Height = rect.Height;

            Canvas.SetLeft(this.thumb0, rect.X);
            Canvas.SetTop(this.thumb0, rect.Y);

        }
         
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.UpdateLayout(_clipWin.rectClip.Rect);
        } 
    }
}
