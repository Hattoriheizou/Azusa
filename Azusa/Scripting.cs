using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;
using System.Reflection;

namespace Azusa
{
    /* Class name: Scripting Engine
     * 
     * Description:
     * This class contains subclasses and functions that allows extension of program functions.
     * 
     * It works by running hidden, NoWindow, condition-checking Processes (CCP) and communicate with them using Console I/O.
     * If a Process detected a certain condition being satisfied, it will send a message to the scripting engine and require
     * the corresponding action-performing Process(es) (APP) to be run.
     * 
     * The message from CCP is passed on to APP(s) to allow single directional communication between them.
     * 
     * The correspondance between CCPs and AAPs are described by a script file which can be imported.
     * 
     * The script might also be edited/added/deleted and exported to a script file.
     * */

    namespace Scripting
    {


        class ProcessServer
        {
            static List<Process> CurrentProcesses;

            public ProcessServer()
            {

            }

            static public void Initiate()
            {
                CurrentProcesses = new List<Process>();
            }

            static public void RegisterProcess(Process prc)
            {
                CurrentProcesses.Add(prc);
            }

            static public void KillAll()
            {
                foreach (Process prc in CurrentProcesses)
                {
                    try
                    {
                        prc.Kill();
                    }
                    catch { }

                }
            }

            static public List<Process> GetCurrentProcesses()
            {
                return CurrentProcesses;
            }
        }

        class Script
        {
            public string Command = "";
            public string Args = "";

            public Script(string scr)
            {
                string script = scr;

                //first replace all variables with their values

                //user defined
                foreach (KeyValuePair<string, string> pair in Configuration.usrDef)
                {
                    script = script.Replace("{" + pair.Key + "}", pair.Value);
                }

                //system defined variables
                script = script.Replace("{STATUS}", StatusMonitor.CurrentStatus);
                script = script.Replace("{PosX}", ScriptEngine.GetParent().Location.X.ToString());
                script = script.Replace("{PosY}", ScriptEngine.GetParent().Location.Y.ToString());
                script = script.Replace("{Width}", ScriptEngine.GetParent().Size.Width.ToString());
                script = script.Replace("{Height}", ScriptEngine.GetParent().Size.Height.ToString());

                //system defined time variables
                DateTime now = DateTime.Now;
                CultureInfo cult = new CultureInfo(Configuration.culture);

                script = script.Replace("{s}", now.ToString("%s", cult));
                script = script.Replace("{m}", now.ToString("%m", cult));
                script = script.Replace("{h}", now.ToString("%h", cult));
                script = script.Replace("{t}", now.ToString("tt", cult));
                script = script.Replace("{D}", now.ToString("%d", cult));
                script = script.Replace("{M}", now.ToString("MMMM", cult));
                script = script.Replace("{Y}", now.ToString("yyy", cult));
                script = script.Replace("{d}", now.ToString("dddd", cult));




                string[] parsed = script.Trim().Split(':');





                Command = parsed[0];
                if (parsed.Length > 1)
                {
                    for (int i = 1; i < parsed.Length; i++)
                    {
                        if (i != parsed.Length - 1)
                        {
                            Args += parsed[i] + ":";
                        }
                        else
                        {
                            Args += parsed[i];
                        }

                    }

                }
                else
                {
                    Args = "";
                }
            }
        }

        class ScriptEngine
        {
            static Form parent;
            static FormController frm;
            static Dictionary<string, string> localVars = new Dictionary<string, string>();
            static List<string> cache = new List<string>();

            static public void RegisterFormControl(Form form)
            {
                parent = form;
                frm = new FormController(parent);

            }

            static public Form GetParent()
            {
                return parent;
            }

