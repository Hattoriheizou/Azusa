using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUTAN_proto
{
    partial class MUTAN{
    //Used to parse expressions
    public class ExprParser
    {
        //Try to evaluate the expression, will output error message if failed
        static public bool TryParse(string _expr, out string result)
        {
            //Remove unnecessary spaces
            string expr = _expr.Trim();

            //No empty expression or empty quotation allowed 
            if (expr == "" || expr.Trim('"')=="")
            {
                result = "INVALIDEXPR";
                return false;
            }


            //Store intermediate results
            string imd;
            string imd2;
            int tmp;

            try
            {
                //If it is just a variable, reply with the corresponding value of the variable
                if (Variables.Exist(expr))
                {
                    result = Variables.Read(expr);
                    return true;
                }


                //Return directly the value of TRUE, FALSE or integers
                if (expr.Trim().ToUpper() == "TRUE" || expr.Trim().ToUpper() == "FALSE" || Int32.TryParse(expr, out tmp))
                {
                    if (expr.Trim().TrimStart('0') != "") //if it is not purely consists of zeros than we can trim off zeros
                    { 
                        result = expr.Trim().TrimStart('0');
                    }
                    else // otherwise just return 0
                    {
                        result = "0";
                    }
                    return true;
                }

                #region Parenthesis accounting
                //Start with deepest paranthesis if any
                if (expr.Contains('('))
                {
                    int bracketCount = 0;

                    string content = "";
                    bool record = false;

                    for (int i = 0; i < expr.Length; i++)
                    {
                        if (expr[i] == '(')
                        {
                            bracketCount++;
                            content = "";
                            record = true;
                        }
                        else if (expr[i] == ')')
                        {
                            bracketCount--;
                            record = false;
                        }
                        else if (record)
                        {
                            content = content + expr[i].ToString();
                        }
                    }

                    if (bracketCount == 0)  //brackets are balanced
                    {
                        if (TryParse(content, out imd))
                        {
                            return TryParse(expr.Replace("(" + content + ")", imd), out result);
                        }
                    }
                    else
                    {
                        result = "IMBALBRACKET";
                        return false;
                    }
                }
                #endregion


                //At this stage we should be left with rather innocent expressions
                //However, we still need to go through quotation marks to check which operators are in fact part
                //of a string and should not be split

                List<int> InvalidOp = new List<int>();  //stores the index of operators that should be ignored
                bool inStr = false;

                #region Quotation accounting
                if (expr.Contains("\""))
                {
                    for (int i = 0; i < expr.Length; i++)
                    {
                        if (expr[i] == '"')
                        {
                            inStr = !inStr;
                        }
                        else if (inStr)
                        {
                            InvalidOp.Add(i);
                        }
                    }
                }

                #endregion

                #region Logical operators spliting

                if (expr.Contains("&") && !InvalidOp.Contains(expr.IndexOf("&")))
                {
                    if (TryParse(expr.Split('&')[0], out imd) && TryParse(expr.Replace(expr.Split('&')[0] + "&", ""), out imd2))
                    {
                        result = Convert.ToString(Convert.ToBoolean(imd) && Convert.ToBoolean(imd2));
                        return true;
                    }
                }
                if (expr.Contains("|") && !InvalidOp.Contains(expr.IndexOf("|")))
                {
                    if (TryParse(expr.Split('|')[0], out imd) && TryParse(expr.Replace(expr.Split('|')[0] + "|", ""), out imd2))
                    {
                        result = Convert.ToString(Convert.ToBoolean(imd) || Convert.ToBoolean(imd2));
                        return true;
                    }
                }
                if (expr.Contains("=") && !InvalidOp.Contains(expr.IndexOf("=")))
                {
                    //here we insert three branches to check for !=, >= and <=
                    if (expr.Split('=')[0].EndsWith("!"))   // !=
                    {
                        if (TryParse(expr.Split('=')[0].TrimEnd('!'), out imd) && TryParse(expr.Replace(expr.Split('=')[0] + "=", ""), out imd2))
                        {
                            result = Convert.ToString(imd != imd2);
                            return true;
                        }
                    }

                    if (expr.Split('=')[0].EndsWith(">")) // >=
                    {
                        if (TryParse(expr.Split('=')[0].TrimEnd('>'), out imd) && TryParse(expr.Replace(expr.Split('=')[0] + "=", ""), out imd2))
                        {
                            result = Convert.ToString(Convert.ToInt32(imd) >= Convert.ToInt32(imd2));
                            return true;
                        }
                    }

                    if (expr.Split('=')[0].EndsWith("<")) // <=
                    {
                        if (TryParse(expr.Split('=')[0].TrimEnd('<'), out imd) && TryParse(expr.Replace(expr.Split('=')[0] + "=", ""), out imd2))
                        {
                            result = Convert.ToString(Convert.ToInt32(imd) <= Convert.ToInt32(imd2));
                            return true;
                        }
                    }


                    //nothing is preceding '=' so it is just a normal equality check
                    if (TryParse(expr.Split('=')[0], out imd) && TryParse(expr.Replace(expr.Split('=')[0] + "=", ""), out imd2))
                    {
                        result = Convert.ToString(imd == imd2);
                        return true;
                    }
                }
                if (expr.Contains(">") && !InvalidOp.Contains(expr.IndexOf(">")))
                {
                    if (TryParse(expr.Split('>')[0], out imd) && TryParse(expr.Replace(expr.Split('>')[0] + ">", ""), out imd2))
                    {
                        result = Convert.ToString(Convert.ToInt32(imd) > Convert.ToInt32(imd2));
                        return true;
                    }
                }
                if (expr.Contains("<") && !InvalidOp.Contains(expr.IndexOf("<")))
                {
                    if (TryParse(expr.Split('<')[0], out imd) && TryParse(expr.Replace(expr.Split('<')[0] + "<", ""), out imd2))
                    {
                        result = Convert.ToString(Convert.ToInt32(imd) < Convert.ToInt32(imd2));
                        return true;
                    }
                }
                #endregion

                #region String concatenation and arithmetic operators spliting
                if (expr.Contains("+") && !InvalidOp.Contains(expr.IndexOf("+")))
                {
                    if (TryParse(expr.Split('+')[0], out imd) && TryParse(expr.Replace(expr.Split('+')[0] + "+", ""), out imd2))
                    {
                        try
                        {
                            result = Convert.ToString(Convert.ToInt32(imd) + Convert.ToInt32(imd2));
                        }
                        catch
                        {
                            result = imd + imd2;
                        }
                        return true;
                    }
                }

                if (expr.Contains("-") && !InvalidOp.Contains(expr.IndexOf("-")))
                {
                    if (TryParse(expr.Split('-')[0], out imd) && TryParse(expr.Replace(expr.Split('-')[0] + "-", ""), out imd2))
                    {
                        result = Convert.ToString(Convert.ToInt32(imd) - Convert.ToInt32(imd2));
                        return true;
                    }
                }

                if (expr.Contains("*") && !InvalidOp.Contains(expr.IndexOf("*")))
                {
                    if (TryParse(expr.Split('*')[0], out imd) && TryParse(expr.Replace(expr.Split('*')[0] + "*", ""), out imd2))
                    {
                        result = Convert.ToString(Convert.ToInt32(imd) * Convert.ToInt32(imd2));
                        return true;
                    }
                }

                if (expr.Contains("/") && !InvalidOp.Contains(expr.IndexOf("/")))
                {
                    if (TryParse(expr.Split('/')[0], out imd) && TryParse(expr.Replace(expr.Split('/')[0] + "/", ""), out imd2))
                    {
                        result = Convert.ToString(Math.Round((double)Convert.ToInt32(imd) / Convert.ToInt32(imd2), 0));
                        return true;
                    }
                }


                #endregion


                //Negation spliting
                if (expr.StartsWith("~"))
                {
                    if (TryParse(expr.Trim('~'), out imd))
                    {
                        result = Convert.ToString(!Convert.ToBoolean(imd));
                        return true;
                    }
                }

                //If all things fail, treat as a simple string 
                //For properly quoted string, quotation marks are removed
                if (expr.StartsWith("\"") && expr.EndsWith("\""))
                {
                    result = expr.Trim('"');
                    return true;
                }
                else
                {
                    result = expr;
                    return true;
                }

            }
            catch
            {

                result = "INVALIDEXPR";
                return false;

            }

        }
    }

    //Used to determine type of syntax of a single line
    public class LineParser
    {
        public bool TryParse(string line, out IRunnable obj)
        {
            if (line.Trim() == "")
            {
                obj = new empty();
                return true;
            }

            //Parsing should begin from large scale structure to small scale structure
            //in order to ensure correct priority

            if (IsLoop(line))
            {
                obj = new loop(line);
                return true;
            }

            if (IsStmts(line))
            {
                obj = new stmts(line);
                return true;
            }


            obj = null;
            return false;

        }

    }

    //Used to parse a verified block
    static public IRunnable[] ParseBlock(string[] lines)
    {
        List<IRunnable> objects=new List<IRunnable>();
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
                    if(IsLoopBlock(content.ToArray())){
                        objects.Add(new loopblock(content.ToArray()));
                    }
                    else if (IsCondBlock(content.ToArray()))
                    {
                        objects.Add(new condblock(content.ToArray()));
                    }
                    else if (IsNamedBlock(content.ToArray()))
                    {
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

            if (IsLine(line.Trim()))
            {
                if (IsLoop(line.Trim()))
                {
                    objects.Add(new loop(line.Trim()));
                }
                else
                {
                    objects.Add(new stmts(line.Trim()));
                }
            }
        }

        return objects.ToArray();
    }

    //Used to parse a program
    public class Parser
    {
        public bool TryParse(string[] lines, out IRunnable obj)
        {

            if (IsBlock(lines))
            {
                obj=new block(lines);
                return true;
            }           


            obj = null;
            return false;
        }

    }


    }

}
