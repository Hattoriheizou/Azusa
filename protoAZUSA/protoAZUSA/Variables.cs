using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AZUSA
{    
    static class Variables
    {
        static Dictionary<string,string> storage=new Dictionary<string,string>();

        static public void Load(string filePath)
        {

        }

        static public void Save(string filePath)
        {

        }

        static public void Write(string name,string val){        
            if(storage.ContainsKey(name)){
                storage[name]=val;
            }else{
                storage.Add(name, val);
            }
            MessageBox.Show("The variable with ID \"" + name + "\" has been set to " + val);
        }
        static public bool Exist(string name){

            return storage.ContainsKey(name);
        }

        static public string Read(string name)
        {
            return storage[name];
        }
    }
}
