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

        public struct ReturnCode
        {
            public string Command, Argument;

            public ReturnCode(string Cmd, string Arg)
            {
                Command = Cmd;
                Argument = Arg;
            }
        }

        public interface IRunnable
        {
            ReturnCode[] Run();
        }

        //empty object for an empty code
        class empty : IRunnable
        {
            public ReturnCode[] Run()
            {
                return new ReturnCode[] { new ReturnCode("", "") };
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

            public ReturnCode[] Run()
            {
                string val;
                if (ExprParser.TryParse(expr, out val))
                {
                    return new ReturnCode[] { new ReturnCode("VAL", ID + "=" + val) };
                }
                return new ReturnCode[] { new ReturnCode("ERR", expr + "IS NOT A VALID EXPRESSION.") };
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
                arg = line.Substring(RID.Length + 1, line.Length - RID.Length - 2);
                RID = RID.Trim();
            }

            public ReturnCode[] Run()
            {
                string val;
                if (arg == "")
                {
                    return new ReturnCode[] { new ReturnCode(RID, "") };
                }
                else if (ExprParser.TryParse(arg, out val))
                {
                    return new ReturnCode[] { new ReturnCode(RID, val) };
                }
                return new ReturnCode[] { new ReturnCode("ERR", arg + "IS NOT A VALID EXPRESSION.") };
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
                    if (IsComment(parts[i]))
                    {
                        basics[i] = new empty();
                    }
                    else if (IsExec(parts[i]))
                    {
                        basics[i] = new exec(parts[i]);
                    }
                    else
                    {
                        basics[i] = new decla(parts[i]);
                    }
                }

            }

            public ReturnCode[] Run()
            {
                List<ReturnCode> returns = new List<ReturnCode>();
                ReturnCode[] tmp;
                foreach (IRunnable basic in basics)
                {
                    tmp = basic.Run();
                    foreach (ReturnCode code in tmp)
                    {
                        returns.Add(code);
                    }

                }

                return returns.ToArray();
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

            public ReturnCode[] Run()
            {
                string check;
                if (ExprParser.TryParse(condition, out check))
                {
                    if (Convert.ToBoolean(check))
                    {
                        return content.Run();
                    }
                    return new ReturnCode[] { new ReturnCode("", "") };
                }
                else
                {
                    return new ReturnCode[] { new ReturnCode("ERR", condition + "IS NOT A VALID EXPRESSION.") };
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

            public ReturnCode[] Run()
            {

                List<ReturnCode> returns = new List<ReturnCode>();
                ReturnCode[] tmp;
                foreach (IRunnable obj in stmt)
                {
                    tmp = obj.Run();
                    foreach (ReturnCode code in tmp)
                    {
                        returns.Add(code);
                    }

                }

                return returns.ToArray();
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

            public ReturnCode[] Run()
            {
                return new ReturnCode[] { new ReturnCode("LOOP", content) };
            }
        }


        //The named block
        //This definition is not implemented because during actual runtime
        //the content of the block is parsed and executed
        //The definition here is to keep the parser aware of the fact that this
        //is a valid syntax so it doesn't produce error messages
        class namedblock : IRunnable
        {
            IRunnable[] objects;
            string ID;

            public namedblock(string[] lines)
            {
                ID = lines[0].Trim().TrimStart('.').TrimEnd('{');
                string[] content = new string[lines.Length - 2];
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    content[i - 1] = lines[i];
                }
                objects = ParseBlock(content);
                //the last line contains only a '}' and can be ignored
            }

            public ReturnCode[] Run()
            {
                //The block is not executed, it can only be called
                //We include this definition to let parser know it is a valid syntax
                //when executed the content of block is passed directly

                return new ReturnCode[] { new ReturnCode("", "") };
            }



        }

        //The condition block
        class condblock : IRunnable
        {
            IRunnable[] objects;
            string condition;

            public condblock(string[] lines)
            {
                condition = lines[0].Trim().TrimEnd('{');
                string[] content = new string[lines.Length - 2];
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    content[i - 1] = lines[i];
                }
                objects = ParseBlock(content);
                //the last line contains only a '}' and can be ignored
            }

            public ReturnCode[] Run()
            {
                string check;
                if (ExprParser.TryParse(condition, out check))
                {
                    if (Convert.ToBoolean(check))
                    {
                        List<ReturnCode> returns = new List<ReturnCode>();
                        ReturnCode[] tmp;
                        foreach (IRunnable obj in objects)
                        {
                            tmp = obj.Run();
                            foreach (ReturnCode code in tmp)
                            {
                                returns.Add(code);
                            }

                        }

                        return returns.ToArray();
                    }
                    return new ReturnCode[] { new ReturnCode("", "") };
                }
                return new ReturnCode[] { new ReturnCode("ERR", condition + " IS NOT A VALID EXPRESSION.") };
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
                    content[i - 1] = lines[i];
                }
                //the last line contains only a '}' and can be ignored
            }

            public ReturnCode[] Run()
            {
                string msg = "";
                foreach (string line in content)
                {
                    msg += line + "\n";
                }
                return new ReturnCode[] { new ReturnCode("MLP", msg) };
            }
        }

        //The block, mix of lines and simple blocks
        class block : IRunnable
        {
            IRunnable[] objects;

            public block(string[] lines)
            {
                objects = ParseBlock(lines);
            }

            public ReturnCode[] Run()
            {
                List<ReturnCode> returns = new List<ReturnCode>();
                ReturnCode[] tmp;
                foreach (IRunnable obj in objects)
                {
                    tmp = obj.Run();
                    foreach (ReturnCode code in tmp)
                    {
                        returns.Add(code);
                    }

                }

                return returns.ToArray();
            }

        }


    }
}
