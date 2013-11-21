using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Reflection;

namespace AZUSA
{
    public class AZUSAAppContext : ApplicationContext
    {
        

        public AZUSAAppContext()
        {
            Internals.INIT();
            

            //Start the engines
            string EngPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Engines";
            string[] EngList = System.IO.Directory.GetFiles(EngPath, "*.exe");
            foreach (string exePath in EngList)
            {                
                ProcessManager.AddProcess(exePath.Replace(EngPath + @"\", "").Replace(".exe", "").Trim(), exePath);
            }
        }
    }
}
