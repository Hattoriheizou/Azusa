using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AZUSA
{
    static class ThreadManager
    {
        static List<LoopThread> CurrentLoops=new List<LoopThread>();


        static public void AddLoop(string[] code)
        {
            LoopThread loop = new LoopThread(code);
            CurrentLoops.Add(loop);

        }

        static public void RemoveLoop(LoopThread loop)
        {
            CurrentLoops.Remove(loop);

        }

        

        static public void BreakAll()
        {
            List<LoopThread> ListCopy = new List<LoopThread>(CurrentLoops);

            foreach (LoopThread loop in ListCopy)
            {
                loop.Break();
                RemoveLoop(loop);
            }
        }



        static public List<LoopThread> GetCurrentLoops()
        {
            return CurrentLoops;
        }



    }
}
