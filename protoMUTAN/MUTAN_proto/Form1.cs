﻿using System;
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
            MUTAN.Parser parser = new MUTAN.Parser();
            MUTAN.IRunnable obj;

            if (parser.TryParse(textBox1.Text.Split('\n'), out obj))
            {
                MUTAN.ReturnCode[] returns = obj.Run();

                foreach (MUTAN.ReturnCode code in returns)
                {
                    if (code.Command != "")
                    {
                        MessageBox.Show(code.Command + "  " + code.Argument);
                    }
                }

                MessageBox.Show("Done");
            }
            else
            {
                MessageBox.Show("Syntax error");                
                
            }




        }


    }
}
