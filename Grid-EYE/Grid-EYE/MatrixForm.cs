using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grid_EYE
{

    public partial class MatrixForm : Form
    {

        [DllImport("gdi32.dll")]
        static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

        private float[,] showed_matrix;

        public ObservableMatrixArray<float> matrix { get; private set; }

        Graphics form_graphics;

        private IntPtr hdc;

        public SemaphoreSlim mre;

        private float? max_value;
        private float? min_value;

        public MatrixForm()
        {
            InitializeComponent();

            this.form_graphics = this.CreateGraphics();

            hdc = form_graphics.GetHdc();


            mre = new SemaphoreSlim(1);

            this.Paint += MatrixForm_Paint;
        }

        private void MatrixForm_Paint(object sender, PaintEventArgs e)
        {

            PrintMatrix(e.Graphics);
        }

        private void PrintMatrix(Graphics e)
        {
            //e.Clear(this.BackColor);

            showed_matrix = matrix?.getLastInsertedMatrix();

            if (showed_matrix == null)
                return;

            int matrix_size = (int)Math.Sqrt(showed_matrix.Length);

            int PIXEL_SIZE = Math.Max(Width / matrix_size, 1);

            int pixel_h = 0;
            int pixel_v = 0;

            float max = int.MinValue;
            float min = int.MaxValue;



            for (int i = 0; i < matrix_size; i++)
            {
                for (int j = 0; j < matrix_size; j++)
                {
                    if (showed_matrix[i, j] > max)
                        max = showed_matrix[i, j];
                    if (showed_matrix[i, j] < min)
                        min = showed_matrix[i, j];
                }

            }


            if (max_value.HasValue)
                max = max_value.Value;

            if (min_value.HasValue)
                min = min_value.Value;


            for (int i = 0; i < matrix_size; i++)
            {
                for (int j = 0; j < matrix_size; j++)
                {
                    int r = 255;
                    int g = 0;
                    int b = 0;

                    float value = max - showed_matrix[i, j];

                    float x = (value * 510) / (max - min);

                    Color toBlend = GetBlendedColor((value * 100) / (max - min));

                    if (!float.IsNaN(x) && x >= 0 && x <= 510)
                    {
                        g = x >= 255 ? 0 : 255 - (int)x;
                        b = x > 255 ? (int)x - 255 : 0;
                    }

                    Brush brush = new SolidBrush(Color.FromArgb(r, g, b));
                    Brush brush_2 = new SolidBrush(toBlend);
                    //InvokeOnMainThread(() =>
                    //{
                    //    e.FillRectangle(brush, pixel_h, pixel_v, PIXEL_SIZE, PIXEL_SIZE);
                    //});



                    e.FillRectangle(brush_2, pixel_h, pixel_v, PIXEL_SIZE, PIXEL_SIZE);


                    string s = ((int)showed_matrix[i, j]).ToString();
                    TextOut(hdc, pixel_h, pixel_v, s, s.Length);


                    pixel_h += PIXEL_SIZE;
                }

                pixel_v += PIXEL_SIZE;
                pixel_h = 0;
            }


        }

        public Color GetBlendedColor(float percentage)
        {
            if (percentage < 50)
                return Interpolate(Color.Red, Color.Yellow, percentage / 50.0);
            return Interpolate(Color.Yellow, Color.Lime, (percentage - 50) / 50.0);
        }

        private Color Interpolate(Color color1, Color color2, double fraction)
        {
            double r = Interpolate(color1.R, color2.R, fraction);
            double g = Interpolate(color1.G, color2.G, fraction);
            double b = Interpolate(color1.B, color2.B, fraction);
            return Color.FromArgb((int)Math.Round(r), (int)Math.Round(g), (int)Math.Round(b));
        }

        private double Interpolate(double d1, double d2, double fraction)
        {
            return d1 + (d2 - d1) * fraction;
        }

        private void InvokeOnMainThread(Action act)
        {
            if (InvokeRequired)
                Invoke(act);
            else
                act();
        }


        public void SetMatrix(ObservableMatrixArray<float> matrix)
        {
            this.matrix = matrix;



            if (matrix != null)
            {
                int matrix_size = (int)Math.Sqrt(matrix.getLastInsertedMatrix().Length);

                InvokeOnMainThread(() =>
                {

                    this.Text = matrix.MatrixName;
                });

                matrix.OnMatrixAdd += () => mre.Release();
            }

        }

        private async void MatrixForm_Shown(object sender, EventArgs e)
        {
            while (true)
            {
                await mre.WaitAsync();
                PrintMatrix(this.CreateGraphics());
                Application.DoEvents();
            }
        }

        private void MatrixForm_ResizeEnd(object sender, EventArgs e)
        {
            Height = Width;
            PrintMatrix(this.CreateGraphics());
        }

        private void MatrixForm_ResizeBegin(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = !checkBox1.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textBox1.Text))
                    max_value = float.Parse(textBox1.Text);
                else
                    max_value = null;
            }
            catch { }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(textBox2.Text))
                    min_value = float.Parse(textBox2.Text);
                else
                    min_value = null;
            }
            catch { }
        }
    }
}
