using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Azusa
{
    class StatusMonitor
    {
        static public string CurrentStatus = "NORMAL";
        static public bool dragging = false;

        static public bool waiting = false; //for WAIT
        static public List<string> pausedScr = new List<string>();// for WAIT
        static public int waittime = 0;

        static public Stack<string> LockedStatus = new Stack<string>();
        static public List<string> currentAniFrames = new List<string>();
        static public int currentFrame = 0;
        static public bool EXITFLAG = false;
    }
}
