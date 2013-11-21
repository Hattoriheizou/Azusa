using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AZUSA
{
    enum EngineType{Input,Output,AI,Routine }

    struct RIDLinkage{
        public string RID;
        public bool ArgOnly;
    }


    class IOPortedPrc
    {
        public string Name;
        public Process Engine;
        public EngineType Type;

        public List<int> Ports = new List<int>();
        public List<RIDLinkage> RIDs = new List<RIDLinkage>();

        public IOPortedPrc(string name, string enginePath, string arg = "")
        {

            Name = name;
            
            //default type is Routine until the Engine self-identifies itself
            Type = EngineType.Routine;

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
            Engine.StandardInput.AutoFlush = true;

            //dummyAZUSA.Print("[" + Name + " has started.]");

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

            //dummyAZUSA.Print("[" + Name + " has ended.]");

            foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
            {
                //dummyAZUSA.Print("[" + prc.Name + " is still running.]");
            }

        }


        void Engine_Exited(object sender, EventArgs e)
        {
            ProcessManager.RemoveProcess(this);
            Engine.CancelOutputRead();


            //dummyAZUSA.Print("[" + Name + " has ended.]");

            foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
            {
                //dummyAZUSA.Print("[" + prc.Name + " is still running.]");
            }
        }

        void Engine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //dummyAZUSA.Print(Engine.Id + " : " + e.Data);

            if (e.Data.Trim() == "NYAN")
            {
                Engine.StandardInput.WriteLine("NYAN");
            }

            if (e.Data.Trim() == "WAIT")
            {
                System.Threading.Thread.Sleep(8000);
            }
        }



    }
}
