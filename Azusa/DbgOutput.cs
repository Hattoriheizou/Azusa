using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azusa
{
    class DbgOutput
    {
        static string content="";

        static public  void Write(string msg)
        {
            if(content.Length>1000){
                content = "";
            }
            content = msg + "\n"+content;
        }

        static public string Read()
        {
            
            return content;

        }
    }
}
