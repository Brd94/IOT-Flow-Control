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
    public partial class DeltaForm : Form
    {
        private int delta;

        public DeltaForm()
        {
            InitializeComponent();
        }

        public void PushDelta(int i)
        {
            delta += i;
            InvokeOnMainThread(() => label1.Text = delta + "");
        }

        public void PushSopra(int i)
        {
            InvokeOnMainThread(() => label2.Text = i + "");
        }

        public void PushSotto(int i)
        {
            InvokeOnMainThread(() => label3.Text = i + "");
        }


        private void InvokeOnMainThread(Action act)
        {
            if (InvokeRequired)
                Invoke(act);
            else
                act();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            delta = 0;
            PushDelta(0);
        }
    }
}
