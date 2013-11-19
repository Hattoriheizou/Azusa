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

        public IOPortedPrc(string name, string enginePath, string arg = "")
        {

            Name = name;
            //specifies the way the recognizer is run
            Engine = new Process();
            Engine.StartInfo = new ProcessStartInfo(enginePath, arg);

            Engine.StartInfo.UseShellExecute = false;
            Engine.StartInfo.CreateNoWindow = true;
            Engine.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;



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
            dummyAZUSA.Print("[" + Name + " has started.]");

        }

        public void Pause()
        {
            Engine.CancelOutputRead();
        }

        public void Resume()
        {
            Engine.BeginOutputReadLine();
        }

        public void End()
        {

            Engine.CancelOutputRead();
            Engine.Kill();
            Engine.Dispose();
            Engine = null;

            dummyAZUSA.Print("[" + Name + " has ended.]");

            foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
            {
                dummyAZUSA.Print("[" + prc.Name + " is still running.]");
            }

        }


        void Engine_Exited(object sender, EventArgs e)
        {
            ProcessManager.RemoveProcess(this);
            dummyAZUSA.Print("[" + Name + " has ended.]");

            foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
            {
                dummyAZUSA.Print("[" + prc.Name + " is still running.]");
            }
        }

        void Engine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //from who??
            dummyAZUSA.Print(sender.ToString() + " : " + e.Data);
            
        }



    }
}
