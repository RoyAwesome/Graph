using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;

namespace Graph
{
    public partial class Node
    {
        protected static Pen BorderPen = new Pen(Color.FromArgb(64, 64, 64));

    
        virtual protected void DrawBackground(Graphics graphics)
        {
            int cornerSize = (int)GraphConstants.CornerSize * 2;
            int halfConnectorSize = (int)Math.Ceiling(GraphConstants.ConnectorSize / 2.0f);

            var position = Bounds.Location;

            var left = position.X + halfConnectorSize;
            var top = position.Y;
            var right = position.X + Bounds.Size.Width - halfConnectorSize;
            var bottom = position.Y + Bounds.Size.Height;
            using (var path = new GraphicsPath(FillMode.Winding))
            {
                path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
                path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);

                path.AddArc(right - cornerSize, bottom - cornerSize, cornerSize, cornerSize, 0, 90);
                path.AddArc(left, bottom - cornerSize, cornerSize, cornerSize, 90, 90);
                path.CloseFigure();

                if ((state & (RenderState.Dragging | RenderState.Focus)) != 0)
                {
                    graphics.FillPath(Brushes.DarkOrange, path);
                }
                else
                if ((state & RenderState.Hover) != 0)
                {
                    graphics.FillPath(Brushes.LightSteelBlue, path);
                }
                else
                {
                    graphics.FillPath(Brushes.LightGray, path);
                }
                graphics.DrawPath(BorderPen, path);
            }

        }

        virtual protected void DrawHeader(Graphics graphics)
        {
            graphics.DrawString(Title, SystemFonts.CaptionFont, Brushes.White, HeaderBounds, GraphConstants.TitleStringFormat);
        }
              
        
        virtual public void Render(Graphics graphics)
        {
            DrawBackground(graphics);

            DrawHeader(graphics);
                    
            foreach (var item in Items)
            {
                item.Render(graphics);
            }   
        }
    }
}
