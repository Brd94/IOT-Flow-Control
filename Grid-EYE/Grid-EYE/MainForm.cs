using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grid_EYE
{
    public delegate void OnMatrixProcessComplete(float[,] matrix);

    public partial class MainForm : Form
    {

        private SerialPort serialPort;
        private Stopwatch stopWatch;

        private string TermString = "";
        private string MatrixString = "";

        private CultureInfo culture;

        public event OnMatrixProcessComplete OnProcessComplete;
        List<Thread> ui_threads = new List<Thread>();


        public MainForm()
        {
            InitializeComponent();

            stopWatch = Stopwatch.StartNew();

            string[] ports = SerialPort.GetPortNames();


            for (int i = 0; i < ports.Length; i++)
                listBox2.Items.Add(ports[i]);

            culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.NumberFormat.NumberDecimalSeparator = ".";

            OnProcessComplete += matrix =>
            {
                InvokeOnMainThread(() =>
                    {
                        string x = "";

                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                                x += matrix[i, j].ToString("N1") + " | ";

                            x += Environment.NewLine;
                        }

                        textBox3.Text = x;

                    });
            };

            OnProcessComplete += OnMatrixProcessComplete;
            OnProcessComplete += CalculateDeltas;

            listBox1.DisplayMember = "MatrixName";

            
        }

        int n_sopra = 0;
        int n_sotto = 0;

        private void CalculateDeltas(float[,] matrix)
        {

            if (matrix_with_cluster_center == null)
                return;

            int a_sopra = 0;
            int a_sotto = 0;

            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (matrix_with_cluster_center[i, j] != 0)
                    {
                        if (i < 16)
                        {
                            ++a_sopra;

                        }
                        else
                        {
                            ++a_sotto;

                        }
                    }
                }
            }

            if (a_sopra != n_sopra && a_sotto != n_sotto)
                OnError(msg: "Delta : " + (a_sopra-a_sotto));


            n_sopra = a_sopra;
            n_sotto = a_sotto;

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort.DataReceived += OnDataReceive;

        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                ThreadStart start = () => {

                    var matrix_form = new MatrixForm();
                    ObservableMatrixArray<float> selected = null;
                    InvokeOnMainThread(() => selected = (ObservableMatrixArray<float>)listBox1.SelectedItem);
                    matrix_form.SetMatrix(selected);
                    matrix_form.ShowDialog();


                    OnError(msg: "Thread matrice " + selected?.MatrixName + " terminato.");
                };
                

                Thread t = new Thread(start);
               
                start += () => { ui_threads.Remove(t); };

                ui_threads.Add(t);
               
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
                return;

            if (serialPort != null)
                serialPort.Dispose();

            serialPort = new SerialPort();
            serialPort.BaudRate = 38400;
            serialPort.PortName = listBox2.SelectedItem.ToString();

            serialPort.Open();

        }

        private void OnDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            string read = serialPort.ReadLine();

            ReadMatrixFromFlow(read);
            ReadTermFromFlow(read);
        }

        private void ReadTermFromFlow(string read)
        {
            if (read.IndexOf("BeginTerm") != -1)
            {
                TermString = "";
                return;
            }

            if (read.IndexOf("EndTerm") != -1)
            {
                float.TryParse(TermString.Replace('.', ','), out float Term);
                InvokeOnMainThread(() => textBox1.Text = Term + " °");
                return;
            }

            TermString += read + Environment.NewLine;
        }

        private void InvokeOnMainThread(Action act)
        {
            if (InvokeRequired)
                Invoke(act);
            else act();
        }

        private void ReadMatrixFromFlow(string read)
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


                    var matrix = ProcessMatrix(MatrixString);

                    OnProcessComplete?.Invoke(matrix);

                    InvokeOnMainThread(() => textBox2.Text = stopWatch.ElapsedMilliseconds + " ms");



                    stopWatch.Restart();

                    return;
                }

                MatrixString += read + Environment.NewLine;

            }
            catch (Exception e) { OnError(e); }
        }

        ObservableMatrixArray<float> base_matrix = new ObservableMatrixArray<float>(50);
        ObservableMatrixArray<float> dam_matrix = new ObservableMatrixArray<float>(50);

        private float[,] matrix_with_cluster_center;
        private ObservableMatrixArray<float> ip = new ObservableMatrixArray<float>(1);
        private ObservableMatrixArray<float> dam_mwmb01 = new ObservableMatrixArray<float>(1);

        private void OnError(Exception e = null, string msg = "")
        {
            InvokeOnMainThread(() => textBox4.Text += Environment.NewLine + DateTime.Now + " │ " + msg + " : " + e?.ToString() ?? "");
        }

        private void OnMatrixProcessComplete(float[,] matrix)
        {
            InvokeOnMainThread(() =>
            {
                listBox1.Items.Clear();

                var interpolated = MatrixOperation.BicubicInterpolation(matrix, 32, 32);

                ip.AddMatrix(interpolated);
                ip.MatrixName = "32x32 Interpolata";
                listBox1.Items.Add(ip);

                if (base_matrix.CurrentIndex < 50)
                    base_matrix.AddMatrix(interpolated);


                if (dam_matrix.CurrentIndex < 1)
                    dam_matrix.AddMatrix(interpolated);



                base_matrix.MatrixName = "Matrice base " + base_matrix.CurrentIndex;
                listBox1.Items.Add(base_matrix);



                var matrix_med_base = MatrixOperation.ApplyOperation(x =>
                {
                    return System.Linq.Enumerable.Average(x);
                }, base_matrix.getLastMatrixes(), 32, 32);

                var mmd = new ObservableMatrixArray<float>(matrix_med_base);
                mmd.MatrixName = "Media Base";
                listBox1.Items.Add(mmd);

                var matrix_max_base = MatrixOperation.ApplyOperation(x =>
                {
                    return System.Linq.Enumerable.Max(x);
                }, base_matrix.getLastMatrixes(), 32, 32);

                var mud = new ObservableMatrixArray<float>(matrix_max_base);
                mud.MatrixName = "Max Base";
                listBox1.Items.Add(mud);


                var matrix_std_base = MatrixOperation.ApplyOperation((x, i, j) =>
                {

                    double sigma = 0;

                    for (int k = 0; k < x.Length; k++)
                    {
                        sigma += Math.Pow(x[k] - matrix_med_base[i, j], 2);
                    }

                    return (float)Math.Sqrt(sigma / x.Length);

                }, base_matrix.getLastMatrixes(), 32, 32);


                var msb = new ObservableMatrixArray<float>(matrix_std_base);
                msb.MatrixName = "STD Base";
                listBox1.Items.Add(msb);



                var matrix_without_background = MatrixOperation.ApplyOperation<float, float>(x =>
                 {
                     return x[2] > x[0] + (8 * x[1]) ? 1 : 0;
                 }, new[] { matrix_med_base, matrix_std_base, interpolated }, 32, 32);

                var mwb = new ObservableMatrixArray<float>(matrix_without_background);
                mwb.MatrixName = "Background Eliminato";
                listBox1.Items.Add(mwb);

                var matrix_without_max_background = MatrixOperation.ApplyOperation<float, float>(x =>
                {
                    return x[1] - x[0];
                }, new[] { matrix_max_base, interpolated }, 32, 32);



                var mwmb = new ObservableMatrixArray<float>(matrix_without_max_background);
                mwmb.MatrixName = "Background Max Eliminato";
                listBox1.Items.Add(mwmb);

                float media = 0;

                MatrixOperation.ApplyOperation<float, float>(x =>
                {
                    media += x[0];
                    return 0;
                }, new[] { matrix_without_max_background }, 32, 32);

                media /= 1024;

                var matrix_without_max_background_mean = MatrixOperation.ApplyOperation<float, float>(x =>
                {
                    return x[0] > media ? x[0] : 0;
                }, new[] { matrix_without_max_background }, 32, 32);

                var mwmb01 = new ObservableMatrixArray<float>(matrix_without_max_background_mean);
                mwmb01.MatrixName = "Background Max Eliminato sopra media";
                listBox1.Items.Add(mwmb01);



                var centroid = CalculateCentroid(matrix_without_max_background_mean);


                matrix_with_cluster_center = MatrixOperation.ApplyOperation<float, float>((x, i, j) =>
                {
                    return i == centroid.x && j == centroid.y ? 1 : 0;
                }, new[] { matrix_max_base, interpolated }, 32, 32);



                var mwcc = new ObservableMatrixArray<float>(matrix_with_cluster_center);
                mwcc.MatrixName = "Clusters center";
                listBox1.Items.Add(mwcc);


                //TEST X DAMIANO

                float dam_media = 0;
                float[] dam_array = new float[1024];

                MatrixOperation.ApplyOperation<float, float>((x, i, j) =>
                 {
                     dam_array[i + j] = x[0];
                     return 0;
                 }, new[] { dam_matrix.getLastInsertedMatrix() });


                dam_media = dam_array.Skip(1).Take(5).Average() + 8;

                label3.Text = "DAM MEDIA : " + (dam_media );

                var dam_matrix_without_max_background = MatrixOperation.ApplyOperation<float, float>((x,i,j) =>
                {
                    return x[0] > dam_media ? 0 : 1;
                }, new[] { interpolated }, 32, 32);

                dam_mwmb01.AddMatrix(dam_matrix_without_max_background);
                dam_mwmb01.MatrixName = "DAM Background Max Eliminato sopra media";
                listBox1.Items.Add(dam_mwmb01);

      

            });
        }

        private (int x, int y) CalculateCentroid(float[,] matrix)
        {
            float[] sum_x = new float[32];
            float[] sum_y = new float[32];

            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    sum_x[i] += matrix[i, j];
                    sum_y[i] += matrix[j, i];

                }
            }

            float mp_x = 0;
            float mp_y = 0;

            for (int i = 0; i < 32; i++)
            {
                mp_x += (i * sum_x[i]);
                mp_y += (i * sum_y[i]);
            }

            mp_x /= Enumerable.Sum(sum_x);
            mp_y /= Enumerable.Sum(sum_y);

            return ((int)mp_x, (int)mp_y);
        }



        private float[,] ProcessMatrix(string Matrix)
        {
            float[,] matrix = new float[8, 8];

            try
            {


                var rows = Matrix.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < 8; i++)
                {
                    var values = rows[i].Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < 8; j++)
                    {

                        matrix[i, j] = float.Parse(values[j], culture);
                    }
                }

            }
            catch { }

            return matrix;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialPort.DataReceived -= OnDataReceive;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            base_matrix = new ObservableMatrixArray<float>(50);
            dam_matrix = new ObservableMatrixArray<float>(1);
        }
    }
}
