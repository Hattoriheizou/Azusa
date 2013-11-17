using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUTAN_proto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void parseAsExpressionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result;
            if (ExprParser.TryParse(textBox1.Text, out result))
            {
                MessageBox.Show("The expression entered evaluated to: " + result);
            }
            else
            {
                MessageBox.Show("An error occured: " + result);
            }

        }

        
    }
}
