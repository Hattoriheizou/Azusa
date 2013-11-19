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
                    ////lastly the right hand side is a valid expression
                    //if (ExprParser.TryParse(line.Replace(split[0] + "=", ""), out tmp))
                    //{
                    //    return true;
                    //}
                    return true;
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
                    ////lastly the content inside is a valid expression, OR an emtpy string (emtpy string is a valid expression)
                    //if (line.Substring(split[0].Length + 1, line.Length - split[0].Length -2).Trim() == "")
                    //{
                    //    return true;
                    //}
                    //else if (ExprParser.TryParse(line.Substring(split[0].Length+1, line.Length - split[0].Length - 2), out tmp))
                    //{
                    //    return true;
                    //}

                    return true;

                }
            }



            return false;
        }

        static public bool IsComment(string line)
        {
            return line.Trim() == "" || line.StartsWith("#");
        }

        static public bool IsBasic(string line)
        {
            return IsDecla(line) || IsExec(line) || IsComment(line);
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

                ////second the left hand side has to be a valid expression
                //if (ExprParser.TryParse(split[0], out tmp))
                //{
                    //lastly the right hand side has to be a multi
                    if (IsMulti(line.Replace(split[0] + "?", "")))
                    {
                        return true;
                    }
                //}
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

        static public bool IsLine(string line)
        {
            return IsLoop(line) || IsStmts(line);
        }

        static public bool IsNamedBlock(string[] lines)
        {
            //first the first line has to start with '.', ends with '{'
            //the last line should be "}"
            if (lines[0].Trim().StartsWith(".") && lines[0].Trim().EndsWith("{") && lines[lines.Length - 1].Trim() == "}")
            {
                string ID = lines[0].Trim().Trim('.', '{');
                string tmp;
                //second the ID should not be further evaluable, and also should not start/end with spaces
                if (ExprParser.TryParse(ID, out tmp) && tmp == ID)
                {
                    //lastly the content should be a block
                    string[] content = new string[lines.Length - 2];
                    for (int i = 1; i < lines.Length - 1; i++)
                    {
                        content[i - 1] = lines[i];
                    }
                    return IsBlock(content);
                }
            }
            return false;
        }

        static public bool IsCondBlock(string[] lines)
        {
            //first the first line has to ends with '{'
            //the last line should be "}"
            if (lines[0].Trim().EndsWith("{") && lines[lines.Length - 1].Trim() == "}")
            {
                string cond = lines[0].Trim().Trim('{');
                string tmp;
                bool chk;
                //second the cond should be a valid expression and should be boolean
                if (ExprParser.TryParse(cond, out tmp))
                {
                    if (Boolean.TryParse(tmp, out chk))
                    {
                        //lastly the content should be a block
                        string[] content = new string[lines.Length - 2];
                        for (int i = 1; i < lines.Length - 1; i++)
                        {
                            content[i - 1] = lines[i];
                        }
                        return IsBlock(content);
                    }
                }
            }
            return false;
        }

        static public bool IsLoopBlock(string[] lines)
        {
            //first the first line has to be '@{'
            //the last line should be "}"
            if (lines[0].Trim() == "@{" && lines[lines.Length - 1].Trim() == "}")
            {
                //lastly the content should be a block
                string[] content = new string[lines.Length - 2];
                for (int i = 1; i < lines.Length - 1; i++)
                {
                    content[i - 1] = lines[i];
                }
                return IsBlock(content);

            }
            return false;
        }

        
        static public bool IsBlock(string[] lines)
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
                        if (!IsNamedBlock(content.ToArray()) && !IsCondBlock(content.ToArray()) && !IsLoopBlock(content.ToArray()))
                        {
                            return false;
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

                if (!IsLine(line.Trim()))
                {
                    return false;
                }
            }

            if (bracketcount != 0)
            {
                return false;
            }

            return true;
        }
    }
}
