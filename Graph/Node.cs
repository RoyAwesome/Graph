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
using System.ComponentModel;
using Graph.Items;
using System.Drawing.Drawing2D;

namespace Graph
{
    public sealed class NodeEventArgs : EventArgs
    {
        public NodeEventArgs(Node node) { Node = node; }
        public Node Node { get; private set; }
    }

    public sealed class ElementEventArgs : EventArgs
    {
        public ElementEventArgs(IElement element) { Element = element; }
        public IElement Element { get; private set; }
    }

    public sealed class AcceptNodeEventArgs : CancelEventArgs
    {
        public AcceptNodeEventArgs(Node node) { Node = node; }
        public AcceptNodeEventArgs(Node node, bool cancel) : base(cancel) { Node = node; }
        public Node Node { get; private set; }
    }

    public sealed class AcceptElementLocationEventArgs : CancelEventArgs
    {
        public AcceptElementLocationEventArgs(IElement element, Point position) { Element = element; Position = position; }
        public AcceptElementLocationEventArgs(IElement element, Point position, bool cancel) : base(cancel) { Element = element; Position = position; }
        public IElement Element { get; private set; }
        public Point Position { get; private set; }
    }

    public partial class Node : IElement
    {
        public string Title
        {
            get;
            set;
        }

        #region Collapsed
        internal bool internalCollapsed;
        public bool Collapsed
        {
            get
            {
                return (internalCollapsed &&
                        ((state & RenderState.DraggedOver) == 0)) ||
                        HasNoItems;
            }
            set
            {
                var oldValue = Collapsed;
                internalCollapsed = value;                
            }
        }
        #endregion

        public bool HasNoItems { get { return Items.Count() == 0; } }

        public PointF Location { get; set; }
        public object Tag { get; set; }

        public IEnumerable<NodeConnection> Connections { get { return AllConnections; } }
        public IEnumerable<NodeItem> Items { get { return InputItems.Union(OutputItems); } }

        public RectangleF Bounds
        {
            get;
            protected set;
        }
        internal RectangleF inputBounds;
        internal RectangleF outputBounds;
        internal RectangleF itemsBounds;
        internal RenderState state = RenderState.None;
        internal RenderState inputState = RenderState.None;
        internal RenderState outputState = RenderState.None;

        public IEnumerable<NodeConnector> InputConnectors
        {
            get
            {
                return InputItems.Select(x => x.Connector);
            }
        }
        public IEnumerable<NodeConnector> OutputConnectors
        {
            get
            {
                return OutputItems.Select(x => x.Connector);
            }
        }
        internal List<NodeConnection> AllConnections
        {
            get;
            set;
        }

        public RectangleF HeaderBounds
        {
            get;
            set;
        }

     

        public List<NodeItem> InputItems
        {
            private set;
            get;
        }

        public List<NodeItem> OutputItems
        {
            private set;
            get;
        }

        public Node(string title)
        {
            this.Title = title;

            InputItems = new List<NodeItem>();
            OutputItems = new List<NodeItem>();
            AllConnections = new List<NodeConnection>();
        }

        public void AddItem(NodeItem item)
        {
            if (item.Node != null) item.Node.RemoveItem(item);
            item.Node = this;

            if (item.ItemType == NodeItemType.Output)
            {
                OutputItems.Add(item);
            }
            else if (item.ItemType == NodeItemType.Input)
            {
                InputItems.Add(item);
            }
        }

        public void RemoveItem(NodeItem item)
        {
            if (item.Node != this) return;
            item.Node = null;

            if (item.ItemType == NodeItemType.Output)
            {
                OutputItems.Remove(item);
            }
            else if (item.ItemType == NodeItemType.Input)
            {
                InputItems.Remove(item);
            }
        }

        // Returns true if there are some connections that aren't connected
        public bool AnyConnectorsDisconnected
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.ItemType == NodeItemType.Input && item.Connector.Enabled && !item.Connector.HasConnection)
                        return true;
                    if (item.ItemType == NodeItemType.Output && item.Connector.Enabled && !item.Connector.HasConnection)
                        return true;
                }
                return false;
            }
        }

        // Returns true if there are some output connections that aren't connected
        public bool AnyOutputConnectorsDisconnected
        {
            get
            {
                foreach (var item in OutputItems)
                    if (item.Connector.Enabled && !item.Connector.HasConnection)
                        return true;
                return false;
            }
        }

        // Returns true if there are some input connections that aren't connected
        public bool AnyInputConnectorsDisconnected
        {
            get
            {
                foreach (var item in InputItems)
                    if (item.Connector.Enabled && !item.Connector.HasConnection)
                        return true;
                return false;
            }
        }

        public ElementType ElementType { get { return ElementType.Node; } }


    }
}
