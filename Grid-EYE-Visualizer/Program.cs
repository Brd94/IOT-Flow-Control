using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace Grid_EYE_Visualizer
{
    class Program
    {

        public static SerialPort serialPort;
        public static string SerialPortAddr { get; set; }

        static Stopwatch stopWatch;
        public static string MatrixString { get; set; }
        public static string TermString { get; set; }
        public static float Term;

        public static MatrixArray<float> matrixHistory;
        public static MatrixArray<float> matrixBase;

        static void Main(string[] args)
        {
            matrixHistory = new MatrixArray<float>(200);
            matrixBase = new MatrixArray<float>(200);

            stopWatch = Stopwatch.StartNew();

            string[] ports = SerialPort.GetPortNames();

            Console.WriteLine("Seleziona porta");

            for (int i = 0; i < ports.Length; i++)
                Console.WriteLine(i + " - " + ports[i]);

            //SerialPortAddr = ports[int.Parse(Console.ReadLine())];
            SerialPortAddr = ports[1];

            serialPort = new SerialPort();
            serialPort.BaudRate = 38400;
            serialPort.PortName = SerialPortAddr;

            serialPort.DataReceived += OnDataReceive;
            serialPort.Open();

            Console.ReadLine();

        }


        private static void OnDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            string read = serialPort.ReadLine();

            ReadMatrixFromFlow(read);
            ReadTermFromFlow(read);

        }

        private static void ReadTermFromFlow(string read)
        {
            if (read.IndexOf("BeginTerm") != -1)
            {
                TermString = "";
                return;
            }

            if (read.IndexOf("EndTerm") != -1)
            {
                float.TryParse(TermString.Replace('.', ','), out Term);
                Console.Write("Term tem : " + Term);
                return;
            }

            TermString += read + Environment.NewLine;
        }

        static int baseCount = 0;
        private static int count = 0;

        private static void ReadMatrixFromFlow(string read)
        {
            try
            {
                if (read.IndexOf("BeginMatrix") != -1)
                {
                    MatrixString = "";
                    return;
                }

                if (read.IndexOf("EndMatrix") != -1)
                {
                    Console.Clear();
                    //Console.Write( MatrixString);

                    var matrix = ProcessMatrix(MatrixString);
                    matrixHistory.AddMatrix(matrix);
                    float[,] interpolated = Interpolation.BicubicInterpolation(matrix, 32, 32);
                    //PrintMatrix(interpolated, 32, 32);
                    if (baseCount < 50)
                    {
                        matrixBase.AddMatrix(interpolated);
                        Console.WriteLine("Calcolando Base... ({0})", baseCount);
                        baseCount++;
                        //return;
                    }
                    else


                        Console.WriteLine("Base piena");

                    // var comparison = CompareMatrixes(matrix,matrix_history);

                    var matrix_med_base = MatrixArray<float>.ApplyOperation<float>(x =>
                    {
                        return System.Linq.Enumerable.Average(x);
                    }, matrixBase.getLastMatrixes(), 32, 32);

                    var matrix_std_base = MatrixArray<float>.ApplyOperation<float>((x, i, j) =>
                    {
                        // var std_elements = new float[x.Length,1];

                        // for(int i=0;i<x.Length;i++){
                        //     std_elements[i,0] = x[i];
                        // }

                        // return (float)getMatrix_VAR(std_elements,x.Length,1);
                        double sigma = 0;

                        for (int k = 0; k < x.Length; k++)
                        {
                            sigma += Math.Pow(x[k] - matrix_med_base[i, j], 2);
                        }

                        return (float)Math.Sqrt(sigma / x.Length);

                    }, matrixBase.getLastMatrixes(), 32, 32);

                    var matrix_without_background = MatrixArray<float>.ApplyOperation<float>(x =>
                    {
                        return x[2] > x[0] + (4 * x[1]) ? 1 : 0;
                    }, new[] { matrix_med_base, matrix_std_base, interpolated }, 32, 32);

                    PrintMatrix(matrix_without_background, 32, 32);

                    //PrintMatrix(matrix_std_base,32,32);
                    // var matrix_media_base = MatrixArray<float>.ApplyOperation<float>(x =>
                    //     {
                    //         return System.Linq.Enumerable.Average(x);
                    //     }, matrixBase.getLastMatrixes());


                    // var matrix_std_base = MatrixArray<float>.ApplyOperation<float>(x =>
                    //     {

                    //         float[,] matrix = new float[x.Length, 1];

                    //         for (int i = 0; i < x.Length; i++)
                    //             matrix[i, 0] = x[i];

                    //         return (float)getMatrix_VAR(matrix, x.Length, 1);

                    //     }, matrixBase.getLastMatrixes());

                    // //PrintMatrix(matrix_media_base);

                    // var MatrixCmp = MatrixArray<float>.ApplyOperation<float>(x =>
                    // {

                    //     float[,] matrix = new float[x.Length, 1];

                    //     for (int i = 0; i < x.Length; i++)
                    //         matrix[i, 0] = x[i];

                    //     return (float)getMatrix_VAR(matrix, x.Length, 1);

                    // }, new[] { matrix_max_base, matrixHistory.getLastInsertedMatrix() });

                    // PROVE IERI
                    // if (matrixHistory.CurrentIndex > 2)
                    // {

                    //     var prova = MatrixArray<float>.ApplyOperation<float>(x =>
                    //     {
                    //         return Math.Abs(x[1] - x[0]) < 8 ? 0 : 1;
                    //     }, matrixHistory.getLastMatrixes(2));

                    //     var prova2 = MatrixArray<float>.ApplyOperation<float>(x =>
                    //     {
                    //         return Math.Abs(x[1] - x[0]) < 5 ? 0 : 1;
                    //     }, matrixHistory.getLastMatrixes(2));

                    //     PrintMatrix(prova);

                    // }

                    var MatrixCmpThreshold = MatrixArray<float>.ApplyOperation<float>(x =>
                    {
                        var diff = x[1] - x[0];
                        return Math.Abs(diff) > 2 ? 1 : 0;

                    }, new[] { matrix_med_base, matrixHistory.getLastInsertedMatrix() });

                    //var MatrixCmpPoints = GetMatrix_Points(MatrixCmpThreshold, 8, 8);
                    // var MatrixCmp = MatrixArray<float>.ApplyOperation<float>(x =>
                    // {

                    //     float[,] matrix = new float[x.Length, 1];

                    //     for (int i = 0; i < x.Length; i++)
                    //         matrix[i, 0] = x[i];

                    //     return (float)getMatrix_VAR(matrix, x.Length, 1);

                    // }, new[] { matrix_media_base, matrixHistory.getLastInsertedMatrix() });


                    // var MatrixCmpStd = MatrixArray<float>.ApplyOperation<float>(x =>
                    // {

                    //    return x[0] >= x[1] ? 0 : 1;

                    // }, new[] { matrix_std_base, MatrixCmp });
                    //PrintMatrix(MatrixCmpPoints);

                    //PrintMatrix_C(matrixHistory.getLastInsertedMatrix());

                    Console.WriteLine("\n\nMeasureTime = " + stopWatch.ElapsedMilliseconds);



                    stopWatch.Restart();

                    return;
                }

                MatrixString += read + Environment.NewLine;

            }
            catch (Exception e) { Console.WriteLine("Errore : " + e.ToString()); }
        }

        /*
        Fase iniziale
        Prendo i primi x secondi. Durante questo periodo controllo che non ci siano spostamenti con la funzione spostamento
        Se c'è spostamento -> Riavvio la fase iniziale
        Se non c'è spostamento -> Dalla storia creo una media
        */

        private static float[,] CompareMatrixes(float[,] matrix1, float[,] matrix2)
        {
            float[,] matrixRes = new float[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (matrix1[i, j] == matrix2[i, j])
                        matrixRes[i, j] = 1;
                }
            }

            return matrixRes;
        }

        private static double getMatrix_VAR(float[,] matrix, int x, int y)
        {
            int iCountDiff = 0;
            int iCountStd = 0;


            float sum = 0;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    sum += matrix[i, j];

            var mean = sum / (x * y);

            float sum_sqrt = 0;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {

                    matrix[i, j] -= mean;
                    matrix[i, j] *= matrix[i, j];

                }
            }

            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    sum_sqrt += matrix[i, j];

            double std = (sum_sqrt / (x * y));

            return std;

            // Console.WriteLine("STD DEV : {0}", std);
            // Console.WriteLine("Matrix STD : {0} - DIFF : {1}", iCountStd, iCountDiff);
        }

        private static float[,] GetMatrix_Points(float[,] matrix, int x, int y)
        {
            float[,] res = new float[x, y];



            float maxval = 0;

            int max_i = -1;
            int max_j = -1;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    if (matrix[i, j] > maxval)
                    {
                        maxval = matrix[i, j];
                        max_i = i;
                        max_j = j;
                    }
                }
            }

            if (max_i >= 0 && max_j >= 0)
            {
                SetMatrix_Points_Intorno(matrix, x, y, max_i, max_j, 0);
                SetMatrix_Points_Intorno(res, x, y, max_i, max_j, 1);
            }

            return res;
        }

        private static bool GetMatrix_Points_Int(float[,] matrix, int x, int y, int px, int py)
        {
            int iCountMagg = 0;

            for (int i = Math.Max(0, px - 1); i < Math.Min(px + 2, x); i++)
            {
                for (int j = Math.Max(0, py - 1); j < Math.Min(py + 2, y); j++)
                {
                    if (i != px && j != py && matrix[i, j] >= matrix[px, py])
                        ++iCountMagg;
                }
            }



            return iCountMagg == 0;
        }

        private static void SetMatrix_Points_Intorno(float[,] matrix, int x, int y, int px, int py, int val)
        {
            int iCountMagg = 0;

            for (int i = Math.Max(0, px - 1); i < Math.Min(px + 2, x); i++)
            {
                for (int j = Math.Max(0, py - 1); j < Math.Min(py + 2, y); j++)
                {
                    matrix[i, j] = val;
                }
            }

        }



        private static void PrintMatrix(float[,] matrix, int x = 8, int y = 8)
        {
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    //Console.Write(matrix[i, j].ToString("N2"));
                    if (matrix[i, j] > 0)
                        Console.Write("█ ");
                    else if (matrix[i, j] > 25)
                        Console.Write("▓ ");
                    else if (matrix[i, j] > 20)
                        Console.Write("▒ ");
                    else
                        Console.Write("░ ");

                }
                Console.WriteLine();
            }
        }

        private static void PrintMatrix_P(float[,] matrix)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //  Console.Write(matrix[i, j].ToString("N2"));
                    if (matrix[i, j] > Term)
                        Console.Write("P ");
                    else
                        Console.Write("░ ");

                }
                Console.WriteLine();
            }
        }

        private static float[,] ProcessMatrix(string Matrix)
        {
            float[,] matrix = new float[8, 8];

            try
            {


                var rows = Matrix.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < 8; i++)
                {
                    var values = rows[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < 8; j++)
                    {

                        matrix[i, j] = float.Parse(values[j].Replace('.', ','));
                    }
                }

            }
            catch { }

            return matrix;
        }

    }
}
