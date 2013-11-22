﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace AZUSA
{
    static class Variables
    {
        static Dictionary<string, string> storage = new Dictionary<string, string>();
        static string[] DateTimeVars = new string[] { "Y", "M", "D", "h", "m", "s", "d" };

        static public void Load(string filePath)
        {
            int numLine = 1;
            string[] entry;
            string ID;
            foreach (string line in File.ReadAllLines(filePath))
            {
                try
                {
                    if (line.Trim() != "" && !line.StartsWith("#"))
                    {
                        entry = line.Trim().Split('=');
                        ID = entry[0];
                        Write(ID.Trim(), line.Replace(ID + "=", "").Trim());
                    }
                }
                catch
                {
                    //Notifier.ErrorMsg("Ill-formatted setting in line " + numLine.ToString() + "of " + filePath);
                }

                numLine++;
            }
        }

        static public void Save(string filePath)
        {

            List<string> updated = new List<string>();
            List<string> newConfig = new List<string>();
            string[] parsed;
            string ID;
            string val;

            //update old values in file
            foreach (string line in File.ReadAllLines(filePath))
            {
                parsed = line.Split('=');

                //see if the line is defining a variable
                if (parsed.Length == 2)
                {
                    ID = parsed[0];
                    val = line.Replace(ID + "=", "");
                    ID = ID.Trim();
                    //see if the variable already exists
                    if (storage.ContainsKey(ID))
                    {
                        newConfig.Add(ID + "=" + storage[ID]);
                    }
                    else //keep it
                    {
                        newConfig.Add(line);
                    }
                    updated.Add(ID);
                }
                else
                {
                    newConfig.Add(line);
                }
            }

            //add new entries that are not in the file
            foreach (KeyValuePair<string, string> pair in storage)
            {
                if (!pair.Key.StartsWith("$") && !updated.Contains(pair.Key))
                {
                    newConfig.Add(pair.Key + "=" + pair.Value);
                }
            }


            File.WriteAllLines(filePath, newConfig.ToArray());

        }

        static public void Write(string name, string val)
        {
            //cannot write to date time variables
            if(DateTimeVars.Contains(name)){
                return;
            }

            if (storage.ContainsKey(name))
            {
                storage[name] = val;
            }
            else
            {
                storage.Add(name, val);
            }
        }

        static public bool Exist(string name)
        {
            //interrupt for date time variables
            if (DateTimeVars.Contains(name)) { return true; }
            return storage.ContainsKey(name);
        }

        static public string Read(string name)
        {
            //interrupt with date time variables
            switch (name)
            {
                case "Y":
                    return DateTime.Now.Year.ToString();

                case "M":
                    return DateTime.Now.Month.ToString();

                case "D":
                    return DateTime.Now.Day.ToString();

                case "h":
                    return DateTime.Now.Hour.ToString();

                case "m":
                    return DateTime.Now.Minute.ToString();

                case "s":
                    return DateTime.Now.Second.ToString();

                case "d":
                    return DateTime.Now.DayOfWeek.ToString();

                default:                    
                    return storage[name];                    
            }


        }
    }
}
