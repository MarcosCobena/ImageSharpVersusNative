using System;
using System.IO;
#if !ANDROID && !IOS
using System.Drawing;
using System.Drawing.Imaging;
#elif IOS
using CoreGraphics;
using Foundation;
using UIKit;
#endif
using VisualTestCommonLibrary.Images;

namespace ImageSharpVersusNativeShared
{
	public static class NativeImageLoading
	{
		public static byte[] PlatformSpecificLoading(Stream stream)
		{
#if ANDROID
			var bytes = ImageHelpers.GetRGBABytes(stream);

            return bytes;
#elif IOS
			int imageWidth, imageHeight;
			var bytes = GetRGBABytes(stream, out imageWidth, out imageHeight);

			return bytes;
#else
            using (var image = System.Drawing.Image.FromStream(stream))
            using (var bitmap = new Bitmap(image))
            {
				var bytes = ImageHelpers.GetRGBABytes(bitmap);

                return bytes;
            }
#endif
		}

#if IOS
		// Untouched from Wave Engine 2.x, WaveEngineiOS.Adapter: IOManager.cs
		private static byte[] GetRGBABytes(Stream imageStream, out int imageWidth, out int imageHeight)
		{
			byte[] outputData = null;

			using (var uiImage = UIImage.LoadFromData(NSData.FromStream(imageStream)))
			{
				var cgImage = uiImage.CGImage;

				imageWidth = (int)cgImage.Width;
				imageHeight = (int)cgImage.Height;

				outputData = new byte[imageWidth * imageHeight * 4];

				using (var colorSpace = CGColorSpace.CreateDeviceRGB())
				using (var bitmapContext = new CGBitmapContext(outputData, imageWidth, imageHeight, 8, imageWidth * 4, colorSpace, CGBitmapFlags.PremultipliedLast))
				{
					bitmapContext.DrawImage(new System.Drawing.RectangleF(0, 0, imageWidth, imageHeight), cgImage);
				}
			}

			return outputData;
		}
#endif
	}
}
