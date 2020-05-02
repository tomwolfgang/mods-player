using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TPlayer {
	/// <summary>
	/// Summary description for ImageParser.
	/// </summary>
	public class ImageParser {
		private Size m_imageSize;
		public Size ImageSize	{
			get	{
				return m_imageSize;
			}
		}

		private int m_imageCount;
		public int ImageCount	{
			get	{
				return m_imageCount;
			}
		}

		private Image m_image;

//		private Color m_transparentColor;
//		public Color TransparentColor
//		{
//			get
//			{
//				return m_transparentColor;
//			}
//		}

		private ImageAttributes m_imageAtt;
		private Rectangle m_destRect = new Rectangle(0, 0, 0, 0);

		public ImageParser(Image image, int imagesCount, Size imageSize, Color transparentColor) {
			// check validity
			int imageWidth = imageSize.Width * imagesCount;

			if ((image.Width != imageWidth) || (image.Height != imageSize.Height)) {
				throw new ArgumentException("The width or height do not match the image.", "imageSize");
			}

			if (imagesCount < 1) {
				throw new ArgumentException("The count isn't valid.", "imagesCount");
			}

			m_image				= image;
			m_imageCount		= imagesCount;
			m_imageSize			= imageSize;

			m_imageAtt = new ImageAttributes();
			m_imageAtt.SetColorKey(transparentColor, transparentColor);	
		}

		public bool Draw(Graphics grph, int dstX, int dstY, int imageIndex)	{
			// check index validity
			if ((imageIndex < 0) || (imageIndex >= m_imageCount))	{
				throw new IndexOutOfRangeException("imageIndex out of range");
			}

			m_destRect.X = dstX;
			m_destRect.Y = dstY;
			m_destRect.Width = m_imageSize.Width;
			m_destRect.Height = m_imageSize.Height;
			
			int currentImageX = imageIndex * m_imageSize.Width;
			grph.DrawImage(m_image, m_destRect, currentImageX, 0, m_imageSize.Width, m_imageSize.Height, GraphicsUnit.Pixel, m_imageAtt);

			return true;
		}

		public bool Draw(Graphics grph, int dstX, int dstY, int dstWidth, int dstHeight, int imageIndex, Color blackColor) {
			// check index validity
			if ((imageIndex < 0) || (imageIndex >= m_imageCount)) {
				throw new IndexOutOfRangeException("imageIndex out of range");
			}

			m_imageAtt.ClearRemapTable();
			System.Drawing.Imaging.ColorMap[] map = new ColorMap[1]{new System.Drawing.Imaging.ColorMap()};
			map[0].NewColor = blackColor;
			map[0].OldColor = System.Drawing.Color.Black;
			m_imageAtt.SetRemapTable(map);


			m_destRect.X = dstX;
			m_destRect.Y = dstY;
			m_destRect.Width = dstWidth;
			m_destRect.Height = dstHeight;
			
			int currentImageX = imageIndex * m_imageSize.Width;
			grph.DrawImage(m_image, m_destRect, currentImageX, 0, m_imageSize.Width, m_imageSize.Height, GraphicsUnit.Pixel, m_imageAtt);

			return true;
		}

	}
}
