using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUTAN_proto
{
    static class dummyAZUSA
    {
        static public bool CallRoutine(string RID, string arg)
        {
            MessageBox.Show("Routine with RID \""+RID+"\" has been called with argument \""+arg+"\".");
            return true;
        }

        static public bool CreateLoop(string line)
        {
            MessageBox.Show("Looping "+line);
            return true;
        }

        static public bool CreateLoop(string[] lines)
        {
            string msg="";
            foreach (string line in lines)
            {
                msg += line + ",";
            }
            MessageBox.Show("Looping "  + msg.TrimEnd(','));
            return true;
        }
    }
}
