using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FortyTwoLib
{
	public class DominoImage
	{
		private double aspectRatio = 1.922509225092;

		private static Point[][] patterns;
		private static Color[] colors;
		private static Dictionary<string, byte[]> cache;

		static DominoImage()
		{
			cache = new Dictionary<string, byte[]>();

			colors = new[] { Color.White, Color.DarkRed, Color.DarkGreen, Color.DarkBlue, Color.DarkOrange, Color.Purple, Color.Gold };

			patterns = new Point[7][];

			patterns[0] = new Point[0];

			patterns[1] = new Point[1];
			patterns[1][0] = new Point(1, 1);

			patterns[2] = new Point[2];
			patterns[2][0] = new Point(2, 0);
			patterns[2][1] = new Point(0, 2);

			patterns[3] = new Point[3];
			patterns[3][0] = new Point(0, 2);
			patterns[3][1] = new Point(1, 1);
			patterns[3][2] = new Point(2, 0);

			patterns[4] = new Point[4];
			patterns[4][0] = new Point(0, 0);
			patterns[4][1] = new Point(2, 0);
			patterns[4][2] = new Point(0, 2);
			patterns[4][3] = new Point(2, 2);

			patterns[5] = new Point[5];
			patterns[5][0] = new Point(0, 0);
			patterns[5][1] = new Point(2, 0);
			patterns[5][3] = new Point(0, 2);
			patterns[5][2] = new Point(2, 2);
			patterns[5][4] = new Point(1, 1);

			patterns[6] = new Point[6];
			patterns[6][0] = new Point(0, 0);
			patterns[6][1] = new Point(1, 0);
			patterns[6][2] = new Point(2, 0);
			patterns[6][3] = new Point(0, 2);
			patterns[6][4] = new Point(1, 2);
			patterns[6][5] = new Point(2, 2);
		}

		public byte[] GetImage(string dots, int requestedWidth, bool horizontal)
		{
			string key = dots + ":" + requestedWidth.ToString() + ":" + horizontal.ToString();

			byte[] ret;
			if ( cache.TryGetValue(key, out ret) )
			{
				return ret;
			}

			int width = 200;
			int height = (int) (width / aspectRatio);
			int shift = (int) (0.06 * height);

			using (var bitmap = new Bitmap(width, height))
			{
				var graphics = Graphics.FromImage(bitmap);
				var clearBrush = new SolidBrush(Color.Cyan);
				var blackBrush = new SolidBrush(Color.Black);
				var whiteBrush = new SolidBrush(Color.White);
				var faceBrush = new SolidBrush(Color.Beige);
				var blackPen = new Pen(Color.Black);
				var whitePen = new Pen(Color.White);
				var rectFace = new Rectangle(1, 1, width - shift - 1, height - shift - 1);
				graphics.FillRectangle(clearBrush, 0, 0, width, height);
				if ( !dots.StartsWith(" ") )
				{
					graphics.FillRectangle(blackBrush, shift + 1, shift + 1, width - shift - 1, height - shift - 1);
					graphics.FillRectangle(whiteBrush, 0, 0, width - shift - 1, height - shift - 1);
					graphics.FillRectangle(faceBrush, rectFace);
					int lineX = 1 + ((width - shift - 1) / 2);
					graphics.DrawLine(blackPen, lineX, 1, lineX, height - shift - 1);
					graphics.DrawLine(whitePen, lineX + 1, 1, lineX + 1, height - shift - 1);

					int cellSize = (int) ((height - shift - 1) / 4);
					int xShift = rectFace.Left + (((rectFace.Width / 2) - ((3 * rectFace.Height) / 4)) / 2);
					int yShift = rectFace.Top + (rectFace.Height / 8);

					DrawSide(graphics, dots[0] - '0', cellSize, xShift, yShift);
					DrawSide(graphics, dots[1] - '0', cellSize, rectFace.Left + (rectFace.Width / 2) + (xShift - rectFace.Left), yShift);
				} else
				{
					graphics.DrawRectangle(blackPen, 0, 0, width + 1, height + 1);
					graphics.DrawRectangle(whitePen, -1, -1, width - 1, height - 1);
				}
				bitmap.MakeTransparent(Color.Cyan);

				var resizedImage = new Bitmap(requestedWidth, (int)(requestedWidth / aspectRatio));
				var g = Graphics.FromImage(resizedImage);
				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.SmoothingMode = SmoothingMode.HighQuality;
				g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				g.CompositingQuality = CompositingQuality.HighQuality;
				g.DrawImage(bitmap, 0, 0, requestedWidth, (int)(requestedWidth / aspectRatio));

				if ( dots == " ~" )
				{
					var s = "Drop here";
					var f = new Font(FontFamily.GenericSansSerif, 8f, FontStyle.Bold);
					var sizef = g.MeasureString(s, f);
					float x = (resizedImage.Size.Width - sizef.Width) / 2.0f;
					float y = (resizedImage.Size.Height - sizef.Height) / 2.0f;
					var redBrush = new SolidBrush(Color.Red);
					g.FillRectangle(redBrush, x - 2, y - 2, sizef.Width + 6, sizef.Height + 2);
					g.DrawString(s, f, whiteBrush, x, y);
				}

				using ( var memStream = new MemoryStream() )
				{
					if ( !horizontal ) resizedImage.RotateFlip(RotateFlipType.Rotate90FlipX);
					resizedImage.Save(memStream, ImageFormat.Png);
					ret = memStream.ToArray();
					cache.Add(key, ret);
					return ret;
				}
			}
		}

		private void DrawSide(Graphics graphics, int dots, int cellSize, int xShift, int yShift)
		{
			for ( int dot = 0; dot < dots; dot++)
			{
				Point p = patterns[dots][dot];
				var rect = new Rectangle(xShift + (p.X * cellSize), yShift + (p.Y * cellSize), cellSize, cellSize);
				var colorBrush = new SolidBrush(colors[dots]);
				var whiteBrush = new SolidBrush(Color.White);
				graphics.FillEllipse(whiteBrush, rect.Left + 1, rect.Top + 1, cellSize, cellSize);
				graphics.FillEllipse(colorBrush, rect);
			}
		}
	}
}
