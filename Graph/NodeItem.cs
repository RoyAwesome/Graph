#region License
// Copyright (c) 2009 Sander van Rossen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Graph
{
    public enum NodeItemType
    {
        Input,
        Output,
        Label,
    }

    public sealed class NodeItemEventArgs : EventArgs
    {
        public NodeItemEventArgs(NodeItem item) { Item = item; }
        public NodeItem Item { get; private set; }
    }

    public abstract class NodeItem : IElement
    {
        public NodeItemType ItemType
        {
            get
            {
                if (Input == null && Output == null) return NodeItemType.Label;
                if (Input != null) return NodeItemType.Input;
                return NodeItemType.Output;
            }
        }

        public NodeItem()
        {

        }

        public NodeItem(NodeItemType Type)
        {
            if (Type == NodeItemType.Input) this.Input = new NodeInputConnector(this, true);
            if (Type == NodeItemType.Output) this.Output = new NodeOutputConnector(this, true);
        }

        public Node Node { get; internal set; }
        public object Tag { get; set; }

        public NodeConnector Input { get; private set; }
        public NodeConnector Output { get; private set; }
               
        public RectangleF ContentBounds
        {
            get;
            protected set;
        }
        public RectangleF PinBounds
        {
            get;
            protected set;
        }

        public RectangleF ItemBounds
        {
            get;
            protected set;
        }

        internal RenderState state = RenderState.None;

        public event EventHandler<NodeItemEventArgs> Clicked;

        public virtual bool OnClick()
        {
            if (Clicked != null)
            {
                Clicked(this, new NodeItemEventArgs(this));
                return true;
            }
            return false;

        }
        public virtual bool OnDoubleClick() { return false; }
        public virtual bool OnStartDrag(PointF location, out PointF original_location) { original_location = Point.Empty; return false; }
        public virtual bool OnDrag(PointF location) { return false; }
        public virtual bool OnEndDrag() { return false; }


        #region Rendering
        public abstract void RenderContent(Graphics graphics);

        public virtual void RenderPin(Graphics graphics)
        {
            using (var brush = new SolidBrush(GraphRenderer.GetArrowLineColor(state)))
            {
                graphics.FillEllipse(brush, PinBounds);
            }

            if (state == RenderState.None)
            {
                graphics.DrawEllipse(Pens.Black, PinBounds);
            }
            else
            // When we're compatible, but not dragging from this node we render a highlight
            if ((state & (RenderState.Compatible | RenderState.Dragging)) == RenderState.Compatible)
            {
                // First draw the normal black border
                graphics.DrawEllipse(Pens.Black, PinBounds);

                // Draw an additional highlight around the connector
                RectangleF highlightBounds = new RectangleF(PinBounds.X, PinBounds.Y, PinBounds.Width, PinBounds.Height);
                highlightBounds.Width += 10;
                highlightBounds.Height += 10;
                highlightBounds.X -= 5;
                highlightBounds.Y -= 5;
                graphics.DrawEllipse(Pens.OrangeRed, highlightBounds);
            }
            else
            {
                graphics.DrawArc(Pens.Black, PinBounds, 90, 180);
                using (var pen = new Pen(GraphRenderer.GetArrowLineColor(state)))
                {
                    graphics.DrawArc(pen, PinBounds, 270, 180);
                }
            }
        }

        public virtual void Render(Graphics graphics)
        {
            RenderContent(graphics);

            RenderPin(graphics);
        }
        #endregion


        #region Layouting
        public abstract SizeF MeasureContent(Graphics context);
        protected virtual SizeF MeasurePin(Graphics context)
        {
            return new SizeF(GraphConstants.ConnectorSize, GraphConstants.ConnectorSize);
        }
        public virtual SizeF MeasureItem(Graphics context)
        {
            var contentSize = MeasureContent(context);
            var pinSize = MeasurePin(context);

            SizeF totalSize = new SizeF();
            totalSize.Width = contentSize.Width + GraphConstants.PinSpacing + pinSize.Width;
            totalSize.Height = Math.Max(contentSize.Height, pinSize.Height);

            return totalSize;
        }

        public virtual void PerformLayout(Graphics context, PointF position)
        {
            var contentSize = MeasureContent(context);
            var pinSize = MeasurePin(context);                       

            ItemBounds = new RectangleF(position, MeasureItem(context));
        
            if (ItemType == NodeItemType.Input)
            {
                PinBounds = new RectangleF(position, pinSize);

                position.X += pinSize.Width + GraphConstants.PinSpacing;
                ContentBounds = new RectangleF(position, contentSize);
            }
            else if(ItemType == NodeItemType.Output)
            {
                ContentBounds = new RectangleF(position, contentSize);
                position.X += contentSize.Width + GraphConstants.PinSpacing;

                PinBounds = new RectangleF(position, pinSize);
            }
        }
        #endregion

        public ElementType ElementType { get { return ElementType.NodeItem; } }
    }
}
