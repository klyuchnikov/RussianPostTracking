using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RussianPostTracking.Properties;

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
            Properties.Settings.Default.Reload();
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Inds))
                this.Listidentifier.AddRange(Properties.Settings.Default.Inds.Split(';'));
        }


        public void Save()
        {
            Properties.Settings.Default.PropertyValues["Inds"].PropertyValue = string.Join(";", this.Listidentifier.ToArray());
            Properties.Settings.Default.Save();
        }

        ~Model()
        {
            Save();
        }

    }
}
