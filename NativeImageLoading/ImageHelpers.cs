// Untouched from Wave Engine 3, Commit cb5b3a67
using System;
using System.IO;
using System.Runtime.InteropServices;

#if WINDOWS || LINUX || MAC
using System.Drawing;
using System.Drawing.Imaging;
#elif ANDROID
using Android.Graphics;
using Java.Nio;
#elif UWP
using Windows.Graphics.Imaging;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace VisualTestCommonLibrary.Images
{
    public static class ImageHelpers
    {
        /// <summary>
        /// Read int16 from binaryReader
        /// </summary>
        /// <param name="binaryReader">binary reader</param>
        /// <returns>int16 data</returns>
        public static short ReadLittleEndianInt16(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(short)];

            for (int i = 0; i < sizeof(short); i += 1)
            {
                bytes[sizeof(short) - 1 - i] = binaryReader.ReadByte();
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Read UInt16 from binary reader
        /// </summary>
        /// <param name="binaryReader">binary reader</param>
        /// <returns>uint16 data</returns>
        public static ushort ReadLittleEndianUInt16(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(ushort)];

            for (int i = 0; i < sizeof(ushort); i += 1)
            {
                bytes[sizeof(ushort) - 1 - i] = binaryReader.ReadByte();
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Read int32 from binary reader
        /// </summary>
        /// <param name="binaryReader">binary reader</param>
        /// <returns>int32 data</returns>
        public static int ReadLittleEndianInt32(BinaryReader binaryReader)
        {
            byte[] bytes = new byte[sizeof(int)];
            for (int i = 0; i < sizeof(int); i += 1)
            {
                bytes[sizeof(int) - 1 - i] = binaryReader.ReadByte();
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Starts with
        /// </summary>
        /// <param name="thisBytes">source byte array</param>
        /// <param name="thatBytes">pattern byte array</param>
        /// <returns>if thisBytes start with thatBytes</returns>
        public static bool StartsWith(byte[] thisBytes, byte[] thatBytes)
        {
            for (int i = 0; i < thatBytes.Length; i += 1)
            {
                if (thisBytes[i] != thatBytes[i])
                {
                    return false;
                }
            }

            return true;
        }

#if WINDOWS || LINUX || MAC
        /// <summary>
        /// Gets the rgba bytes from an image stream. (IOManager)
        /// </summary>
        /// <param name="imageStream">The source image stream.</param>
        /// <returns>An array containing the premultiplied RGBA bytes of the raw image</returns>
        public static byte[] GetRGBABytes(Stream imageStream)
        {
            byte[] outputData = null;

            using (var sourceImage = Bitmap.FromStream(imageStream))
            using (Bitmap sourceBitmap = new Bitmap(sourceImage))
            {
                outputData = GetRGBABytes(sourceBitmap);
            }

            return outputData;
        }

        public static byte[] GetRGBABytes(Bitmap sourceBitmap)
        {
            byte[] outputData = null;

            Rectangle imageRect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);

            BitmapData lockedData = sourceBitmap.LockBits(imageRect, ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);

            int sizeInBytes = imageRect.Width * imageRect.Height * 4;

            outputData = new byte[sizeInBytes];

            // Copy bitmap data to byte array
            Marshal.Copy(lockedData.Scan0, outputData, 0, sizeInBytes);

            // BGRA to RGBA          
            outputData = FromBGRA32ToRGBA32(ref outputData);

            sourceBitmap.UnlockBits(lockedData);

            return outputData;
        }            
#elif ANDROID
        public static byte[] GetRGBABytes(Stream imageStream)
        {
            byte[] outputData = null;

            using (Bitmap image = BitmapFactory.DecodeStream(
                imageStream,
                null,
                new BitmapFactory.Options { InScaled = false, InDither = false, InJustDecodeBounds = false, InPurgeable = true, InInputShareable = true, }))
            {
                outputData = GetRGBABytes(image);
            }

            return outputData;
        }

        public static byte[] GetRGBABytes(Bitmap sourceBitmap)
        {
            byte[] outputData = null;
         
            var bytesize = sourceBitmap.RowBytes * sourceBitmap.Height; // or bitmap.ByteCount if target is  api 12 or later
            var buffer = ByteBuffer.Allocate(bytesize);
            sourceBitmap.CopyPixelsToBuffer(buffer);
            buffer.Rewind();
            outputData = new byte[bytesize];
            buffer.Get(outputData);

            return outputData;
        }
#elif UWP

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

        public static byte[] FromBGRA32ToRGBA32(ref byte[] bytes)
        {
            // BGRA to RGBA
            for (int k = 0; k < bytes.Length; k += 4)
            {
                var sourceBlue = bytes[k];
                bytes[k] = bytes[k + 2];
                bytes[k + 2] = sourceBlue;
            }

            return bytes;
        }

        public static byte[] FromRGB24ToRGBA32(byte[] bytes, int width, int height)
        {
            // RGB to RGBA
            byte[] rgba = new byte[width * height * 4];

            int index = 0;
            for (int i = 0; i < bytes.Length; i += 3)
            {
                rgba[index++] = bytes[i];
                rgba[index++] = bytes[i + 1];
                rgba[index++] = bytes[i + 2];
                rgba[index++] = 1;
            }

            return rgba;
        }

        public static byte[] FromBGR24ToRGBA32(byte[] bytes, int width, int height)
        {
            // BGR to RGBA
            byte[] rgba = new byte[width * height * 4];

            int index = 0;
            for (int i = 0; i < bytes.Length; i += 3)
            {
                rgba[index++] = bytes[i + 2];
                rgba[index++] = bytes[i + 1];
                rgba[index++] = bytes[i];
                rgba[index++] = 1;
            }

            return rgba;
        }
    }
}
