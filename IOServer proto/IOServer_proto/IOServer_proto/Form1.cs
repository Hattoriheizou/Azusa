using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IOServer_proto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void WriteText(string msg){
            textBox1.Text+=msg+Environment.NewLine;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dummyAZUSA.RegisterFormControl(this);
            string EngPath=System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+@"\Engines";
            string[] EngList=System.IO.Directory.GetFiles(EngPath,"*.exe");
            foreach (string exe in EngList)
            {
                dummyAZUSA.Print("DEBUG: " + exe);
                ProcessManager.AddProcess(exe.Replace(EngPath+@"\","").Replace(".exe","").Trim(), exe);
                
                foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
                {
                    dummyAZUSA.Print("[" + prc.Name + " is running.]");
                }
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessManager.KillAll();
        }
    }
}
