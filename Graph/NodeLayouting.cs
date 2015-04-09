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
        protected SizeF HeaderSize = new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.HeaderHeight);

        virtual public SizeF Measure(Graphics graphics)
        {
            SizeF size = Size.Empty;
            size.Height = GraphConstants.BottomHeight;
            size.Height += GraphConstants.HeaderTopSpacing;
            size.Height += HeaderSize.Height;
            size.Height += GraphConstants.HeaderBottomSpacing;
           
            

            SizeF InputColumnSize = new SizeF();
            //Measure the Input Items
            foreach(var item in InputItems)
            {
                var itemSize = item.MeasureItem(graphics); 
                InputColumnSize.Width = Math.Max(size.Width, itemSize.Width);
                InputColumnSize.Height += GraphConstants.ItemSpacing + itemSize.Height;
            }

            //Measure the output items
            SizeF OutputColumnSize = new SizeF();
            foreach (var item in OutputItems)
            {
                var itemSize = item.MeasureItem(graphics);
                OutputColumnSize.Width = Math.Max(size.Width, itemSize.Width);
                OutputColumnSize.Height += GraphConstants.ItemSpacing + itemSize.Height;
            }

            //Select the height that is the largest.  
            size.Height += Math.Max(InputColumnSize.Height, OutputColumnSize.Height);

            //Add together each column's width
            size.Width += InputColumnSize.Width + GraphConstants.HorizontalColumnSpacing + OutputColumnSize.Width;
            size.Width = Math.Max(size.Width, HeaderSize.Width); //And select what is larger

          
            if (Collapsed)
                size.Height -= GraphConstants.ItemSpacing;

            size.Width += GraphConstants.NodeExtraWidth;
                      
            return size;

        }

        virtual protected void LayoutItems(Graphics graphics, PointF ItemPosition)
        {
            var itemPosition = ItemPosition;

            float RightWidth = 0;
            foreach(var item in InputItems)
            {
                item.PerformLayout(graphics, itemPosition); 
                itemPosition.Y += item.ItemBounds.Height + GraphConstants.ItemSpacing;

                RightWidth = Math.Max(RightWidth, item.ItemBounds.Width);


            }

            itemPosition = ItemPosition;
            itemPosition.X += RightWidth + GraphConstants.HorizontalColumnSpacing;

            foreach (var item in OutputItems)
            {
                item.PerformLayout(graphics, itemPosition);
                itemPosition.Y += item.ItemBounds.Height + GraphConstants.ItemSpacing;


            }
    }

        virtual public void PerformLayout(Graphics graphics)
        {
            var nodeSize = Measure(graphics);
            var position = Location;
            Bounds = new RectangleF(position, nodeSize);

            HeaderBounds = new RectangleF(position.X, position.Y + GraphConstants.HeaderTopSpacing, 
                nodeSize.Width, HeaderSize.Height);
           
            itemsBounds = new RectangleF(position.X, position.Y + HeaderBounds.Height + GraphConstants.HeaderBottomSpacing, 
                nodeSize.Width, nodeSize.Height - HeaderSize.Height - GraphConstants.HeaderBottomSpacing);

            var itemPosition = position;
            itemPosition.X += GraphConstants.ConnectorSize + (int)GraphConstants.HorizontalSpacing;

            itemPosition.Y += HeaderSize.Height + GraphConstants.HeaderBottomSpacing; //Move the itemPosition below the header
           
            LayoutItems(graphics, itemPosition);
          

        }


    }
}
