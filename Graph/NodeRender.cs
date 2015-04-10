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

        protected Brush HeaderColor = new SolidBrush(Color.FromArgb((int)(255 * .5), Color.PaleVioletRed));
        protected Brush BackgroundColor = new SolidBrush(Color.FromArgb((int)(240), Color.Black));

        protected Brush HeaderTextColor = Brushes.White;

        protected Pen DraggingBorder = new Pen(Color.White, 5);
        protected Pen SelectedBorder = new Pen(Color.White, 2);
        protected Pen HoverBorder = new Pen(Color.LightGray, 2);
        protected Pen IdleBorder = new Pen(Color.Black, 2);

        public Brush TextColor = Brushes.White;


    
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

                graphics.FillPath(BackgroundColor, path);

                DrawHeader(graphics);

                if ((state & (RenderState.Dragging)) != 0)
                {
                    graphics.DrawPath(DraggingBorder, path);
                }
                else if ((state & RenderState.Focus) != 0)
                {
                    graphics.DrawPath(SelectedBorder, path);
                }
                else if ((state & RenderState.Hover) != 0)
                {
                    graphics.DrawPath(HoverBorder, path);
                }
                else
                {
                    graphics.DrawPath(IdleBorder, path);
                }
               
            }

            if(GraphConstants.DebugRender) graphics.DrawRectangle(Pens.Turquoise, Bounds.X, Bounds.Y, Bounds.Width, Bounds.Height);

        }

        virtual protected void DrawHeader(Graphics graphics)
        {
            using (var path = new GraphicsPath(FillMode.Winding))
            {
                int cornerSize = (int)GraphConstants.CornerSize * 2;
                int halfConnectorSize = (int)Math.Ceiling(GraphConstants.ConnectorSize / 2.0f);

                var position = HeaderBounds.Location;
                var left = position.X + halfConnectorSize;
                var top = position.Y - GraphConstants.TopHeight;
                var right = position.X + HeaderBounds.Size.Width - halfConnectorSize;
                var bottom = position.Y + HeaderBounds.Size.Height;

                path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
                path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);
                path.AddLine(right, top, right, bottom);
                path.AddLine(right, bottom, left, bottom);
                path.AddLine(left, bottom, left, top);
                

                path.CloseFigure();

                graphics.FillPath(HeaderColor, path);



            }

            //graphics.FillRectangle(Brushes.Red, HeaderBounds);
            graphics.DrawString(Title, SystemFonts.CaptionFont, Brushes.White, HeaderBounds, GraphConstants.TitleStringFormat);
        }
              
        
        virtual public void Render(Graphics graphics)
        {
            DrawBackground(graphics);

                     
            foreach (var item in Items)
            {
                item.Render(graphics);
            }   
        }
    }
}
