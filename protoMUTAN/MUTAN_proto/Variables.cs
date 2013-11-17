using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MUTAN_proto
{

    //This is a temporary class used to simulate the response from AZUSA's data manager
    //This is NOT a part of this side project. Please replace with function calls to the actual data manager.
    static class Variables
    {
        static Dictionary<string,string> storage=new Dictionary<string,string>();

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
