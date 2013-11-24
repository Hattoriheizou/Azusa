using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AZUSA
{
    class LoopThread
    {

            string[] codes;
            Thread thread;
            MUTAN.IRunnable obj;
            bool BREAKING=false;

            public LoopThread(string[] content)
            {
                codes=content;
                thread = new Thread(new ThreadStart(this.RunScript));
                thread.Start();
            }

            public void Break(){
                BREAKING = true;
                thread.Abort();
                thread = null;
            }

            ~LoopThread()
            {
                codes = null;
                obj = null;

                BREAKING = true;
                try
                {
                    thread.Abort();
                    thread = null;
                }
                catch { }
            }

            void RunScript()
            {
                MUTAN.Parser.TryParse(codes, out obj);
                while (!BREAKING)
                {
                    foreach (MUTAN.ReturnCode code in obj.Run())
                    {
                        if (code.Command.Trim() == "BREAK")
                        {
                            BREAKING = true;
                            break;                            
                        }
                        else
                        {
                            Internals.Execute(code.Command, code.Argument);
                        }
                    }
                }

                Break();
            }
    }
}
