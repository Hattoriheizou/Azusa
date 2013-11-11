using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Azusa
{

    public partial class frmMain : Form
    {

        static string exec = "";
        public frmMain(string execpath = "")
        {
            InitializeComponent();

            exec = execpath;
        }

        //For API Graphics Calls
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00080000; // Required: set WS_EX_LAYERED extended style
                return cp;
            }
        }


        private void frmMain_Load(object sender, EventArgs e)
        {


            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //load settings
            Configuration.Import(currentPath + @"\Settings\AZUSA.txt");

            //start scripting services and process server
            Scripting.ProcessServer.Initiate();

            //Register form controls
            Scripting.ScriptEngine.RegisterFormControl(this);


            //apply settings            
            this.Location = new Point(Configuration.frmPosX - Configuration.frmWidth, Configuration.frmPosY - Configuration.frmHeight);
            this.Size = new Size(Configuration.frmWidth, Configuration.frmHeight);
            if (Configuration.draggable) { MakeDraggable(); }
            notifyIcon1.Visible = Configuration.showicon;
            aniTimer.Interval = Configuration.aniRate;



            //initialize flash screen


            //run start up script
            Scripting.ScriptEngine.Run("SCRIPT:SYS.startup");



            //run all input channels
            foreach (string engexe in Directory.EnumerateFiles(currentPath + @"\Engines\", "*.exe", SearchOption.AllDirectories))
            {
                IOChannel Eng = new IOChannel(engexe.Replace(currentPath + @"\Engines\", ""), engexe);
                Eng.Start();
            }

            if (Configuration.debugging)
            {

                DBG_STATMON DbgWin = new DBG_STATMON();
                DbgWin.Show();

                DBG_OUTPUT DbgOut = new DBG_OUTPUT();
                DbgOut.Show();
            }

            if (exec != "")
            {
                Scripting.ScriptEngine.Run("EXEC:" + exec);
            }


        }

        private void frmMain_Deactivate(object sender, EventArgs e)
        {

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            StatusMonitor.EXITFLAG = true;

            Scripting.ScriptEngine.Run("SCRIPT:SYS.exit");

            #region Update AZUSA.txt to store global variables

            List<string> newConfig = new List<string>();
            string[] parsed;

            //update old values
            foreach (string line in File.ReadAllLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings\AZUSA.txt"))
            {
                parsed = line.Split('=');

                //see if the line is defining a variable
                if (parsed.Length == 2)
                {
                    //see if the variable already exists and the value is the same
                    if (Configuration.usrDef.ContainsKey(parsed[0].Trim()))
                    {
                        if (Configuration.usrDef[parsed[0].Trim()].Trim() == parsed[1].Trim())
                        {
                            newConfig.Add(line);
                        }
                        else //use new value
                        {
                            newConfig.Add(parsed[0].Trim() + "=" + Configuration.usrDef[parsed[0].Trim()].Trim());
                        }
                        Configuration.usrDef.Remove(parsed[0].Trim());
                    }
                    else
                    {
                        newConfig.Add(line);
                    }

                }
                else
                {
                    newConfig.Add(line);
                }
            }

            //add new entries
            foreach (KeyValuePair<string, string> pair in Configuration.usrDef)
            {
                if (!pair.Key.StartsWith("$"))
                {
                    newConfig.Add(pair.Key + "=" + pair.Value);
                }
            }


            File.WriteAllLines(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings\AZUSA.txt", newConfig);
            #endregion

            

            //End all processes
            Scripting.ProcessServer.KillAll();


        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scripting.ScriptEngine.Run("SCRIPT:SYS.close");
        }


        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scripting.ScriptEngine.Run("SCRIPT:SYS.reload");
        }

        private void showHideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                Scripting.ScriptEngine.Run("SCRIPT:SYS.hide");

            }
            else
            {
                Scripting.ScriptEngine.Run("SCRIPT:SYS.show");
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            showHideToolStripMenuItem_Click(this, EventArgs.Empty);
        }


        List<string> tmplist;
        private void aniTimer_Tick(object sender, EventArgs e)
        {
           
            //handles animation
            if (StatusMonitor.currentAniFrames.Count > StatusMonitor.currentFrame && this.Visible)
            {
                string frame = StatusMonitor.currentAniFrames[StatusMonitor.currentFrame];
                //put image
                if (!MediaCache.preloadedImg.ContainsKey(frame))
                {
                    if (File.Exists(Environment.CurrentDirectory + @"\Media\img\" + frame))
                    {

                        MediaCache.preloadedImg.Add(frame, Image.FromFile(Environment.CurrentDirectory + @"\Media\img\" + frame));

                    }
                }
                if (!StatusMonitor.dragging)
                {
                    this.Location = new Point(Configuration.frmPosX - MediaCache.preloadedImg[frame].Width, Configuration.frmPosY - MediaCache.preloadedImg[frame].Height);
                }

                APIDraw.UpdateFormDisplay(MediaCache.preloadedImg[frame], this);
                Configuration.frmHeight = MediaCache.preloadedImg[frame].Height;
                Configuration.frmWidth = MediaCache.preloadedImg[frame].Width;


                //shift frame
                if (StatusMonitor.currentFrame != StatusMonitor.currentAniFrames.Count - 1)
                {
                    StatusMonitor.currentFrame = StatusMonitor.currentFrame + 1;
                }
                else
                {
                    StatusMonitor.currentFrame = 0;
                }
            }

            //handles WAIT command
            if (StatusMonitor.waiting)
            {
                if (StatusMonitor.waittime > 0)
                {
                    StatusMonitor.waittime -= aniTimer.Interval;

                }
                else
                {
                    StatusMonitor.waiting = false;
                    if (StatusMonitor.pausedScr.Count > 0)
                    {
                        tmplist = new List<string>(StatusMonitor.pausedScr);

                        Scripting.ScriptEngine.RunFromList(tmplist);
                        
                        foreach (string cmd in tmplist)
                        {                            
                            StatusMonitor.pausedScr.Remove(cmd) ;
                        }

                        
                    }

                }
            }
        }


    }
}
