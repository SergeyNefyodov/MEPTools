using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MEPTools
{
    internal class SettingsManager
    {
        internal static string filename
        {
            get
            { return @"C:\ProgramData\Autodesk\Revit\Addins\2022\CDSEngineeringTools\settings.txt"; }
        }

        internal void CheckExisting()
        {
            if (!File.Exists(filename))
            {
                File.Create(filename);
            }
        }
        private StreamWriter writer = null;
        internal void Record(string propertyName, string value)
        {    
            Dictionary<string, string> dict = Read();
            writer = new StreamWriter(filename);
            dict[propertyName] = value;
            foreach (var v in dict)
            {
                writer.WriteLine(v.Key+'|'+v.Value);
            }
            writer.Close();
            writer.Dispose();
        }
        internal Dictionary<string, string> Read()
        {
            string[] strings = File.ReadAllLines(filename);
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string s in strings)
            {
                string key = s.Substring(0, s.IndexOf('|'));
                string val = s.Substring(s.IndexOf('|') + 1);
                dict.Add(key, val);
            }
            return dict;
        }
    }
}
