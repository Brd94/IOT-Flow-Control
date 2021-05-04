using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grid_EYE
{
    public class MatrixOperation
    {
        public static TResult[,] ApplyOperation<T,TResult>(Func<T[], TResult> op, IEnumerable<T[,]> input, int size_x = 8, int size_y = 8)
        {
            int inputLenght = System.Linq.Enumerable.Count(input);

            TResult[,] result = new TResult[size_x, size_y];


            for (int i = 0; i < size_x; i++)
            {
                for (int j = 0; j < size_y; j++)
                {
                    int iCount = 0;
                    T[] operations = new T[inputLenght];

                    foreach (var matrix in input)
                        operations[iCount++] = matrix[i, j];

                    result[i, j] = op.Invoke(operations);

                }
            }

            return result;
        }

        public static TResult[,] ApplyOperation<T,TResult>(Func<T[], int, int, TResult> op, IEnumerable<T[,]> input, int size_x = 8, int size_y = 8)
        {
            int inputLenght = System.Linq.Enumerable.Count(input);

            TResult[,] result = new TResult[size_x, size_y];


            for (int i = 0; i < size_x; i++)
            {
                for (int j = 0; j < size_y; j++)
                {
                    int iCount = 0;
                    T[] operations = new T[inputLenght];

                    foreach (var matrix in input)
                        operations[iCount++] = matrix[i, j];

                    result[i, j] = op.Invoke(operations, i, j);

                }
            }

            return result;
        }

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

        public static (int x, int y) CalculateCentroid(float[,] matrix)
        {
      
            float sum_x;
            float sum_y;

            float mp_x = 0;
            float mp_y = 0;

            float sum = 0;

            for (int i = 0; i < 32; i++)
            {
                sum_x = 0;
                sum_y = 0;

                for (int j = 0; j < 32; j++)
                {
                   
                    sum_x += matrix[i, j];
                    sum_y += matrix[j, i];
                    sum += matrix[i, j];
                }

                mp_x += (i * sum_x);
                mp_y += (i * sum_y);

            }

           

            mp_x /= sum;
            mp_y /= sum;

            return ((int)mp_x, (int)mp_y);
        }

        public static (int[] x,int[] y) CalculateCentroids(float[,] matrix)
        {
            int lenght = matrix.GetLength(0);

            List<float[,]> matrixes = new List<float[,]>();

            for (int i = 0; i < lenght; i++)
            {
                for(int j = 0; j < lenght; j++)
                {
                    if(matrix[i,j] != 0)
                    {
                        //Program.PrintMatrix(matrix);
                        var matrix_out = new float[lenght, lenght];
                        ApplyFloodFill_Iterative((short)i, (short)j, 0,ref matrix,ref matrix_out);
                        matrixes.Add(matrix_out);
                    }
                }
            }

            int[] x = new int[matrixes.Count];
            int[] y = new int[matrixes.Count];

            for (int i = 0; i < matrixes.Count; i++)
            {
                var centroid = CalculateCentroid(matrixes[i]);
                x[i] = centroid.x;
                y[i] = centroid.y;
            }

            return (x,y);

        }

        public static void ApplyFloodFill(int x_node,int y_node,float limit_val,ref float[,] matrix_in,ref float[,] matrix_out)
        {
            if (matrix_in[x_node, y_node] == limit_val)
                return;

            matrix_out[x_node, y_node] = matrix_in[x_node, y_node];
            matrix_in[x_node, y_node] = limit_val;

            ApplyFloodFill(Math.Max(x_node - 1, 0), y_node, limit_val,ref matrix_in,ref matrix_out);
            ApplyFloodFill(Math.Min(x_node + 1, matrix_in.GetLength(0) -1), y_node, limit_val,ref matrix_in, ref matrix_out);
            ApplyFloodFill(x_node, Math.Max(y_node - 1, 0), limit_val,ref matrix_in, ref matrix_out);
            ApplyFloodFill(x_node, Math.Min(y_node + 1, matrix_in.GetLength(1) -1), limit_val,ref matrix_in, ref matrix_out);

        }

        public static void ApplyFloodFill_Iterative(short x_node, short y_node, short limit_val,ref float[,] matrix_in,ref float[,] matrix_out)
        {
            short MATRIX = 32;

            int coda = 0;
            int testa = 1;

            short[] matrix_queue = new short[1024];
            bool[,] matrix_visited = new bool[32,32];

            matrix_queue[0] = (short)((x_node * MATRIX) + y_node);

            while (coda < testa)
            {
                short x = (short)(matrix_queue[coda] / MATRIX);
                short y = (short)(matrix_queue[coda] % MATRIX);

               

                if (matrix_in[x,y] != limit_val)
                {
                    matrix_out[x,y] = matrix_in[x,y];
                    matrix_in[x,y] = limit_val;
                }

                short x1 = (short)Math.Max(x - 1, 0);
                short y1 = y;

                if (matrix_in[x1,y1] != limit_val && !matrix_visited[x1,y1])
                {
                    matrix_visited[x1,y1] = true;
                    matrix_queue[testa] = (short)((x1 * MATRIX) + y1);
                    testa++;
                }

                short x2 = (short)(Math.Min(x + 1, MATRIX - 1));
                short y2 = y;

                if (matrix_in[x2,y2] != limit_val && !matrix_visited[x2,y2])
                {
                    matrix_visited[x2,y2] = true;
                    matrix_queue[testa] = (short)((x2 * MATRIX) + y2);
                    testa++;
                }

                short x3 = x;
                short y3 = (short)Math.Max(y - 1, 0);

                if (matrix_in[x3,y3] != limit_val && !matrix_visited[x3,y3])
                {
                    matrix_visited[x3,y3] = true;
                    matrix_queue[testa] = (short)((x3 * MATRIX) + y3);
                    testa++;
                }

                short x4 = x;
                short y4 = (short)Math.Min(y + 1, MATRIX - 1);

                if (matrix_in[x4,y4] != limit_val && !matrix_visited[x4,y4])
                {
                    matrix_visited[x4,y4] = true;
                    matrix_queue[testa] = (short)((x4 * MATRIX) + y4);
                    testa++;
                }

                coda++;
            }
        }
    }
}
