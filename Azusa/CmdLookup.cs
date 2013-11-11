using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Azusa
{
    /* Class name: Command Look-up
     * 
     * Description:
     * This class acts as a database of text and voice commands, and their corresponding executables.
     * In other words it is a dictionary which allows the program to look up what the user just inputed and execute
     * the corresponding command.
     * 
     * Entries are imported from a text file.
     * 
     * It also allows modification and addition of entries, and possibly exporting the new database to a text file.
     * */

    class CmdLookup
    {       
        //list used to store all the command
        static List<Command> CmdList= new List<Command>();

        // a simple structure used to store a single command
        struct Command  
        {
            public string Name;  //name of command            
            public string Exec;    //content to be executed
        }

        //constructor
        public CmdLookup()
        {
        }

        //add a command to the list
        static public void Add(string name, string vname, string exec)
        {
            try
            {
                Command newcmd;
                newcmd.Name = name;                
                newcmd.Exec = exec;

                if (GetExec(name) != null)
                {
                    CmdList.Add(newcmd);
                }
                else //the command already exist, treat as an update
                {
                    //first locate the existing command
                    Command result = CmdList.Find(x => x.Name == name);

                    //update the entry
                    Update(result.Name, result.Exec, name, exec);
                }
            }
            catch
            {
                Notifier.ErrorMsg("Error when trying to add the command \"" + name + "\"");
            }
        }

        //remove a command from the list
        static public void Remove(string name)
        {
        }

        //modify a command
        static public void Update(string name,string exec, string newname, string newexec)
        {
            try
            {
                Command target;
                target.Name = name;                
                target.Exec = exec;

                Command newcmd;
                newcmd.Name = newname;                
                newcmd.Exec = newexec;

                int index = CmdList.IndexOf(target);

                CmdList[index] = newcmd;
            }
            catch
            {
                Notifier.ErrorMsg("Error when trying to update the command \"" + name + "\"");
            }
        }

        //get the exec for a give command, the name may come from text input or voice input
        static public string GetExec(string name)
        {
            try
            {
                Command result = CmdList.Find(x => x.Name == name);
                return result.Exec;
            }
            catch
            {
                Notifier.ErrorMsg("Error when trying to look for the command \"" + name + "\"");
                return null;
            }
        }

        //import command list from a text file
        static public void Import(string filePath)
        {
            string[] db={};

            try
            {
                db = File.ReadAllLines(filePath);
            }
            catch
            {
                Notifier.ErrorMsg("Unable to read the command list located at "+filePath+". It might be corrupted.");
                return;
            }

            string[] parsed;
            int lineNum = 1;

            foreach (string line in db)
            {
                try
                {
                    //This allows some formatting and commenting in the text file
                    if (line.Trim() != "" && !line.StartsWith("#"))
                    {
                        parsed = line.Split(',');

                        Command entry;
                        entry.Name = parsed[0];
                        entry.Exec = parsed[1];

                        CmdList.Add(entry);
                    }
                }
                catch
                {
                    Notifier.ErrorMsg("Ill-formatted command entry in command list at line " + lineNum.ToString());
                    return;
                }

                lineNum++;
            }
        }

        //emport command list to a text file
        static public void Emport(string filePath)
        {
            StreamWriter sw;
            try
            {
                sw = new StreamWriter(filePath,false); 
            }
            catch
            {
                Notifier.ErrorMsg("Unable to access "+filePath+".");
                return;
            }

            
            foreach (Command cmd in CmdList)
            {
                try
                {
                    sw.WriteLine(cmd.Name + ","  + cmd.Exec);
                }
                catch
                {
                    Notifier.ErrorMsg("Unable to write the command \"" + cmd.Name + "\".\n" + cmd.Exec);
                    return;
                }
                
            }

            try
            {
                sw.Flush();
            }
            catch
            {
                Notifier.ErrorMsg("Unable to flush command table to " + filePath + ".");
            }

            sw.Close();
        }
        
    }
}
