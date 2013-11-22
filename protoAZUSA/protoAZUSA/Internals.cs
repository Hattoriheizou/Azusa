using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AZUSA
{
    class Internals
    {

        static NotifyIcon notifyIcon = new NotifyIcon();

        static public void INIT()
        {
            //Load all the variables
            Variables.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA");

            //Set up notify icon
            notifyIcon.Icon = AZUSA.Properties.Resources.icon;
            notifyIcon.Visible = true;

            MenuItem itmRELD = new MenuItem("Reload");
            itmRELD.Click += new EventHandler(itmRELD_Click);
            MenuItem itmEXIT = new MenuItem("Exit");
            itmEXIT.Click += new EventHandler(itmEXIT_Click);
            ContextMenu menu = new ContextMenu(new MenuItem[]{itmEXIT,itmRELD});

            notifyIcon.ContextMenu = menu;


            //Start the engines
            string EngPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Engines";
            string[] EngList = System.IO.Directory.GetFiles(EngPath, "*.exe");
            foreach (string exePath in EngList)
            {
                ProcessManager.AddProcess(exePath.Replace(EngPath + @"\", "").Replace(".exe", "").Trim(), exePath);
            }

            System.Threading.Thread.Sleep(1000);

            if (!ProcessManager.CheckCompleteness())
            {
                notifyIcon.ShowBalloonTip(1000, "AZUSA", "Some engines are missing. AZUSA will not function unless AI and I/O are all registered.", ToolTipIcon.Error);                
            }
        }

        static void itmRELD_Click(object sender, EventArgs e)
        {
            RESTART();
        }
        static void itmEXIT_Click(object sender, EventArgs e)
        {
            EXIT();
        }

        static public void EXIT()
        {
            ThreadManager.BreakAll();
            ProcessManager.KillAll();
            Variables.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA");
            notifyIcon.Dispose();
            Application.Exit();
        }

        static public void RESTART()
        {
            ThreadManager.BreakAll();
            ProcessManager.KillAll();
            Variables.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA");
            notifyIcon.Dispose();
            Application.Restart();
        }

        static public void ERROR(string msg)
        {
            notifyIcon.ShowBalloonTip(5000, "AZUSA", msg, ToolTipIcon.Error);
        }

        static public void MESSAGE(string msg)
        {
            notifyIcon.ShowBalloonTip(5000, "AZUSA", msg, ToolTipIcon.Info);
        }



        static public void Execute(string cmd, string arg)
        {
            bool routed = false;
            //Check if the RID needs to be routed to engines
            foreach (IOPortedPrc prc in ProcessManager.GetCurrentProcesses())
            {
                if (prc.RIDs.ContainsKey(cmd))
                {
                    routed = true;
                    if (prc.RIDs[cmd])
                    {
                        prc.Engine.StandardInput.WriteLine(arg);
                    }
                    else
                    {
                        prc.Engine.StandardInput.WriteLine(cmd + "(" + arg + ")");
                    }                    
                }
            }

            //No need to continue executing the command because it has been routed already
            if (routed) { return; }            

            //Internal commands
            switch (cmd)
            {
                case "":
                    //do nothing
                    break;
                case "VAR":
                    string ID = arg.Split('=')[0];
                    string val = arg.Replace(ID + "=", "").Trim();
                    Variables.Write(ID, val);
                    break;
                case "LOOP":
                    string[] content = new string[] { arg };
                    ThreadManager.AddLoop(content);
                    break;
                case "MLOOP":
                    ThreadManager.AddLoop(arg.Split('\n'));
                    break;
                case "SCRIPT":
                    MUTAN.IRunnable obj;
                    
                    string[] scr = arg.Split('.');
                    string[] program;
                    try
                    {
                        program = File.ReadAllLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Scripts\" + scr[0]);
                    }
                    catch
                    {
                        Internals.ERROR("Unable to find the script named " + scr[0] + ". Please make sure it is in the correct folder.");
                        return;
                    }

                    if (scr.Length == 2)
                    {
                        MUTAN.Parser.TryParse(program, out obj, scr[1].Trim());                        
                    }
                    else
                    {
                        MUTAN.Parser.TryParse(program, out obj);
                    }

                    foreach (MUTAN.ReturnCode code in obj.Run())
                    {
                        Execute(code.Command, code.Argument);
                    }
                    break;
                case "WAIT":
                    System.Threading.Thread.Sleep(Convert.ToInt32(arg));
                    break;
                case "ERR":
                    ERROR(arg);
                    break;
                case "MSG":
                    MESSAGE(arg);
                    break;
                case "EXIT":
                    EXIT();
                    break;
                case "RESTART":
                    RESTART();
                    break;
                default:
                    ProcessManager.AddProcess(cmd, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Routines\" + cmd, arg);
                    break;
            }

        }

        static public void Execute(string cmd, string arg, LoopThread caller)
        {
            //check if there is a break loop command
            if (cmd.Trim() == "BREAK")
            {
                caller.Break();
                return;
            }

            //else, execute using the usual routine
            Execute(cmd, arg);

        }
    }
}
