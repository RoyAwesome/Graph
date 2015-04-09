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

            var position = bounds.Location;

            var left = position.X + halfConnectorSize;
            var top = position.Y;
            var right = position.X + bounds.Size.Width - halfConnectorSize;
            var bottom = position.Y + bounds.Size.Height;
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

        virtual protected void RenderCollapsed(Graphics graphics)
        {
            bool inputConnected = false;
            var inputState = RenderState.None;
            var outputState = this.outputState;
            foreach (var connection in connections)
            {
                if (connection.To.Node == this)
                {
                    inputState |= connection.state;
                    inputConnected = true;
                }
                if (connection.From.Node == this)
                    outputState |= connection.state | RenderState.Connected;
            }


            if (this.inputConnectors.Count > 0)
                GraphRenderer.RenderConnector(graphics, this.inputBounds, this.inputState);
            if (this.outputConnectors.Count > 0)
                GraphRenderer.RenderConnector(graphics, this.outputBounds, outputState);
            if (inputConnected)
                GraphRenderer.RenderArrow(graphics, this.inputBounds, inputState);
        }

        virtual protected void RenderItems(Graphics graphics, PointF StartPosition)
        {           
            PointF itemPosition = StartPosition;            
            foreach (var item in Items)
            {
                RenderItem(graphics, item, itemPosition);
                itemPosition.Y += item.bounds.Height + GraphConstants.ItemSpacing;
            }

        }

        virtual protected void RenderItem(Graphics graphics, NodeItem item, PointF itemPosition)
        {
            var minimumItemSize = new SizeF(this.bounds.Width - GraphConstants.NodeExtraWidth, 0);

            item.Render(graphics, minimumItemSize, itemPosition);


            var inputConnector = item.Input;
            if (inputConnector != null && inputConnector.Enabled)
            {
                if (!inputConnector.bounds.IsEmpty)
                {
                    var state = RenderState.None;
                    var connected = false;
                    foreach (var connection in connections)
                    {
                        if (connection.To == inputConnector)
                        {
                            state |= connection.state;
                            connected = true;
                        }
                    }

                    GraphRenderer.RenderConnector(graphics,
                                    inputConnector.bounds,
                                    inputConnector.state);

                    if (connected)
                        GraphRenderer.RenderArrow(graphics, inputConnector.bounds, state);
                }
            }
            var outputConnector = item.Output;
            if (outputConnector != null && outputConnector.Enabled)
            {
                if (!outputConnector.bounds.IsEmpty)
                {
                    var state = outputConnector.state;
                    foreach (var connection in connections)
                    {
                        if (connection.From == outputConnector)
                            state |= connection.state | RenderState.Connected;
                    }
                    GraphRenderer.RenderConnector(graphics, outputConnector.bounds, state);
                }
            }
            
        }

        virtual public void Render(Graphics graphics)
        {
            DrawBackground(graphics);

            var position = bounds.Location;

            position.X += GraphConstants.ConnectorSize + (int)GraphConstants.HorizontalSpacing;

            titleItem.Render(graphics, new SizeF(this.bounds.Width - GraphConstants.NodeExtraWidth, 0), position);
            position.Y += titleItem.bounds.Height + GraphConstants.ItemSpacing;

            if (Collapsed)
            {
                RenderCollapsed(graphics);
            }
            else
            {
                RenderItems(graphics, position);
            }

        }

    }
}
