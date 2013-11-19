using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOServer_proto
{
    static class dummyAZUSA
    {
        static Form1 parent;
        static public void RegisterFormControl(Form1 frm)
        {
            parent = frm;
        }


        private delegate void CrossThreadCallBackStr(string var);


        static public void Print(string msg)
        {
            if (parent.InvokeRequired)
            {
                parent.Invoke(new CrossThreadCallBackStr(Print), msg);
            }
            else
            {
                parent.WriteText(msg);

            }

        }
    }
}
