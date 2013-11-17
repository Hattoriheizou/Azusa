using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUTAN_proto
{
    static class dummyAZUSA
    {
        static public void CallRoutine(string RID, string arg)
        {
            MessageBox.Show("Routine with RID \""+RID+"\" has been called with argument \""+arg+"\".");
        }

        static public void CreateLoop(string line)
        {
            MessageBox.Show("Looping "+line);
        }
    }
}
