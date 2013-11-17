using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUTAN_proto
{
    partial class MUTAN
    {
        static public bool IsDecla(string line)
        {

            //first there has to be an equal sign
            if (line.Contains('='))
            {
                string tmp;
                string[] split = line.Split('=');

                //second the left hand side must be a simple string that is not further evaluable ,ie an expression cannot be used as an ID
                // or it can be an existing ID
                if (ExprParser.TryParse(split[0], out tmp) && tmp == split[0].Trim() || Variables.Exist(split[0].Trim()))
                {
                    //lastly the right hand side is a valid expression
                    if (ExprParser.TryParse(line.Replace(split[0] + "=", ""), out tmp))
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        static public bool IsExec(string line)
        {

            //first there has to be an open bracket and ends with closed bracket
            if (line.Contains('(') && line.EndsWith(")"))
            {
                string tmp;
                string[] split = line.Split('(');
                //second the left hand side must be a simple string that is not further evaluable, ie an expression cannot be used as a RID
                if (ExprParser.TryParse(split[0], out tmp) && tmp == split[0].Trim())
                {
                    //lastly the content inside is a valid expression, OR an emtpy string (emtpy string is a valid expression)
                    if (line.Replace(split[0] + "(", "").TrimEnd(')').Trim() == "")
                    {
                        return true;
                    }
                    else if (ExprParser.TryParse(line.Replace(split[0] + "(", "").TrimEnd(')'), out tmp))
                    {
                        return true;
                    }

                }
            }
            


            return false;
        }

        static public bool IsBasic(string line)
        {
            return IsDecla(line) || IsExec(line);
        }

        static public bool IsMulti(string line)
        {
            //split each part with ';', each part should be a basic (decla or exec)
            foreach (string part in line.Split(';'))
            {
                if (!IsBasic(part))
                {
                    return false;
                }
            }

            return true;
        }

        static public bool IsCond(string line)
        {

            //first there has to be a question mark
            if (line.Contains('?'))
            {
                string tmp;
                string[] split = line.Split('?');

                //second the left hand side has to be a valid expression
                if (ExprParser.TryParse(split[0], out tmp))
                {
                    //lastly the right hand side has to be a multi
                    if (IsMulti(line.Replace(split[0] + "?", "")))
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        static public bool IsStmt(string line)
        {
            return IsCond(line) || IsMulti(line);  //what is basic is also a multi
        }

        static public bool IsStmts(string line)
        {
            //split each part with ';', each part should be a stmt
            foreach (string part in line.Split(';'))
            {
                if (!IsStmt(part))
                {
                    return false;
                }
            }

            return true;
        }

        static public bool IsLoop(string line)
        {
            //first the line has to start with '@'
            if (line.StartsWith("@"))
            {
                //the rest of the line has to be a stmts
                if (IsStmts(line.TrimStart('@')))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
