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
    public partial class DBG_STATMON : Form
    {
        public DBG_STATMON()
        {
            InitializeComponent();
            MakeDraggable();
            
        }

        private void DBG_STATMON_Load(object sender, EventArgs e)
        {
            
                label1.Text = StatusMonitor.CurrentStatus;                
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = StatusMonitor.CurrentStatus;
        }
    }
}
