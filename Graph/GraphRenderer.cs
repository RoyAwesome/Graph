﻿﻿#region License
// Copyright (c) 2009 Sander van Rossen, 2013 Oliver Salzburg
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
using System.Drawing.Drawing2D;

namespace Graph
{
	public static class GraphRenderer
	{
		
		private static Pen BorderPen = new Pen(Color.FromArgb(64, 64, 64));

		internal static void RenderConnector(Graphics graphics, RectangleF bounds, RenderState state)
		{
			
		}

		internal static void RenderArrow(Graphics graphics, RectangleF bounds, RenderState connectionState)
		{
			var x = (bounds.Left + bounds.Right) / 2.0f;
			var y = (bounds.Top + bounds.Bottom) / 2.0f;
			using (var brush = new SolidBrush(GetArrowLineColor(connectionState | RenderState.Connected)))
			{
				graphics.FillPolygon(brush, GetArrowPoints(x,y), FillMode.Winding);
			}
		}

		public static void PerformLayout(Graphics graphics, IEnumerable<Node> nodes)
		{
			foreach (var node in nodes.Reverse<Node>())
			{
				GraphRenderer.PerformLayout(graphics, node);
			}
		}

		public static void Render(Graphics graphics, IEnumerable<Node> nodes, bool showLabels)
		{
			var skipConnections = new HashSet<NodeConnection>();
			foreach (var node in nodes.Reverse<Node>())
			{
				GraphRenderer.RenderConnections(graphics, node, skipConnections, showLabels);
			}
			foreach (var node in nodes.Reverse<Node>())
			{
				GraphRenderer.Render(graphics, node);
			}
		}

		public static void PerformLayout(Graphics graphics, Node node)
		{
            node.PerformLayout(graphics);
		}

		static void Render(Graphics graphics, Node node)
		{
            node.Render(graphics);
		}

		public static void RenderConnections(Graphics graphics, Node node, HashSet<NodeConnection> skipConnections, bool showLabels)
		{
			foreach (var connection in node.AllConnections.Reverse<NodeConnection>())
			{
				if (connection == null ||
					connection.From == null ||
					connection.To == null)
					continue;

				if (skipConnections.Add(connection))
				{
					var to		= connection.To;
					var from	= connection.From;
					RectangleF toBounds;
					RectangleF fromBounds;
					if (to.Node.Collapsed)		toBounds = to.Node.inputBounds;
					else						toBounds = to.Bounds;
					if (from.Node.Collapsed)	fromBounds = from.Node.outputBounds;
					else						fromBounds = from.Bounds;

					var x1 = (fromBounds.Left + fromBounds.Right) / 2.0f;
					var y1 = (fromBounds.Top + fromBounds.Bottom) / 2.0f;
					var x2 = (toBounds.Left + toBounds.Right) / 2.0f;
					var y2 = (toBounds.Top + toBounds.Bottom) / 2.0f;

					float centerX;
					float centerY;
					using (var path = GetArrowLinePath(x1, y1, x2, y2, out centerX, out centerY, false))
					{
                        using (var brush = new SolidBrush(from.Item.MainColor))
						{
							graphics.FillPath(brush, path);
						}
						connection.bounds = path.GetBounds();
					}

					if (showLabels &&
						!string.IsNullOrWhiteSpace(connection.Name))
					{
						var center = new PointF(centerX, centerY);
						RenderLabel(graphics, connection, center, connection.state);
					}
				}
			}
		}

