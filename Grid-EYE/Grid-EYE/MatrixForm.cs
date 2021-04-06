using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Grid_EYE
{
    public partial class MatrixForm : Form
    {
        public const int PIXEL_SIZE = 20;

        private float[,] showed_matrix;

        public MatrixForm()
        {
            InitializeComponent();

            this.Paint += MatrixForm_Paint;
        }

        private void MatrixForm_Paint(object sender, PaintEventArgs e)
        {
            if (showed_matrix == null)
                return;

            int matrix_size = (int)Math.Sqrt(showed_matrix.Length);

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



            for (int i = 0; i < matrix_size; i++)
            {
                for (int j = 0; j < matrix_size; j++)
                {
                    int r = 255;
                    int g = 0;
                    int b = 0;

                    float value = max - showed_matrix[i, j];

                    float x = (value * 510) / (max - min);

                    if (!float.IsNaN(x))
                    {
                        g = x >= 255 ? 255 : (int)x;
                        b = x > 255 ? (int)x - 255 : 0;
                    }

                    Brush brush = new SolidBrush(Color.FromArgb(r, g, b));

                    e.Graphics.FillRectangle(brush, pixel_h, pixel_v, PIXEL_SIZE, PIXEL_SIZE);

                    pixel_h += PIXEL_SIZE;
                }

                pixel_v += PIXEL_SIZE;
                pixel_h = 0;
            }


        }

        private void InvokeOnMainThread(Action act)
        {
            if (InvokeRequired)
                Invoke(act);
        }


        public void DrawMatrix(float[,] matrix)
        {
            showed_matrix = matrix;
            Refresh();
        }
    }
}
