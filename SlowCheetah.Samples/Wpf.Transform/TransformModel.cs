namespace Wpf.Transform {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Configuration;
    using System.IO;

    public class TransformModel {
        public TransformModel() {
            this.AppSettings = new Dictionary<string, string>();
            this.ConnectionStrings = new Dictionary<string, string>();
        }

        public IDictionary<string, string> AppSettings { get; set; }
        public IDictionary<string, string> ConnectionStrings { get; set; }
        public string ConfigContents { get; set; }

        public static TransformModel BuildFromCurrent() {
            TransformModel tm = new TransformModel();
            foreach (string key in ConfigurationManager.AppSettings.AllKeys) {
                tm.AppSettings.Add(key, ConfigurationManager.AppSettings[key]);
            }

            foreach (ConnectionStringSettings cn in ConfigurationManager.ConnectionStrings) {
                tm.ConnectionStrings.Add(cn.Name, cn.ConnectionString);
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            tm.ConfigContents = File.ReadAllText(config.FilePath);

            return tm;
        }
    }
}
