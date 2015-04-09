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
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
    public sealed class NodeLabelItem : NodeItem
    {
        public NodeLabelItem(string text, NodeItemType type) :
            base(type)
        {
            this.Text = text;
        }

        public NodeLabelItem(string text) :
            this(text, NodeItemType.Output)
        { }

        #region Text
        string internalText = string.Empty;
        public string Text
        {
            get { return internalText; }
            set
            {
                if (internalText == value)
                    return;
                internalText = value;
                TextSize = Size.Empty;
            }
        }
        #endregion

        internal SizeF TextSize;


        public override SizeF MeasureContent(Graphics graphics)
        {
            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                if (this.TextSize.IsEmpty)
                {
                    var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

                    if (this.Input != null)
                    {
                        this.TextSize = graphics.MeasureString(this.Text, SystemFonts.MenuFont, size, GraphConstants.LeftMeasureTextStringFormat);
                    }
                    else if (this.Output != null)
                    {
                        this.TextSize = graphics.MeasureString(this.Text, SystemFonts.MenuFont, size, GraphConstants.RightMeasureTextStringFormat);
                    }

                    else
                    {
                        this.TextSize = graphics.MeasureString(this.Text, SystemFonts.MenuFont, size, GraphConstants.CenterMeasureTextStringFormat);
                    }


                    this.TextSize.Width = Math.Max(size.Width, this.TextSize.Width);
                    this.TextSize.Height = Math.Max(size.Height, this.TextSize.Height);
                }
                return this.TextSize;
            }
            else
            {
                return new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
            }
        }

        public override void RenderContent(Graphics graphics)
        {            

            if (this.Input != null) 
            {
                graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, ContentBounds, GraphConstants.LeftTextStringFormat);
            }
            else if (this.Output != null)
            {
                graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, ContentBounds, GraphConstants.RightTextStringFormat);
            }
            else
            {
                graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, ContentBounds, GraphConstants.CenterTextStringFormat);
            }

        }
             
    }
}
