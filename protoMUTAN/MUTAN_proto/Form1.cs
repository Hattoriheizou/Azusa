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
            MUTAN.LineParser parser = new MUTAN.LineParser();
            MUTAN.IRunnable obj;
            
                foreach (string line in textBox1.Text.Split('\n'))
                {
                    if (parser.TryParse(line.Trim(), out obj))
                    {
                        obj.Run();
                    }
                    else
                    {
                        MessageBox.Show("The line cannot be parsed: " + line);
                    }
                }
            
        }

        
    }
}
