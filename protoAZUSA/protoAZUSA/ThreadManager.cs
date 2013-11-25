﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace AZUSA
{
    //線程管理員
    static class ThreadManager
    {
        //現在運行中的線程
        static List<LoopThread> CurrentLoops=new List<LoopThread>();

        //增加線程
        static public void AddLoop(string[] code)
        {
            LoopThread loop = new LoopThread(code);
            CurrentLoops.Add(loop);

        }
        //移除線程
        static public void RemoveLoop(LoopThread loop)
        {
            CurrentLoops.Remove(loop);

        }
                
        //中斷現在運行中的所有的線程
        static public void BreakAll()
        {
            List<LoopThread> ListCopy = new List<LoopThread>(CurrentLoops);

            foreach (LoopThread loop in ListCopy)
            {
                loop.Break();
            }

            Refresh();
            
        }


        //取得當前線程
        static public List<LoopThread> GetCurrentLoops()
        {
            return CurrentLoops;
        }

        //刷新登錄, 清除掉已退出的線程
        static public void Refresh()
        {
            foreach (LoopThread loop in CurrentLoops)
            {
                if (!loop.IsAlive())
                {
                    CurrentLoops.Remove(loop);
                }
            }
        }

    }
}