		static void RenderLabel(Graphics graphics, NodeConnection connection, PointF center, RenderState state)
		{
			using (var path = new GraphicsPath(FillMode.Winding))
			{			
				int cornerSize			= (int)GraphConstants.CornerSize * 2;
				int connectorSize		= (int)GraphConstants.ConnectorSize;
				int halfConnectorSize	= (int)Math.Ceiling(connectorSize / 2.0f);

				SizeF size;
				PointF position;
				var text		= connection.Name;
				if (connection.textBounds.IsEmpty ||
					connection.textBounds.Location != center)
				{
					size		= graphics.MeasureString(text, SystemFonts.StatusFont, center, GraphConstants.CenterTextStringFormat);
					position	= new PointF(center.X - (size.Width / 2.0f) - halfConnectorSize, center.Y - (size.Height / 2.0f));
					size.Width	+= connectorSize;
					connection.textBounds = new RectangleF(position, size);
				} else
				{
					size		= connection.textBounds.Size;
					position	= connection.textBounds.Location;
				}

				var halfWidth  = size.Width / 2.0f;
				var halfHeight = size.Height / 2.0f;
				var connectorOffset		= (int)Math.Floor((GraphConstants.MinimumItemHeight - GraphConstants.ConnectorSize) / 2.0f);
				var left				= position.X;
				var top					= position.Y;
				var right				= position.X + size.Width;
				var bottom				= position.Y + size.Height;
				path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
				path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);

				path.AddArc(right - cornerSize, bottom - cornerSize, cornerSize, cornerSize, 0, 90);
				path.AddArc(left, bottom - cornerSize, cornerSize, cornerSize, 90, 90);
				path.CloseFigure();

				using (var brush = new SolidBrush(GetArrowLineColor(state)))
				{
					graphics.FillPath(brush, path);
				}
				graphics.DrawString(text, SystemFonts.StatusFont, Brushes.Black, center, GraphConstants.CenterTextStringFormat);

				if (state == RenderState.None)
					graphics.DrawPath(Pens.Black, path);

				//graphics.DrawRectangle(Pens.Black, connection.textBounds.Left, connection.textBounds.Top, connection.textBounds.Width, connection.textBounds.Height);
			}
		}

		public static Region GetConnectionRegion(NodeConnection connection)
		{
			var to		= connection.To;
			var from	= connection.From;
			RectangleF toBounds;
			RectangleF fromBounds;
			if (to.Node.Collapsed)		toBounds = to.Node.inputBounds;
			else						toBounds = to.Bounds;
			if (from.Node.Collapsed)	fromBounds = from.Node.outputBounds;
			else						fromBounds = from.Bounds;

			var x1 = (fromBounds.Left + fromBounds.Right) / 2.0f;
			var y1 = (fromBounds.Top + fromBounds.Bottom) / 2.0f;
			var x2 = (toBounds.Left + toBounds.Right) / 2.0f;
			var y2 = (toBounds.Top + toBounds.Bottom) / 2.0f;

			Region region;
			float centerX;
			float centerY;
			using (var linePath = GetArrowLinePath(	x1, y1, x2, y2, out centerX, out centerY, true, 5.0f))
			{
				region = new Region(linePath);
			}
			return region;
		}

		public static Color GetArrowLineColor(RenderState state)
		{
			if ((state & (RenderState.Hover | RenderState.Dragging)) != 0)
			{
				if ((state & RenderState.Incompatible) != 0)
				{
					return Color.Red;
				} else
				if ((state & RenderState.Compatible) != 0)
				{
					return Color.DarkOrange;
				} else
				if ((state & RenderState.Dragging) != 0)
					return Color.SteelBlue;
				else
					return Color.DarkOrange;
			} else
			if ((state & RenderState.Incompatible) != 0)
			{
				return Color.Gray;
			} else
			if ((state & RenderState.Compatible) != 0)
			{
				return Color.White;
			} else
			if ((state & RenderState.Connected) != 0)
			{
				return Color.Black;
			} else
				return Color.LightGray;
		}

        public static PointF[] GetArrowPoints(float x, float y, float extra_thickness = 0)
		{
			return new PointF[]{
					new PointF(x - (GraphConstants.ConnectorSize + 1.0f) - extra_thickness, y + (GraphConstants.ConnectorSize / 1.5f) + extra_thickness),
					new PointF(x + 1.0f + extra_thickness, y),
					new PointF(x - (GraphConstants.ConnectorSize + 1.0f) - extra_thickness, y - (GraphConstants.ConnectorSize / 1.5f) - extra_thickness)};
		}