            static public void RunFromFile(string filePath, string section = "")
            {

                List<string> block = new List<string>();
                #region load specific section only
                // if only a secific section is needed
                if (section != "")
                {

                    bool blockfound = false;
                    int bracketcount = 0;
                    string line = "";
                    StreamReader sw = new StreamReader(filePath);

                    while (!blockfound || bracketcount != 0)
                    {
                        line = sw.ReadLine();
                        if (line == "." + section + "{")
                        {
                            blockfound = true;
                            bracketcount++;
                        }
                        else if (blockfound)
                        {
                            if (line == "}")
                            {
                                bracketcount--;
                                if (bracketcount != 0)
                                {
                                    block.Add(line);
                                }
                            }
                            else if (!line.StartsWith("#") && line.EndsWith("{"))
                            {
                                bracketcount++;
                                block.Add(line);
                            }
                            else
                            {
                                block.Add(line);
                            }
                        }
                    }

                    foreach (string ln in block)
                    {
                        DbgOutput.Write(ln);
                    }

                    RunFromList(block);
                    block.Clear();
                    return;
                }
                #endregion
                //read in script file


                try
                {
                    RunFromList(new List<string>(File.ReadAllLines(filePath)));
                }
                catch
                {
                    Notifier.ErrorMsg("Unable to load script file \"" + filePath + "\".");
                }
            }

            static public void RunFromList(List<string> cmd, Loop caller=null)
            {
                //if still waiting for response, stop waiting, run new script
                if (StatusMonitor.CurrentStatus == "WAITFORRESP")
                {
                    StatusMonitor.CurrentStatus = "NORMAL";
                    cache.Clear();
                }
                
                //interpret script file

                bool skip = false; //decide whether the line should be skipped under the current scope
                bool inloop = false;
                List<string> loopblock = new List<string>();
                int bracketcount = 0;

                int numLine = 0;
                string line;
                foreach (string ln in cmd)
                {
                    numLine++;
                    line = ln.Trim();

                    if (line == "" || line.StartsWith("#"))
                    {

                    }
                    else if (!line.StartsWith("*") && StatusMonitor.CurrentStatus == "WAITFORRESP" && caller == null)
                    {
                        cache.Add(line);
                    }
                    else
                    {
                        line=line.TrimStart('*');

                        try
                        {
                             //handle looping codes
                            if (!inloop && line == "@{")
                            {
                                inloop = true;
                                bracketcount++;
                            }
                            else if(inloop && line.EndsWith("{"))
                            {
                                bracketcount++;
                                loopblock.Add(line);
                            }else if(inloop && line=="}"){
                                bracketcount--;
                                if (bracketcount == 0)
                                {
                                    inloop = false;
                                    new Loop(new List<string>(loopblock));
                                    loopblock.Clear();

                                }
                                else
                                {
                                    loopblock.Add(line);
                                }
                            }
                            else if (inloop)
                            {
                                loopblock.Add(line);
                            }
                            else if (line == "}") //end of conditional region
                            {
                                skip = false;
                            }
                            else if (line == "}ELSE{")  // flip condition
                            {
                                skip = !skip;

                            }
                            else if (!skip)
                            {
                                if (line.EndsWith("{"))  //start of conditional region
                                {
                                    skip = !ConditionParser.Parse(line.Trim('{'));

                                }
                                else
                                {
                                    foreach (KeyValuePair<string, string> var in localVars)
                                    {
                                        line = line.Replace(var.Key, var.Value);
                                    }
                                    if (!ScriptEngine.Run(line,caller))
                                    {
                                        break;
                                    }

                                }
                            }
                        }
                        catch
                        {
                            Notifier.ErrorMsg("Unable to parse script at line " + numLine.ToString() + ":\n" + ln);
                        }
                    }
                }

                //clean up variables
                localVars.Clear();
                

            }

