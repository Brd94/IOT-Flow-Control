using System;
using System.Threading.Tasks;

namespace Grid_EYE_Visualizer
{

    public class Interpolation
    {
        public static float InterpolateCubic(float v0, float v1, float v2, float v3, float fraction)
        {
            float p = (v3 - v2) - (v0 - v1);
            float q = (v0 - v1) - p;
            float r = v2 - v0;

            return (fraction * ((fraction * ((fraction * p) + q)) + r)) + v1;
        }
        public static float[,] BicubicInterpolation(float[,] data, int outWidth, int outHeight)
        {
            if (outWidth < 1 || outHeight < 1)
            {
                throw new ArgumentException();
            }


            int rowsPerChunk = 6000 / outWidth;
            if (rowsPerChunk == 0)
            {
                rowsPerChunk = 1;
            }

            int chunkCount = (outHeight / rowsPerChunk)
                             + (outHeight % rowsPerChunk != 0 ? 1 : 0);

            var width = data.GetLength(1);
            var height = data.GetLength(0);
            var ret = new float[outHeight, outWidth];

            Parallel.For(0, chunkCount, (chunkNumber) =>
            {
                int jStart = chunkNumber * rowsPerChunk;
                int jStop = jStart + rowsPerChunk;
                if (jStop > outHeight)
                {
                    jStop = outHeight;
                }

                for (int j = jStart; j < jStop; ++j)
                {
                    float jLocationFraction = j / (float)outHeight;
                    var jFloatPosition = height * jLocationFraction;
                    var j2 = (int)jFloatPosition;
                    var jFraction = jFloatPosition - j2;
                    var j1 = j2 > 0 ? j2 - 1 : j2;
                    var j3 = j2 < height - 1 ? j2 + 1 : j2;
                    var j4 = j3 < height - 1 ? j3 + 1 : j3;
                    for (int i = 0; i < outWidth; ++i)
                    {
                        float iLocationFraction = i / (float)outWidth;
                        var iFloatPosition = width * iLocationFraction;
                        var i2 = (int)iFloatPosition;
                        var iFraction = iFloatPosition - i2;
                        var i1 = i2 > 0 ? i2 - 1 : i2;
                        var i3 = i2 < width - 1 ? i2 + 1 : i2;
                        var i4 = i3 < width - 1 ? i3 + 1 : i3;
                        float jValue1 = InterpolateCubic(
                            data[j1, i1], data[j1, i2], data[j1, i3], data[j1, i4], iFraction);
                        float jValue2 = InterpolateCubic(
                            data[j2, i1], data[j2, i2], data[j2, i3], data[j2, i4], iFraction);
                        float jValue3 = InterpolateCubic(
                            data[j3, i1], data[j3, i2], data[j3, i3], data[j3, i4], iFraction);
                        float jValue4 = InterpolateCubic(
                            data[j4, i1], data[j4, i2], data[j4, i3], data[j4, i4], iFraction);
                        ret[j, i] = InterpolateCubic(
                            jValue1, jValue2, jValue3, jValue4, jFraction);
                    }
                }
            });

            return ret;
        }
    }
}