        public static List<PointF> GetArrowLinePoints(float x1, float y1, float x2, float y2, out float centerX, out float centerY, float extra_thickness = 0)
		{
			var widthX	= (x2 - x1);
			var lengthX = Math.Max(60, Math.Abs(widthX / 2)) 
				//+ Math.Max(0, -widthX / 2)
				;
			var lengthY = 0;// Math.Max(-170, Math.Min(-120.0f, widthX - 120.0f)) + 120.0f; 
			if (widthX < 120)
				lengthX = 60;
			var yB = ((y1 + y2) / 2) + lengthY;// (y2 + ((y1 - y2) / 2) * 0.75f) + lengthY;
			var yC = y2 + yB;
			var xC = (x1 + x2) / 2;
			var xA = x1 + lengthX;
			var xB = x2 - lengthX;

			/*
			if (widthX >= 120)
			{
				xA = xB = xC = x2 - 60;
			}
			//*/
			
			var points = new List<PointF> { 
				new PointF(x1, y1),
				new PointF(xA, y1),
				new PointF(xB, y2),
				new PointF(x2 - GraphConstants.ConnectorSize - extra_thickness, y2)
			};

			var t  = 1.0f;//Math.Min(1, Math.Max(0, (widthX - 30) / 60.0f));
			var yA = (yB * t) + (yC * (1 - t));

			if (widthX <= 120)
			{
				points.Insert(2, new PointF(xB, yA));
				points.Insert(2, new PointF(xC, yA));
				points.Insert(2, new PointF(xA, yA));
			}
			//*
			using (var tempPath = new GraphicsPath())
			{
				tempPath.AddBeziers(points.ToArray());
				tempPath.Flatten();
				points = tempPath.PathPoints.ToList();
			}
			//*/
			var angles	= new PointF[points.Count - 1];
			var lengths = new float[points.Count - 1];
			float totalLength = 0;
			centerX = 0;
			centerY = 0;
			points.Add(points[points.Count - 1]);
			for (int i = 0; i < points.Count - 2; i++)
			{
				var pt1 = points[i];
				var pt2 = points[i + 1];
				var pt3 = points[i + 2];
				var deltaX = (float)((pt2.X - pt1.X) + (pt3.X - pt2.X));
				var deltaY = (float)((pt2.Y - pt1.Y) + (pt3.Y - pt2.Y));
				var length = (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
				if (length <= 1.0f)
				{
					points.RemoveAt(i);
					i--;
					continue;
				}
				lengths[i] = length;
				totalLength += length;
				angles[i].X = deltaX / length;
				angles[i].Y = deltaY / length;
			}

			float midLength		= (totalLength / 2.0f);// * 0.75f;
			float startWidth	= extra_thickness + 0.75f;
			float endWidth		= extra_thickness + (GraphConstants.ConnectorSize / 3.5f);
			float currentLength = 0;
			var newPoints = new List<PointF>();
			newPoints.Add(points[0]);
			for (int i = 0; i < points.Count - 2; i++)
			{
				var angle	= angles[i];
				var point	= points[i + 1];
				var length	= lengths[i];
				var width	= (((currentLength * (endWidth - startWidth)) / totalLength) + startWidth);
				var angleX	= angle.X * width;
				var angleY	= angle.Y * width;

				var newLength = currentLength + length;
				if (currentLength	<= midLength &&
					newLength		>= midLength)
				{
					var dX = point.X - points[i].X;
					var dY = point.Y - points[i].Y;
					var t1 = midLength - currentLength;
					var l  = length;



					centerX = points[i].X + ((dX * t1) / l);
					centerY = points[i].Y + ((dY * t1) / l);
				}

				var pt1 = new PointF(point.X - angleY, point.Y + angleX);
				var pt2 = new PointF(point.X + angleY, point.Y - angleX);
				if (Math.Abs(newPoints[newPoints.Count - 1].X - pt1.X) > 1.0f ||
					Math.Abs(newPoints[newPoints.Count - 1].Y - pt1.Y) > 1.0f)
					newPoints.Add(pt1);
				if (Math.Abs(newPoints[0].X - pt2.X) > 1.0f ||
					Math.Abs(newPoints[0].Y - pt2.Y) > 1.0f)
					newPoints.Insert(0, pt2);

				currentLength = newLength;
			}

			return newPoints;
		}

		static GraphicsPath GetArrowLinePath(float x1, float y1, float x2, float y2, out float centerX, out float centerY, bool include_arrow, float extra_thickness = 0)
		{
			var newPoints = GetArrowLinePoints(x1, y1, x2, y2, out centerX, out centerY, extra_thickness);

			var path = new GraphicsPath(FillMode.Winding);
			path.AddLines(newPoints.ToArray());
			if (include_arrow)
				path.AddLines(GetArrowPoints(x2, y2, extra_thickness).ToArray());
			path.CloseFigure();
			return path;
		}

		public static void RenderOutputConnection(Graphics graphics, NodeConnector output, float x, float y, RenderState state)
		{
			if (graphics == null ||
				output == null)
				return;
			
			RectangleF outputBounds;
			if (output.Node.Collapsed)	outputBounds = output.Node.outputBounds;
			else						outputBounds = output.Bounds;

			var x1 = (outputBounds.Left + outputBounds.Right) / 2.0f;
			var y1 = (outputBounds.Top + outputBounds.Bottom) / 2.0f;
			
			float centerX;
			float centerY;
			using (var path = GetArrowLinePath(x1, y1, x, y, out centerX, out centerY, true, 0.0f))
			{
				using (var brush = new SolidBrush(GetArrowLineColor(state)))
				{
					graphics.FillPath(brush, path);
				}
			}
		}
		
		public static void RenderInputConnection(Graphics graphics, NodeConnector input, float x, float y, RenderState state)
		{
			if (graphics == null || 
				input == null)
				return;
			
			RectangleF inputBounds;
			if (input.Node.Collapsed)	inputBounds = input.Node.inputBounds;
			else						inputBounds = input.Bounds;

			var x2 = (inputBounds.Left + inputBounds.Right) / 2.0f;
			var y2 = (inputBounds.Top + inputBounds.Bottom) / 2.0f;

			float centerX;
			float centerY;
			using (var path = GetArrowLinePath(x, y, x2, y2, out centerX, out centerY, true, 0.0f))
			{
				using (var brush = new SolidBrush(GetArrowLineColor(state)))
				{
					graphics.FillPath(brush, path);
				}
			}
		}

		public static GraphicsPath CreateRoundedRectangle(SizeF size, PointF location)
		{
			int cornerSize			= (int)GraphConstants.CornerSize * 2;
			int connectorSize		= (int)GraphConstants.ConnectorSize;
			int halfConnectorSize	= (int)Math.Ceiling(connectorSize / 2.0f);

			var height				= size.Height;
			var width				= size.Width;
			var halfWidth			= width / 2.0f;
			var halfHeight			= height / 2.0f;
			var connectorOffset		= (int)Math.Floor((GraphConstants.MinimumItemHeight - GraphConstants.ConnectorSize) / 2.0f);
			var left				= location.X;
			var top					= location.Y;
			var right				= location.X + width;
			var bottom				= location.Y + height;

			var path = new GraphicsPath(FillMode.Winding);
			path.AddArc(left, top, cornerSize, cornerSize, 180, 90);
			path.AddArc(right - cornerSize, top, cornerSize, cornerSize, 270, 90);

			path.AddArc(right - cornerSize, bottom - cornerSize, cornerSize, cornerSize, 0, 90);
			path.AddArc(left, bottom - cornerSize, cornerSize, cornerSize, 90, 90);
			path.CloseFigure();
			return path;
		}
	}
}
