using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Azusa
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frmMain MainWin;
            if (Args.Length != 0)
            {
                MainWin = new frmMain(Args[0]);
            }
            else
            {
                MainWin = new frmMain();
            }
            
            Application.Run(MainWin);
            

            
            
        }
    }
}
