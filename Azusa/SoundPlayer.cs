using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Azusa
{
    /* Class name: Sound Player
     * 
     * Description:
     * This class performs the simple task of loading and playing specified sounds.
     * */

    class SoundPlayer
    {

        static System.Media.SoundPlayer sndPlayer = new System.Media.SoundPlayer();

        static public void Play(string name, bool sync = false)
        {

            sndPlayer.Stop();
  
            sndPlayer.SoundLocation = Environment.CurrentDirectory + @"\Media\wav\" + name + ".wav";
            
            
            if (sync)
            {
                try { sndPlayer.PlaySync(); }
                catch { } // ignore all errors 
            }
            else
            {
                try { sndPlayer.Play(); }
                catch { } // ignore all errors 
            }
        }

    }
}
