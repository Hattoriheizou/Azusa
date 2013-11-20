using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace AZUSA
{
    public class AZUSAAppContext : ApplicationContext
    {
        NotifyIcon notifyIcon = new NotifyIcon();

        public AZUSAAppContext()
        {           

            notifyIcon.Icon = AZUSA.Properties.Resources.icon;            
            notifyIcon.Visible = true;
        }
    }
}
