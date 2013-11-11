using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Azusa
{
    /* Class name: Notification Manager
     * 
     * Description: 
     * The class is a channel that allows notifications to be displayed. However, the class is not responsible for deciding when/what 
     * notifcation is to be displayed.
     * */
    class Notifier
    {
        //A global method for throwing error messages
        static public void ErrorMsg(string msg)
        {
            if (Configuration.debugging)
            {
                try
                {
                    Scripting.ScriptEngine.Run("SCRIPT:SYS.error");
                }
                catch { }

                MessageBox.Show(msg);
            }
        }

        //A global method for showing notifications
        static public void ShowMsg(string msg)
        {
            try
            {
                Scripting.ScriptEngine.Run("SCRIPT:SYS.msg");
            }
            catch { }

            MessageBox.Show(msg);
        }
    }
}
