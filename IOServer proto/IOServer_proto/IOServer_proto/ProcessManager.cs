using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace IOServer_proto
{
    static class ProcessManager
    {
        static List<IOPortedPrc> CurrentProcesses=new List<IOPortedPrc>();

        static public void AddProcess(string name, string enginePath, string arg = "", bool win = false)
        {
            IOPortedPrc prc = new IOPortedPrc(name, enginePath, arg, win);
            prc.Start();
            CurrentProcesses.Add(prc);

        }

        static public void RemoveProcess(IOPortedPrc prc)
        {
            CurrentProcesses.Remove(prc);

        }

        

        static public void KillAll()
        {
            foreach (IOPortedPrc prc in CurrentProcesses)
            {
                prc.End();
                CurrentProcesses.Remove(prc);                
            }
        }



        static public List<IOPortedPrc> GetCurrentProcesses()
        {
            return CurrentProcesses;
        }

        static public void Refresh()
        {
            foreach (IOPortedPrc prc in CurrentProcesses)
            {
                if (prc.Engine.HasExited)
                {
                    CurrentProcesses.Remove(prc);
                }
            }
        }


    }
}
