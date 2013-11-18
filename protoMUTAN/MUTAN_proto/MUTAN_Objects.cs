using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUTAN_proto
{
    partial class MUTAN
    {
        //The Runnable interface is a representation of "line" in the definition of MUTAN
        //The only important feature is that it can be "run" since it is a valid syntax
        //The detailed implementation of how to "run" a line is defered to more concrete classes
        //Implementations here are just for testing purposes, please do NOT use them directly
        //They should be rewritten when combining to other parts of AZUSA.

        public interface IRunnable
        {
            bool Run();
        }

        //empty object for an empty code
        class empty : IRunnable
        {
            public bool Run()
            {
                return true;
            }
        }

        //The simple decla statement
        class decla : IRunnable
        {
            string ID;
            string expr;

            public decla(string line)
            {
                ID = line.Split('=')[0];
                expr = line.Replace(ID + "=", "");
                ID = ID.Trim();
            }

            public bool Run()
            {
                string val;
                if (ExprParser.TryParse(expr, out val))
                {
                    Variables.Write(ID, val);
                    return true;
                }
                return false;
            }
        }

        //The simple exec statement
        class exec : IRunnable
        {
            string RID;
            string arg;
            public exec(string line)
            {
                RID = line.Split('(')[0];
                arg = line.Replace(RID + "(", "").TrimEnd(')');
                RID = RID.Trim();
            }

            public bool Run()
            {
                string val;
                if (arg == "")
                {
                    return dummyAZUSA.CallRoutine(RID, "");
                }else if (ExprParser.TryParse(arg, out val))
                {                    
                    return dummyAZUSA.CallRoutine(RID, val);
                }
                return false;
            }
        }

        //The multi statement
        class multi : IRunnable
        {
            IRunnable[] basics;
            public multi(string line)
            {
                string[] parts = line.Split(';');
                basics = new IRunnable[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                {
                    if(IsComment(parts[i])){

                    }else if (IsExec(parts[i]))
                    {
                        basics[i] = new exec(parts[i]);
                    }
                    else
                    {
                        basics[i] = new decla(parts[i]);
                    }
                }

            }

            public bool Run()
            {
                
                    foreach (IRunnable basic in basics)
                    {
                            if (!basic.Run())
                            {
                                return false;
                            }
                        
                    }
                
                return true;
            }
        }

        //The cond statement
        class cond : IRunnable
        {
            multi content;
            string condition;
            public cond(string line)
            {
                condition = line.Split('?')[0];
                content = new multi(line.Replace(condition + "?", ""));
            }

            public bool Run()
            {
                string check;
                if (ExprParser.TryParse(condition, out check))
                {
                    if (Convert.ToBoolean(check))
                    {
                        return content.Run();
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //The stmts statement
        class stmts : IRunnable
        {
            IRunnable[] stmt;
            public stmts(string line)
            {
                string[] parts = line.Split(';');
                stmt = new IRunnable[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                {
                    if (IsCond(parts[i]))
                    {
                        stmt[i] = new cond(parts[i]);
                    }
                    else
                    {
                        stmt[i] = new multi(parts[i]);
                    }
                }

            }

            public bool Run()
            {
                foreach (IRunnable obj in stmt)
                {
                    if (!obj.Run())
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        //The loop statement
        class loop : IRunnable
        {
            string content;

            public loop(string line)
            {
                content = line.TrimStart('@');
            }

            public bool Run()
            {                
                return dummyAZUSA.CreateLoop(content);
            }
        }
                

        //The named block
        //This definition is not implemented because during actual runtime
        //the content of the block is parsed and executed
        //The definition here is to keep the parser aware of the fact that this
        //is a valid syntax so it doesn't produce error messages
        class namedblock : IRunnable
        {
            IRunnable[] content;
            string ID;

            public namedblock(string[] lines)
            {
                ID=lines[0].Trim().TrimStart('.').TrimEnd('{');
                content = new IRunnable[lines.Length - 2];
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    if(IsLoop(lines[i])){
                        content[i] = new loop(lines[i]);
                    }else{
                        content[i] = new stmts(lines[i]);
                    }
                }
                //the last line contains only a '}' and can be ignored
            }

            public bool Run()
            {
                //The block is not executed, it can only be called
                //We include this definition to let parser know it is a valid syntax
                //when executed the content of block is passed directly
                
                return true;
            }

        

        }


        //The condition block
        class condblock : IRunnable
        {
            IRunnable[] content;
            string condition;

            public condblock(string[] lines)
            {
                condition = lines[0].Trim().TrimEnd('{');
                content = new IRunnable[lines.Length - 2];
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    if (IsLoop(lines[i]))
                    {
                        content[i-1] = new loop(lines[i]);
                    }
                    else
                    {
                        content[i-1] = new stmts(lines[i]);
                    }
                }
                //the last line contains only a '}' and can be ignored
            }

            public bool Run()
            {
                string check;
                if (ExprParser.TryParse(condition, out check))
                {
                    if (Convert.ToBoolean(check))
                    {
                        foreach (IRunnable line in content)
                        {
                            if (!line.Run())
                            {
                                return false;
                            }
                            return true;
                        }
                    }
                }
                return false;
            }



        }

        //The loop block
        class loopblock : IRunnable
        {
            string[] content;

            public loopblock(string[] lines)
            {
                content = new string[lines.Length - 2];
                //the first line is just "@{" and can be ignored
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    content[i-1] = lines[i];
                }
                //the last line contains only a '}' and can be ignored
            }

            public bool Run()
            {                
                return dummyAZUSA.CreateLoop(content);
            }
        }

        //The block, mix of lines and simple blocks
        class block : IRunnable
        {
            List<IRunnable> objects=new List<IRunnable>();

            public block(string[] lines)
            {
                int bracketcount = 0;
                bool inblock = false;
                List<string> content = new List<string>();

                foreach (string line in lines)
                {
                    //see if it is beginning of a block
                    if (line.Trim().EndsWith("{"))
                    {
                        bracketcount++;
                        inblock = true;
                        content.Add(line.Trim());
                        continue;
                    }

                    if (line.Trim() == "}")
                    {
                        bracketcount--;
                        content.Add("}");

                        if (bracketcount == 0)
                        {
                            inblock = false;
                            //check the block
                            if (IsLoopBlock(content.ToArray()))
                            {
                                objects.Add(new loopblock(content.ToArray()));
                            }else if(IsCondBlock(content.ToArray()))
                            {
                                objects.Add(new condblock(content.ToArray()));
                            }else{
                                objects.Add(new namedblock(content.ToArray()));
                            }

                            content.Clear();
                        }
                        continue;
                    }

                    if (inblock)
                    {
                        content.Add(line.Trim());
                        continue;
                    }

                    if (IsLine(line))
                    {
                        if (IsLoop(line))
                        {
                            objects.Add(new loop(line));
                        }
                        else
                        {
                            objects.Add(new stmts(line));
                        }
                    }
                }   
            }

            public bool Run()
            {
                foreach (IRunnable obj in objects)
                {
                    if (!obj.Run())
                    {
                        return false;
                    }
                }
                return true;
            }
        }


    }
}
