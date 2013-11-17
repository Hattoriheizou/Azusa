using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUTAN_proto
{

    //Used to parse expressions
    static class ExprParser
    {



        //Try to evaluate the expression, will output error message if failed
        static public bool TryParse(string _expr, out string result)
        {
            //Remove unnecessary spaces
            string expr = _expr.Trim();

            //No empty expression allowed
            if (expr == "")
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
                    result = expr.Trim().TrimStart('0');    //remove starting zeros for integer
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

    //Used to determine type of syntax
    class Classifier
    {
        

        public bool TryClassify(string line, out IRunnable obj)
        {

            if (IsDecla(line))
            {
                obj = new decla(line);
                return true;
            }

            if (IsExec(line))
            {
                obj = new exec(line);
                return true;
            }

            obj = null;
            return false;

        }


        #region Simple check functions for different definitions
        bool IsDecla(string line)
        {
            string tmp;

            //first there has to be an equal sign
            if (line.Contains('='))
            {
                string[] split = line.Split('=');

                //second the left hand side must be a simple string that is not further evaluable ,ie an expression cannot be used as an ID 
                if (ExprParser.TryParse(split[0], out tmp) && tmp == split[0].Trim())
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

        bool IsExec(string line)
        {
            string tmp;

            //first there has to be a colon
            if (line.Contains(':'))
            {
                string[] split = line.Split(':');
                //second the left hand side must be a simple string that is not further evaluable, ie an expression cannot be used as a RID
                if (ExprParser.TryParse(split[0], out tmp) && tmp == split[0].Trim())
                {
                    //lastly the right hand side is a valid expression
                    if (ExprParser.TryParse(line.Replace(split[0] + ":", ""), out tmp))
                    {
                        return true;
                    }

                }
            }


            return false;
        }

        bool IsDecExe(string line)
        {
            return IsDecla(line) || IsExec(line);
        }


        #endregion



    }

}
