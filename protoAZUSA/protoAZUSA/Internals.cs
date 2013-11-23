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
            //從 DATA 載入所有已儲存的變量
            //Load all the variables
            if (File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA"))
            {
                Variables.Load(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA");
            }

            //創建提示圖標
            //Set up notify icon
            notifyIcon.Icon = AZUSA.Properties.Resources.icon;
            notifyIcon.Visible = true;

            //創建圖標右擊菜單的項目
            MenuItem itmRELD = new MenuItem("Reload"); //重新載入
            itmRELD.Click += new EventHandler(itmRELD_Click); 
            MenuItem itmEXIT = new MenuItem("Exit"); //退出
            itmEXIT.Click += new EventHandler(itmEXIT_Click);
            ContextMenu menu = new ContextMenu(new MenuItem[]{itmEXIT,itmRELD});

            //把圖標右擊菜單設成上面創建的菜單
            notifyIcon.ContextMenu = menu;

            //搜索 Engines\ 底下的所有執行檔, SearchOption.AllDirectories 表示子目錄也在搜索範圍內
            //Start the engines
            string EngPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Engines";
            string[] EngList = System.IO.Directory.GetFiles(EngPath, "*.exe",SearchOption.AllDirectories);

            //每一個執行檔都添加為引擎
            foreach (string exePath in EngList)
            {
                ProcessManager.AddProcess(exePath.Replace(EngPath + @"\", "").Replace(".exe", "").Trim(), exePath);
            }

            //等待一秒鐘, 讓各引擎做好初始化和登錄
            System.Threading.Thread.Sleep(1000);

            //一秒後如果 AI, 輸入, 輸出不齊備的話就對用戶作出提示
            //如果引擎不齊備的話, 所有 NYAN 指令組以外的指令不會被執行
            //NYAN 指令組的具體內容請看 IOPortedPrc
            if (!ProcessManager.CheckCompleteness())
            {
                notifyIcon.ShowBalloonTip(1000, "AZUSA", "Some engines are missing. AZUSA will not function unless AI and I/O are all registered.", ToolTipIcon.Error);                
            }

            //初始化到此結束, 然後就是各 IOPortedPrc 聽取和執行引擎的指令了
        }

        static void itmRELD_Click(object sender, EventArgs e)
        {
            RESTART();
        }
        static void itmEXIT_Click(object sender, EventArgs e)
        {
            EXIT();
        }

        //結束程序
        static public void EXIT()
        {
            //中止一切線程
            ThreadManager.BreakAll();

            //結束一切進程
            ProcessManager.KillAll();

            //線程和進程結束後, 變數的值就不可能再有變化了
            //此時可以保存變數的值
            Variables.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA");

            //拋棄圖標
            notifyIcon.Dispose();

            //處理完畢, 可以通知程序退出
            Application.Exit();
        }

        //重啟程序
        static public void RESTART()
        {
            //中止一切線程
            ThreadManager.BreakAll();

            //結束一切進程
            ProcessManager.KillAll();

            //線程和進程結束後, 變數的值就不可能再有變化了
            //此時可以保存變數的值
            Variables.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\DATA");

            //拋棄圖標
            notifyIcon.Dispose();

            //處理完畢, 可以通知程序重啟
            Application.Restart();
        }

        //發出錯誤提示
        static public void ERROR(string msg)
        {
            notifyIcon.ShowBalloonTip(5000, "AZUSA", msg, ToolTipIcon.Error);
        }

        //發出普通提示
        static public void MESSAGE(string msg)
        {
            notifyIcon.ShowBalloonTip(5000, "AZUSA", msg, ToolTipIcon.Info);
        }


        //執行指令
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

                    if (obj != null)
                    {
                        foreach (MUTAN.ReturnCode code in obj.Run())
                        {
                            Execute(code.Command, code.Argument);
                        }
                    }
                    else
                    {
                        ERROR("An error occured while running script named " + scr[0]+". Please make sure there is no syntax error.");
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
            

            //else, execute using the usual routine
            Execute(cmd, arg);

        }
    }
}
