using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUTAN_proto
{
    //The Runnable interface is a representation of "line" in the definition of MUTAN
    //The only important feature is that it can be "run" since it is a valid syntax
    //The detailed implementation of how to "run" a line is defered to more concrete classes
    //Implementations here are just for testing purposes, please do NOT use them directly
    //They should be rewritten when combining to other parts of AZUSA.

    interface IRunnable{
        bool Run();
    }

    //The simple decla statement
    class decla :IRunnable
    {
        string ID;
        string expr;
        public decla(string line)
        {
            ID = line.Split('=')[0];
            expr = line.Replace(ID + "=", "");
            ID=ID.Trim();
        }

        public bool Run()
        {
            string val;
            if (ExprParser.TryParse(expr, out val))
            {
                Variables.Write(ID, val);
                return true;
            }
            return false;
        }
    }

    //The simple exec statement
    class exec : IRunnable
    {
        string RID;
        string arg;
        public exec(string line)
        {
            RID = line.Split(':')[0];
            arg = line.Replace(RID + ":", "");
            RID = RID.Trim();
        }

        public bool Run()
        {
            string val;
            if (ExprParser.TryParse(arg, out val))
            {
                dummyAZUSA.CallRoutine(RID, val);
                return true;
            }
            return false;
        }
    }

    //The simple multi statement
    class multi : IRunnable
    {
        IRunnable[] basics;
        public multi(IRunnable[] lines)
        {
            basics = lines;
        }

        public bool Run()
        {
            foreach (IRunnable basic in basics)
            {
                if (!basic.Run())
                {
                    return false;
                }                
            }
            return true;
        }
    }

}
