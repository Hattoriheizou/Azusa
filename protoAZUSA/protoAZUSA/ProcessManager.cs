using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AZUSA
{
    static class ProcessManager
    {
        static List<IOPortedPrc> CurrentProcesses=new List<IOPortedPrc>();

        static public List<int> AIPid = new List<int>();
        static public List<int> InputPid = new List<int>();
        static public List<int> OutputPid=new List<int>();

        static public bool CheckCompleteness()
        {
            return AIPid.Count != 0 && InputPid.Count != 0 && OutputPid.Count != 0;
        }


        static public void AddProcess(string name, string enginePath, string arg = "")
        {
            IOPortedPrc prc = new IOPortedPrc(name, enginePath, arg);
            prc.Start();
            CurrentProcesses.Add(prc);

        }

        static public void RemoveProcess(IOPortedPrc prc)
        {
            CurrentProcesses.Remove(prc);           

        }

        

        static public void KillAll()
        {
            List<IOPortedPrc> ListCopy = new List<IOPortedPrc>(CurrentProcesses);

            foreach (IOPortedPrc prc in ListCopy)
            {
                prc.End();
                CurrentProcesses.Remove(prc);
            }

            Refresh();         
            

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
