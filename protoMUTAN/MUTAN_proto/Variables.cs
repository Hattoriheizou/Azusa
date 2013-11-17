using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUTAN_proto
{
    static class Variables
    {
        static public bool Exist(string name){

            if (name== "VAR")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public string Read(string name)
        {

            return "NYAN";


        }
    }
}
