using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RussianPostTracking
{
    public class Model
    {
        private Model()
        {
            this.Listidentifier = new List<string>();
            Load();
        }

        public static Model Current = new Model();

        public List<string> Listidentifier { get; set; }

        public void Load()
        {
            if (File.Exists("Identifiers"))
            {
                var strs = File.ReadAllLines("Identifiers", Encoding.Default);
                this.Listidentifier.AddRange(strs);
            }
        }


        public void Save()
        {
            File.WriteAllLines("Identifiers", this.Listidentifier);
        }

        ~Model()
        {
            Save();
        }
        
    }
}
