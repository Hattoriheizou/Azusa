using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Azusa
{
    /* Class name: Form Controller
     * 
     * Description:
     * This class stores the reference to a form and contains function calls
     * that changes the position and/or opacity the form to create form special effects.
     * 
     * 
     * */


    class FormController
    {
        Form targetForm;

        int X;
        int Y;

        private delegate void CrossThreadCallBack();
        private delegate void CrossThreadCallBackStr(string var);
        private delegate void CrossThreadCallBackInt(int var);
        private delegate void CrossThreadCallBackDbl(double var);

        public FormController(Form form)
        {
            targetForm = form;
            X = targetForm.Location.X;
            Y = targetForm.Location.Y;
        }


        public void Activate()
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBack(Activate));
            }
            else
            {

                targetForm.Activate();
            }
        }

        public void PosX(int newX)
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBackInt(PosX), newX);
            }
            else
            {
                X = newX;
                targetForm.Location = new Point(X, Y);
            }
        }

        public void PosY(int newY)
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBackInt(PosY), newY);
            }
            else
            {
                Y = newY;
                targetForm.Location = new Point(X, Y);
            }
        }

        public void HideForm()
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBack(HideForm));
            }
            else
            {
                targetForm.Hide();
                StatusMonitor.CurrentStatus = "HIDDEN";
            }
        }

        public void ShowForm()
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBack(ShowForm));
            }
            else
            {
                targetForm.Show();

            }
        }

        public void CloseForm()
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBack(CloseForm));
            }
            else
            {
                StatusMonitor.CurrentStatus = "CLOSING";
                targetForm.Close();
            }
        }




        //For drawing image
        public void PutImg(string filepath)
        {
            if (targetForm.InvokeRequired)
            {
                targetForm.Invoke(new CrossThreadCallBackStr(PutImg), filepath);
            }
            else
            {
                if (!MediaCache.preloadedImg.ContainsKey(filepath))
                {
                    if (File.Exists(Environment.CurrentDirectory + @"\Media\img\" + filepath))
                    {

                        MediaCache.preloadedImg.Add(filepath, Image.FromFile(Environment.CurrentDirectory + @"\Media\img\" + filepath));

                    }
                }
                if (!StatusMonitor.dragging)
                {
                    targetForm.Location = new Point(Configuration.frmPosX - MediaCache.preloadedImg[filepath].Width, Configuration.frmPosY  - MediaCache.preloadedImg[filepath].Height);
                }

                APIDraw.UpdateFormDisplay(MediaCache.preloadedImg[filepath], targetForm);
                Configuration.frmHeight = MediaCache.preloadedImg[filepath].Height;
                Configuration.frmWidth = MediaCache.preloadedImg[filepath].Width;
               
            }
        }


    }
}   