            static public bool Run(string script, Loop caller=null)
            {
                if (StatusMonitor.waiting && !script.StartsWith("*"))
                {
                    StatusMonitor.pausedScr.Add(script);
                    return true;
                }

                DbgOutput.Write(script);

                List<string> cachecopy;
                string _script = script.Trim().TrimStart('*');
                if (_script == "" || _script.StartsWith("#"))
                {
                    return true;
                }

                if (_script.StartsWith("@"))
                {
                    new Loop(_script.Trim('@'));
                    return true;
                }

                if (_script.Contains("?"))
                {
                    if (!_script.Split('?')[0].Contains(":")) // this is added because sometimes the argument for a command contains ?
                    {
                        if (!ConditionParser.Parse(_script.Split('?')[0])) { return true; } else { _script = script.Split('?')[1]; }

                    }
                }

                if (_script.Contains(";"))
                {
                    RunFromList(new List<string>(_script.Split(';')));
                    return true;
                }
                

                if (_script.Contains("=") && !_script.Contains("?") && !_script.Contains(":"))
                {

                    //local variable
                    if (_script.StartsWith("$"))
                    {
                        if (localVars.ContainsKey(_script.Split('=')[0].Trim()))
                        {
                            localVars[_script.Split('=')[0].Trim()] = ValueParser.Parse(_script.Substring(_script.IndexOf('=') + 1, _script.Length - _script.IndexOf('=') - 1).Trim());
                        }else{
                            localVars.Add(_script.Split('=')[0].Trim(), ValueParser.Parse(_script.Substring(_script.IndexOf('=') + 1, _script.Length - _script.IndexOf('=') - 1).Trim()));
                        }
                    }
                    else if (Configuration.usrDef.ContainsKey(_script.Split('=')[0].Trim()))
                    {
                        Configuration.usrDef[_script.Split('=')[0].Trim()] = ValueParser.Parse(_script.Substring(_script.IndexOf('=') + 1, _script.Length - _script.IndexOf('=') - 1).Trim());
                    }
                    else
                    {
                        Configuration.usrDef.Add(_script.Split('=')[0].Trim(), ValueParser.Parse(_script.Substring(_script.IndexOf('=') + 1, _script.Length - _script.IndexOf('=') - 1).Trim()));
                    }
                    return true;
                }


                Script scr = new Script(_script);

                switch (scr.Command.Trim())
                {
                    case "":

                        break;
                    case "WAIT":
                        if (caller != null)
                        {

                            System.Threading.Thread.Sleep(Convert.ToInt32(scr.Args));
                            
                        }
                        else
                        {
                            StatusMonitor.waiting = true;
                            StatusMonitor.waittime = Convert.ToInt32(scr.Args);
                        }
                        break;
                    case "WAITFORRESP":
                        StatusMonitor.CurrentStatus = "WAITFORRESP";
                        break;
                    case "END":
                        return false;
                    case "BREAK":
                        if (caller != null) { caller.Break(); }
                        break;
                    case "SHOWFRM":
                        if (!parent.Visible)
                        {
                            frm.ShowForm();
                        }
                        break;
                    case "HIDEFRM":
                        if (parent.Visible)
                        {
                            frm.HideForm();
                            StatusMonitor.CurrentStatus = "HIDDEN";
                        }
                        break;
                    case "CLOSEFRM":
                        frm.CloseForm();
                        break;
                    case "RESTART":
                        Application.Restart();
                        break;
                    case "EXEC":
                        if (scr.Args.Contains("$"))
                        {
                            if (scr.Args.Split('$')[0].Trim().EndsWith(".exe") && !scr.Args.Trim().StartsWith("*"))
                            {
                                IOChannel Eng = new IOChannel(scr.Args.Split('$')[0].Trim(), scr.Args.Split('$')[0].Trim(), scr.Args.Split('$')[1].Trim());
                                Eng.Start();
                            }
                            else
                            {
                                Process.Start(scr.Args.Split('$')[0].Trim().Trim('*'), scr.Args.Split('$')[1].Trim());
                            }
                        }
                        else
                        {
                            if (scr.Args.Trim().EndsWith(".exe") && !scr.Args.Trim().StartsWith("*"))
                            {
                                IOChannel Eng = new IOChannel(scr.Args.Trim(), scr.Args.Trim(), "");
                                Eng.Start();
                            }
                            else
                            {
                                Process.Start(scr.Args.Trim().Trim('*'));
                            }
                        }
                        break;
                    //case "LOCK":
                    //    StatusMonitor.LockedStatus.Push(StatusMonitor.CurrentStatus);
                    //    break;
                    //case "JOIN":
                    //    if (StatusMonitor.LockedStatus.Count != 0)
                    //    {
                    //        StatusMonitor.CurrentStatus = StatusMonitor.LockedStatus.Pop();
                    //        if (StatusMonitor.CurrentStatus != "WAITFORRESP")
                    //        {
                    //            StatusMonitor.waitingforresp = false;

                    //            cachecopy = new List<string>(cache);
                    //            foreach (string line in cachecopy)
                    //            {
                    //                DbgOutput.Write(line);
                    //            }
                    //            RunFromList(cachecopy);
                    //            foreach (string line in cachecopy)
                    //            {
                    //                cache.Remove(line);
                    //            }

                    //        }
                    //    }
                    //    break;
                    case "IMG":
                        StatusMonitor.currentAniFrames.Clear();
                        frm.PutImg(scr.Args.Trim());                        
                        break;
                    case "_IMG": //internal use only, for putting animation frames
                        frm.PutImg(scr.Args.Trim());
                        break;
                    case "ANIMATE":
                        StatusMonitor.currentAniFrames.Clear();
                        StatusMonitor.currentFrame = 0;
                        foreach (string path in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Media\img\" + scr.Args.Trim().Trim('\\') + @"\", "*.png", SearchOption.TopDirectoryOnly))
                        {
                            StatusMonitor.currentAniFrames.Add(path.Replace(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Media\img\", ""));
                        }
                        break;
                    case "MSG":
                        Notifier.ShowMsg(scr.Args);
                        break;
                    case "POSX":
                        frm.PosX(Convert.ToInt32(scr.Args.Trim()));
                        break;
                    case "POSY":
                        frm.PosY(Convert.ToInt32(scr.Args.Trim()));
                        break;
                    case "ACTIVATE":
                        frm.Activate();
                        break;
                    case "ERROR":
                        Notifier.ErrorMsg(scr.Args);
                        break;
                    case "SCRIPT":
                        if (scr.Args.Contains("."))
                        {
                            RunFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Scripts\" + scr.Args.Split('.')[0].Trim(), scr.Args.Split('.')[1]);
                        }
                        else
                        {
                            RunFromFile(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Scripts\" + scr.Args.Trim());
                        }
                        break;
                    case "STATUS":
                        StatusMonitor.CurrentStatus = scr.Args.Trim();
                        if (StatusMonitor.CurrentStatus != "WAITFORRESP")
                        {                           

                            cachecopy = new List<string>(cache);
                            foreach (string line in cachecopy)
                            {
                                DbgOutput.Write(line);
                                cache.Remove(line);
                            }                           
                            RunFromList(cachecopy);
                            

                        }
                        break;
                    case "SOUND":
                        SoundPlayer.Play(scr.Args.Trim());
                        break;
                    default:
                        try
                        {
                            IOChannel Eng = new IOChannel(scr.Command.Trim(), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Routines\" + scr.Command.Trim() + ".exe", scr.Args);
                            Eng.Start();
                        }
                        catch
                        {
                            Notifier.ErrorMsg("Unable to " + scr.Command.Trim() + " " + scr.Args.Trim() + ". \nMake sure all necessary files are under \"Routines\".");
                        }
                        break;
                }

                return true;

            }
        }

        class Loop
        {
            List<string> cmd;
            Thread scrLoop;
            bool terminate = false;

            public Loop(List<string> commands)
            {
                cmd = commands;
                scrLoop = new Thread(new ThreadStart(this.RunScript));
                scrLoop.Start();
            }

            public Loop(string command)
            {
                cmd = new List<string>();
                cmd.Add(command);
                scrLoop = new Thread(new ThreadStart(this.RunScript));
                scrLoop.Start();
            }

            ~Loop()
            {
                scrLoop.Abort();
                scrLoop = null;
            }

            public void Break()
            {
                terminate = true;
            }

            void RunScript()
            {
                while (!StatusMonitor.EXITFLAG && !terminate)
                {
                    
                    ScriptEngine.RunFromList(cmd,this);
                    
                }
            }
        }

        class ConditionParser
        {
            static char[] op = { '&', '|', '=', '<', '>' };
            static char[] evalop = { '+', '-', '*', '/', '^' };
            static char[] brac = { '(', ')' };
            static string[] parsed;
            static string tmp;
            public ConditionParser()
            {

            }

            static public bool Parse(string cond)
            {
                DbgOutput.Write("PARSING COND:" + cond);

                if (cond == "TRUE") { return true; }
                if (cond == "FALSE") { return false; }

                if (Configuration.usrDef.ContainsKey(cond))
                {
                    return Convert.ToBoolean(Configuration.usrDef[cond]);
                }
                
                if (cond.StartsWith("~") && cond.IndexOfAny(op)==-1) //Negation
                {
                    return !Parse(cond.Trim('~'));
                }

                if (cond.IndexOfAny(op) == -1) //if there are no operators, then the condition is a status check
                {
                    if (ValueParser.Parse(cond) == StatusMonitor.CurrentStatus) //Status condition
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (cond.IndexOf('(') != -1)
                    {
                        tmp = cond.Substring(cond.LastIndexOf("("), cond.IndexOf(")") - cond.LastIndexOf("(") + 1);
                        return Parse(cond.Replace(tmp, Parse(tmp.Trim(brac)).ToString().ToUpper()));
                    }

                    if (cond.IndexOf('&') != -1)
                    {
                        return Parse(cond.Split('&')[0].Trim()) && Parse(cond.Replace(cond.Split('&')[0]+"&",""));
                    }
                    if (cond.IndexOf('|') != -1)
                    {
                        return Parse(cond.Split('|')[0].Trim()) || Parse(cond.Replace(cond.Split('|')[0] + "|", ""));
                    }
                    if (cond.IndexOf('=') != -1)
                    {
                        parsed = cond.Split('=');

                        if (parsed[0].Trim() == parsed[1].Trim())
                        {
                            return true;
                        }

                        //check user-defined variables
                        if (Configuration.usrDef.ContainsKey(parsed[0].Trim()))
                        {
                            return ValueParser.Parse(parsed[1]).Trim() == Configuration.usrDef[parsed[0]];
                        }

                        //check time variables
                        switch (parsed[0].Trim())
                        {
                            case "Y":
                                return DateTime.Now.Year == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "M":
                                return DateTime.Now.Month == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "D":
                                return DateTime.Now.Day == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "h":
                                return DateTime.Now.Hour == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "m":
                                return DateTime.Now.Minute == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "s":
                                return DateTime.Now.Second == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "d":
                                return Convert.ToInt32(DateTime.Now.DayOfWeek) == Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            default:
                                return false;

                        }
                    }
                    if (cond.IndexOf('>') != -1)
                    {
                        parsed = cond.Split('>');

                        
                        //if (Convert.ToInt32(parsed[0].Trim()) > Convert.ToInt32(parsed[1].Trim()))
                        //{
                        //    return true;
                        //}
                        

                        //check user-defined variables
                        if (Configuration.usrDef.ContainsKey(parsed[0].Trim()))
                        {
                            return Convert.ToInt32(Configuration.usrDef[parsed[0]].Trim()) > Convert.ToInt32(ValueParser.Parse(parsed[1]).Trim());
                        }

                        switch (parsed[0].Trim())
                        {
                            case "Y":
                                return DateTime.Now.Year > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "M":
                                return DateTime.Now.Month > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "D":
                                return DateTime.Now.Day > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "h":
                                return DateTime.Now.Hour > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "m":
                                return DateTime.Now.Minute > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "s":
                                return DateTime.Now.Second > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "d":
                                return Convert.ToInt32(DateTime.Now.DayOfWeek) > Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            default:
                                return false;

                        }
                    }
                    if (cond.IndexOf('<') != -1)
                    {
                        parsed = cond.Split('<');

                        //if (Convert.ToInt32(parsed[0].Trim()) < Convert.ToInt32(parsed[1].Trim()))
                        //{
                        //    return true;
                        //}

                        //check user-defined variables
                        if (Configuration.usrDef.ContainsKey(parsed[0].Trim()))
                        {
                            return Convert.ToInt32(Configuration.usrDef[parsed[0]]) < Convert.ToInt32(ValueParser.Parse(parsed[1]).Trim());
                        }

                        switch (parsed[0].Trim())
                        {
                            case "Y":
                                return DateTime.Now.Year < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "M":
                                return DateTime.Now.Month < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "D":
                                return DateTime.Now.Day < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "h":
                                return DateTime.Now.Hour < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "m":
                                return DateTime.Now.Minute < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "s":
                                return DateTime.Now.Second < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            case "d":
                                return Convert.ToInt32(DateTime.Now.DayOfWeek) < Convert.ToInt32(ValueParser.Parse(parsed[1]));

                            default:
                                return false;

                        }
                    }


                }


                return false;
            }

        }

        class ValueParser
        {
            static char[] logicop = { '&', '|', '=', '<', '>', '~' };
            static char[] op = { '+', '-', '*', '/', '^' };
            static char[] brac = { '(', ')' };
            static string tmp;
            public ValueParser()
            {

            }

            static public string Parse(string val)
            {
                string value = val;
                //booleans
                if (value == "TRUE") { return "TRUE"; }
                if (value == "FALSE") { return "FALSE"; }


                //variables

                //user defined
                foreach (KeyValuePair<string, string> pair in Configuration.usrDef)
                {
                    value = value.Replace("{" + pair.Key + "}", pair.Value);
                }

                //system defined variables
                value = value.Replace("{STATUS}", StatusMonitor.CurrentStatus);
                value = value.Replace("{PosX}", ScriptEngine.GetParent().Location.X.ToString());
                value = value.Replace("{PosY}", ScriptEngine.GetParent().Location.Y.ToString());
                value = value.Replace("{Width}", ScriptEngine.GetParent().Size.Width.ToString());
                value = value.Replace("{Height}", ScriptEngine.GetParent().Size.Height.ToString());

                //system defined time variables
                DateTime now = DateTime.Now;
                CultureInfo cult = new CultureInfo(Configuration.culture);

                value = value.Replace("{s}", now.ToString("%s", cult));
                value = value.Replace("{m}", now.ToString("%m", cult));
                value = value.Replace("{h}", now.ToString("%h", cult));
                value = value.Replace("{t}", now.ToString("tt", cult));
                value = value.Replace("{D}", now.ToString("d", cult));
                value = value.Replace("{M}", now.ToString("MMMM", cult));
                value = value.Replace("{Y}", now.ToString("yyy", cult));
                value = value.Replace("{d}", now.ToString("dddd", cult));

                if (value.IndexOfAny(logicop) == -1 && value.IndexOfAny(op) == -1) //if there are no operators, then the value is either a string, a number or the value of another variable
                {
                    if (value.Contains("\""))
                    {
                        return value.Trim().Trim('\"');
                    }
                    else if (Configuration.usrDef.ContainsKey(value.Trim()))
                    {
                        return Configuration.usrDef[value.Trim()];
                    }
                    else
                    {
                        return value;
                    }

                }
                else
                {
                    if (value.IndexOf('(') != -1)
                    {
                        tmp = value.Substring(value.LastIndexOf("("), value.IndexOf(")") - value.LastIndexOf("(") + 1);
                        return Parse(value.Replace(tmp, Parse(tmp.Trim(brac))));
                    }
                    if (value.IndexOfAny(logicop) != -1)
                    {
                        return ConditionParser.Parse(value).ToString().ToUpper();
                    }
                    else
                    {
                        if (value.IndexOf('+') != -1)
                        {
                            try
                            {
                                return (Convert.ToInt32(Parse(value.Split('+')[0].Trim())) + Convert.ToInt32(Parse(value.Split('+')[1].Trim()))).ToString();
                            }
                            catch
                            {
                                return Parse(value.Split('+')[0].Trim()) + Parse(value.Split('+')[1].Trim());
                            }
                        }

                        if (value.IndexOf('-') != -1)
                        {
                            try
                            {
                                return (Convert.ToInt32(Parse(value.Split('-')[0].Trim())) - Convert.ToInt32(Parse(value.Split('-')[1].Trim()))).ToString();
                            }
                            catch
                            {
                                return Parse(value.Split('-')[0].Trim()) + "-" + Parse(value.Split('-')[1].Trim());
                            }
                        }

                        if (value.IndexOf('*') != -1)
                        {
                            try
                            {
                                return (Convert.ToInt32(Parse(value.Split('*')[0].Trim())) * Convert.ToInt32(Parse(value.Split('*')[1].Trim()))).ToString();
                            }
                            catch
                            {
                                return Parse(value.Split('*')[0].Trim()) + "*" + Parse(value.Split('*')[1].Trim());
                            }
                        }

                        if (value.IndexOf('/') != -1)
                        {
                            try
                            {
                                return (Convert.ToInt32(Parse(value.Split('/')[0].Trim())) / Convert.ToInt32(Parse(value.Split('/')[1].Trim()))).ToString();
                            }
                            catch
                            {
                                return Parse(value.Split('/')[0].Trim()) + "/" + Parse(value.Split('/')[1].Trim());
                            }
                        }

                        if (value.IndexOf('^') != -1)
                        {
                            try
                            {
                                return (Convert.ToInt32(Parse(value.Split('^')[0].Trim())) ^ Convert.ToInt32(Parse(value.Split('^')[1].Trim()))).ToString();
                            }
                            catch
                            {
                                return Parse(value.Split('^')[0].Trim()) + "^" + Parse(value.Split('^')[1].Trim());
                            }
                        }

                    }
                }


                return value;
            }
        }



    }
}
