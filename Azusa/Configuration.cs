using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace Azusa
{
    /* Class name: Configuration Manager
     * 
     * Description:
     * This class loads and imports settings from a text file and store it to allow easy access.
     * It also allows configuration to be changed and exported to a text file.
     * */
    class Configuration
    {
        
        static public int frmHeight = 400;
        static public int frmWidth = 300;
        static public int frmPosX = 0;
        static public int frmPosY = 0;       
        static public bool draggable = false;
        static public bool debugging = false;
        static public bool showicon = true;
        static public string culture = "ja-JP";
        static public int aniRate = 20;

        static public Dictionary<string, string> usrDef = new Dictionary<string, string>();



        static public void Set(string property, string value)
        {
            try
            {
                switch (property)
                {

                    //BEHAVIOR

                    case "frmDraggable":
                        draggable = Convert.ToBoolean(value);
                        break;
                    case "dbgMode":
                        debugging = true;
                        break;
                    case "showIcon":
                        showicon = Convert.ToBoolean(value);
                        break;
                    case "culture":
                        culture = value;
                        break;
                    case "frameInterval":
                        aniRate=Convert.ToInt32(value);
                        break;

                    //APPEARANCE

                    //settings for the main form
                    case "frmHeight":
                        frmHeight = Convert.ToInt32(value);
                        break;
                    case "frmWidth":
                        frmWidth = Convert.ToInt32(value);
                        break;
                    case "frmPosX":
                        frmPosX = Convert.ToInt32(value);
                        break;
                    case "frmPosY":
                        frmPosY = Convert.ToInt32(value);
                        break;
                                  

                    //user-defined parameter
                    default:
                        usrDef.Add(property, value);                        
                        break;
                }
            }
            catch {
                Notifier.ErrorMsg("Unable to set \"" + property + "\" to \"" + value + "\".");
            } 
        }
            
        static public void Import(string filePath)
        {
            try
            {
                int numLine = 1;
                string[] entry;
                foreach (string line in File.ReadAllLines(filePath))
                {
                    try
                    {
                        if (line.Trim() != "" && !line.StartsWith("#"))
                        {
                            entry=line.Trim().Split('=');
                            Set(entry[0].Trim(), entry[1].Trim());
                        }
                    }
                    catch
                    {
                        Notifier.ErrorMsg("Ill-formatted setting in line " + numLine.ToString() + "of " + filePath);
                    }

                    numLine++;
                }
            }
            catch
            {
                Notifier.ErrorMsg("Unable to import settings from " + filePath);
            }
        }
    }
}
