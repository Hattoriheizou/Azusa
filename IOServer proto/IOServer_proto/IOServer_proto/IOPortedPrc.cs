using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IOServer_proto
{
    class IOPortedPrc
    {
        public string Name;
        public Process Engine;

        public IOPortedPrc(string name, string enginePath, string arg = "", bool win = false)
        {
          
                Name = name;
                //specifies the way the recognizer is run
                Engine = new Process();
                Engine.StartInfo = new ProcessStartInfo(enginePath, arg);
                if (!win)
                {
                    Engine.StartInfo.UseShellExecute = false;
                    Engine.StartInfo.CreateNoWindow = true;
                    Engine.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }


                //port I/O to allow communication
                Engine.StartInfo.RedirectStandardOutput = true;
                Engine.StartInfo.RedirectStandardInput = true;
                Engine.OutputDataReceived += new DataReceivedEventHandler(Engine_OutputDataReceived);

                //handling process exit
                Engine.EnableRaisingEvents = true;
                Engine.Exited += new EventHandler(Engine_Exited);

        }



        public void Start()
        {
            
                Engine.Start();
                Engine.BeginOutputReadLine();

        }

        public void End()
        {

                Engine.CancelOutputRead();
                Engine.Kill();
                Engine.Dispose();
                Engine = null;
            
        }


        void Engine_Exited(object sender, EventArgs e)
        {
            ProcessManager.RemoveProcess(this);
        }

        void Engine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //from who??
            dummyAZUSA.Print(sender.ToString() + " : " + e.Data);
        }

       

    }
}
