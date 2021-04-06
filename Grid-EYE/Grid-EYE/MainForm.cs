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

        private MatrixForm matrix_form;

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

            listBox1.DisplayMember = "MatrixName";

            matrix_form = new MatrixForm();
            matrix_form.Show();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            serialPort.DataReceived += OnDataReceive;

        }

        private int lb1_index = -1;

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                lb1_index = listBox1.SelectedIndex;
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

        MatrixArray<float> base_matrix = new MatrixArray<float>(50);


        private void OnError(Exception e, string msg = "")
        {
            InvokeOnMainThread(() => textBox4.Text = DateTime.Now + " │ " + msg + " : " + e.ToString());
        }

        private void OnMatrixProcessComplete(float[,] matrix)
        {
            InvokeOnMainThread(() =>
            {
                listBox1.Items.Clear();

                var interpolated = MatrixOperation.BicubicInterpolation(matrix, 32, 32);

                var ip = new MatrixArray<float>(interpolated);
                ip.MatrixName = "32x32 Interpolata";
                listBox1.Items.Add(ip);

                if (base_matrix.CurrentIndex < 50)
                    base_matrix.AddMatrix(interpolated);

                base_matrix.MatrixName = "Matrice base " + base_matrix.CurrentIndex;
                listBox1.Items.Add(base_matrix);


                var matrix_med_base = MatrixOperation.ApplyOperation(x =>
                {
                    return System.Linq.Enumerable.Average(x);
                }, base_matrix.getLastMatrixes(), 32, 32);

                var mmd = new MatrixArray<float>(matrix_med_base);
                mmd.MatrixName = "Media Base";
                listBox1.Items.Add(mmd);


                var matrix_std_base = MatrixOperation.ApplyOperation((x, i, j) =>
                {
                   
                    double sigma = 0;

                    for (int k = 0; k < x.Length; k++)
                    {
                        sigma += Math.Pow(x[k] - matrix_med_base[i, j], 2);
                    }

                    return (float)Math.Sqrt(sigma / x.Length);

                }, base_matrix.getLastMatrixes(), 32, 32);


                var msb = new MatrixArray<float>(matrix_std_base);
                msb.MatrixName = "STD Base";
                listBox1.Items.Add(msb);



                var matrix_without_background = MatrixOperation.ApplyOperation<float,float>(x =>
                {
                    return x[2] > x[0] + (8 * x[1]) ? 1 : 0;
                }, new[] { matrix_med_base, matrix_std_base, interpolated }, 32, 32);

                var mwb = new MatrixArray<float>(matrix_without_background);
                mwb.MatrixName = "Background Eliminato";
                listBox1.Items.Add(mwb);

                if (lb1_index != -1)
                {
                    var selected = (MatrixArray<float>)listBox1.Items[lb1_index];
                    matrix_form.DrawMatrix(selected.getLastInsertedMatrix());
                }

            });
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
            base_matrix = new MatrixArray<float>(50);
        }
    }
}
