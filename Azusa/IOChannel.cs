using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Azusa
{
    /* Class name: Input Channel
     *
     * Description:
     * The class allows the program to start, terminate and control the input channel, be it a speech recognizer, a kinect or anything else
     * 
     * This is achieve through a hidden, CreateNoForm Process running the console application which is responsible for translating the input into
     * understandable command, and communicating with it through standard console I/O
     */

    class IOChannel
    {
        string Name="";
        Process Engine;

        public IOChannel(string name,string enginePath,string arg="",bool win=false)
        {


            try
            {
                Name = name;
                //specifies the way the recognizer is run
                Engine = new Process();
                Engine.StartInfo = new ProcessStartInfo(enginePath,arg);
                if (!win)
                {
                    Engine.StartInfo.UseShellExecute = false;
                    Engine.StartInfo.CreateNoWindow = true;
                    Engine.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                }
                
                
                //port I/O to allow communication
                Engine.StartInfo.RedirectStandardOutput = true;
                Engine.StartInfo.RedirectStandardInput = true;
                

            }
            catch
            {
                Notifier.ErrorMsg("Unable to load engine at " + enginePath + ".");
                
            }
        }

        public void Start()
        {
            try
            {
                Engine.Start();
                Engine.BeginOutputReadLine();               
                Engine.OutputDataReceived += new DataReceivedEventHandler(Recognizer_OutputDataReceived);
                Scripting.ProcessServer.RegisterProcess(Engine);
            }
            catch
            {
                Notifier.ErrorMsg("Unable to start the engine " + Name + ".");
            }
        }

        

        void Recognizer_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //The engine seems to be throwing unreadable output when it exits
            //Thus we need to make sure that the engine is still up and running            
            DbgOutput.Write(e.Data);
            if (e.Data!=null) 
            {

                try
                {
                    switch (e.Data.Trim())
                    {
                        
                        case "STATUS?":
                            Engine.StandardInput.WriteLine(StatusMonitor.CurrentStatus);
                            break;
                        case "PosX?":
                            Engine.StandardInput.WriteLine(Scripting.ScriptEngine.GetParent().Location.X);
                            break;
                        case "PosY?":
                            Engine.StandardInput.WriteLine(Scripting.ScriptEngine.GetParent().Location.Y);
                            break;
                        case "Width?":
                            Engine.StandardInput.WriteLine(Scripting.ScriptEngine.GetParent().Size.Width);
                            break;
                        case "Height?":
                            Engine.StandardInput.WriteLine(Scripting.ScriptEngine.GetParent().Size.Height);
                            break; 
                        //time variables
                        case "s?":
                            //Engine.StandardInput.WriteLine(DateTime.Now.ToString("s",new CultureInfo(Configuration.culture)));
                            Engine.StandardInput.WriteLine(DateTime.Now.Second.ToString());                            
                            break;
                        case "m?":
                            Engine.StandardInput.WriteLine(DateTime.Now.Minute.ToString());
                            break;
                        case "h?":
                            Engine.StandardInput.WriteLine(DateTime.Now.ToString("hh", new CultureInfo(Configuration.culture)));
                            break;
                        case "t?":
                            Engine.StandardInput.WriteLine(DateTime.Now.ToString("tt", new CultureInfo(Configuration.culture)));
                            break;
                        case "D?":
                            Engine.StandardInput.WriteLine(DateTime.Now.ToString("dd", new CultureInfo(Configuration.culture)));
                            break;
                        case "M?":
                            Engine.StandardInput.WriteLine(DateTime.Now.ToString("MMMM", new CultureInfo(Configuration.culture)));
                            break;
                        case "Y?":
                            Engine.StandardInput.WriteLine(DateTime.Now.ToString("yyy", new CultureInfo(Configuration.culture)));
                            break;
                        case "d?":
                            Engine.StandardInput.WriteLine(DateTime.Now.ToString("dddd", new CultureInfo(Configuration.culture)));
                            break;
                
                        default:
                            //check if asking for a response, search user defined variables first and return if any, otherwise try parsing as an condition and return true/false.
                            //otherwise run as a script
                            if (e.Data.Trim().EndsWith("?"))
                            {
                                if (Configuration.usrDef.ContainsKey(e.Data.Trim().Trim('?')))
                                {
                                    Engine.StandardInput.WriteLine(Configuration.usrDef[e.Data.Trim().Trim('?')]);
                                }
                                else
                                {
                                    try
                                    {
                                        if (Scripting.ConditionParser.Parse(e.Data.Trim().Trim('?')))
                                        {
                                            Engine.StandardInput.WriteLine("true");
                                        }
                                        else
                                        {
                                            Engine.StandardInput.WriteLine("false");
                                        }
                                    }
                                    catch { }
                                }
                            }
                            else
                            {
                                
                                Scripting.ScriptEngine.Run("*"+e.Data.Trim());
                               
                            }
                            break;
                    }

                }
                catch
                {
                    Notifier.ErrorMsg("Unable to run script: " + e.Data + "\n Engine: " + Name);
                }
            }
        }

        public void Stop()
        {           

            try
            {
                Engine.CancelOutputRead();
                Engine.Kill(); 
                Engine.Dispose();
                Engine = null;
            }
            catch
            {
                Notifier.ErrorMsg("Unable to stop the engine \"" + Name + "\".");
            }
        }

    }
}
