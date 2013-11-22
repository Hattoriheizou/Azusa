using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AZUSA
{
    enum EngineType { Input, Output, AI, Routine }



    class IOPortedPrc
    {
        public string Name;
        public int pid;
        public Process Engine;
        public EngineType Type;

        public List<string> Ports = new List<string>();
        public Dictionary<string, bool> RIDs = new Dictionary<string, bool>();

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

            pid = Engine.Id;

        }

        public void Pause()
        {
            try
            {
                Engine.CancelOutputRead();
            }
            catch { }
        }

        public void Resume()
        {
            Engine.BeginOutputReadLine();
        }

        public void End()
        {
            try
            {
                Engine.CancelOutputRead();
            }
            catch { }
            Engine.Kill();
            Engine.Dispose();
            Engine = null;
            if (Type == EngineType.AI)
            {
                ProcessManager.AIPid.Remove(pid);
            }
            else if (Type == EngineType.Input)
            {
                ProcessManager.InputPid.Remove(pid);
            }
            else if (Type == EngineType.Output)
            {
                ProcessManager.OutputPid.Remove(pid);
            }

        }


        void Engine_Exited(object sender, EventArgs e)
        {
            ProcessManager.RemoveProcess(this);
            try
            {
                Engine.CancelOutputRead();
            }
            catch { }
            if (Type == EngineType.AI)
            {
                ProcessManager.AIPid.Remove(pid);
            }
            else if (Type == EngineType.Input)
            {
                ProcessManager.InputPid.Remove(pid);
            }
            else if (Type == EngineType.Output)
            {
                ProcessManager.OutputPid.Remove(pid);
            }
        }

        void Engine_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //Ignore NULL and empty inputs that will crash the program
            if (e.Data == null || e.Data.Trim() == "")
            {
                return;
            }

            //First check if the engine is asking a question about value of an expression
            if (e.Data.EndsWith("?"))
            {
                string result;
                MUTAN.ExprParser.TryParse(e.Data.TrimEnd('?'), out result);
                Engine.StandardInput.WriteLine(result);
                return;
            }


            //If no then assume it is a MUTAN command and try parsing, if failed to parse, ignore.
            MUTAN.IRunnable obj;
            if (MUTAN.LineParser.TryParse(e.Data, out obj))
            {
                MUTAN.ReturnCode[] returns = obj.Run();

                foreach (MUTAN.ReturnCode code in returns)
                {

                    //Handle NYAN protocol related commands, leave the rest to AZUSA internals
                    switch (code.Command)
                    {
                        case "Debugging":
                            ProcessManager.AIPid.Add(pid);
                            ProcessManager.InputPid.Add(pid);
                            ProcessManager.OutputPid.Add(pid);
                            break;
                        case "GetAzusaPid":
                            Engine.StandardInput.WriteLine(Process.GetCurrentProcess().Id);
                            break;
                        case "RegisterAs":
                            switch (code.Argument)
                            {
                                case "AI":
                                    this.Type = EngineType.AI;
                                    ProcessManager.AIPid.Add(pid);
                                    break;
                                case "Input":
                                    this.Type = EngineType.Input;
                                    ProcessManager.InputPid.Add(pid);
                                    break;
                                case "Output":
                                    this.Type = EngineType.Output;
                                    ProcessManager.OutputPid.Add(pid);
                                    break;
                                case "Routine":
                                    this.Type = EngineType.Routine;
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "RegisterPort":
                            this.Ports.Add(code.Argument);
                            break;
                        case "GetInputPorts":
                            string result = "";
                            foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
                            {
                                if (prc.Type == EngineType.Input)
                                {
                                    foreach (string port in prc.Ports)
                                    {
                                        result += port + ",";
                                    }
                                }
                            }

                            Engine.StandardInput.WriteLine(result.Trim(','));

                            break;
                        case "LinkRID":
                            string[] parsed = code.Argument.Split(',');

                            this.RIDs.Add(parsed[0], Convert.ToBoolean(parsed[1]));

                            break;
                        default:
                            if (ProcessManager.CheckCompleteness())
                            {
                                Internals.Execute(code.Command, code.Argument);
                            }

                            break;
                    }
                }
            }

        }



    }
}
