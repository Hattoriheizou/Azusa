using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Azusa
{
    public partial class DBG_OUTPUT : Form
    {
        public DBG_OUTPUT()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
                richTextBox1.Text=DbgOutput.Read();
            
        }
    }
}
