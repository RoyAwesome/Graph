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

        internal RectangleF bounds;
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
        public abstract SizeF Measure(Graphics context);
        public abstract void Render(Graphics graphics, SizeF minimumSize, PointF position);

        public abstract void RenderPin(Graphics graphics, SizeF boundingBox, PointF position);

        public ElementType ElementType { get { return ElementType.NodeItem; } }
    }
}
