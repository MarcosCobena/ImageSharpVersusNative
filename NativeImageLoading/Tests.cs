using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp;
using Xunit;

namespace ImageSharpVersusNativeShared
{
	public class Tests
    {
        private const int _iterations = 1000;

        [Fact]
        public void EveryStrategyReturnsTheSameBytes()
        {
            var imageSharpBytes = ImageSharpLoading();
			var platformSpecificBytes = NativeImageLoading.PlatformSpecificLoading(GetImageStream());

            Assert.Equal(platformSpecificBytes, imageSharpBytes);
        }

        [Fact]
        public void GetBytesThroughImageSharpMultipleTimesAndCalcItsAverage()
        {
            RunMultipleIterationsAndTakeStatisticsFor(ImageSharpLoading);
        }

        [Fact]
        public void GetBytesThroughSystemDrawingMultipleTimesAndCalcItsAverage()
        {
            RunMultipleIterationsAndTakeStatisticsFor(
                () => NativeImageLoading.PlatformSpecificLoading(GetImageStream()));
        }

        private Stream GetImageStream()
        {
            var assembly = typeof(Tests).GetTypeInfo().Assembly;
            var stream = assembly.GetManifestResourceStream("ImageSharpVersusNative.crate.bmp");

            return stream;
        }

        private byte[] ImageSharpLoading()
        {
            var stream = GetImageStream();

            using (var image = SixLabors.ImageSharp.Image.Load(stream))
            {
                var bytes = image.SavePixelData();

                return bytes;
            }
        }

        private void RunMultipleIterationsAndTakeStatisticsFor(Func<byte[]> throughStrategy)
        {
            var stopwatch = new Stopwatch();
            long[] elapsedMillisecondsArray = new long[_iterations];

            for (int i = 0; i < _iterations; i++)
            {
                stopwatch.Start();
                throughStrategy();
                stopwatch.Stop();

                elapsedMillisecondsArray[i] = stopwatch.ElapsedMilliseconds;

                stopwatch.Reset();
            }

            var average = elapsedMillisecondsArray.Average();
            var max = elapsedMillisecondsArray.Max();
            var min = elapsedMillisecondsArray.Min();

            Assert.True(false, $"Average: {average} ms\nMax.: {max} ms\nMin.: {min} ms");
        }
    }
}
