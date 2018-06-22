using System;
using System.IO;
#if !ANDROID && !IOS && !WINDOWS_UWP
using System.Drawing;
using System.Drawing.Imaging;
#elif IOS
using CoreGraphics;
using Foundation;
using UIKit;
#elif WINDOWS_UWP
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
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
#elif WINDOWS_UWP
            var bytes = GetRGBABytes(stream);

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
#elif WINDOWS_UWP
        public static byte[] GetRGBABytes(Stream imageStream)
        {
            byte[] result = Task.Run(() => GetRGBABytesAsync(imageStream, CancellationToken.None)).Result;

            return result;
        }

        private static async Task<byte[]> GetRGBABytesAsync(Stream imageStream, CancellationToken cancellationToken)
        {
            byte[] result;
            bool overrideStream = !imageStream.CanSeek;
            Stream stream;

            if (overrideStream)
            {
                stream = new MemoryStream();
                imageStream.CopyTo(stream);
            }
            else
            {
                stream = imageStream;
            }

            using (var ras = stream.AsRandomAccessStream())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(ras).AsTask(cancellationToken).ConfigureAwait(false);
                BitmapFrame frame = await decoder.GetFrameAsync(0).AsTask(cancellationToken).ConfigureAwait(false);

                BitmapTransform transform = new BitmapTransform()
                {
                    ScaledWidth = decoder.PixelWidth,
                    ScaledHeight = decoder.PixelHeight
                };

                PixelDataProvider pixelData = await frame.GetPixelDataAsync(
                    BitmapPixelFormat.Rgba8,
                    BitmapAlphaMode.Premultiplied,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage)
                    .AsTask(cancellationToken).ConfigureAwait(false);

                result = pixelData.DetachPixelData();
            }

            if (overrideStream)
            {
                stream.Dispose();
                stream = null;
            }

            return result;
        }
#endif
    }
}
