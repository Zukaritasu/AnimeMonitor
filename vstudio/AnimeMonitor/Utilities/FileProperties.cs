using System;
using System.Collections.Generic;
using System.IO;

namespace AnimeMonitor.Utilities {
    public class FileProperties {

        private readonly string filepath = null;
        private readonly Dictionary<String, String> props = new Dictionary<String, String>();

        public FileProperties(string filepath) {
            this.filepath = filepath;
            if (filepath == null)
                throw new ArgumentNullException("filepath is null");
            OpenFile();
        }

        private void OpenFile() {
            try {
                if (File.Exists(filepath)) {
                    foreach (var line in File.ReadAllLines(filepath)) {
                        string _line = line.Trim();
                        if (!string.IsNullOrWhiteSpace(_line) && _line.Contains("=")) {
                            string[] parts = _line.Split('=');
                            props.Add(parts[0], parts[1]);
                        }
                    }
                }
            } catch (Exception e) {
                Logs.CreateReport(e);
            }
        }

        private void SaveFile() {
            if (props.Count > 0) {
                try {
                    using (StreamWriter writer = new StreamWriter(filepath)) {
                        foreach (var prop in props) {
                            writer.WriteLine($"{prop.Key}={prop.Value}");
                        }
                    }
                } catch (Exception e) {
                    Logs.CreateReport(e);
                }
            }
        }

        public string this[string str] {
            get {
                return props[str];
            } set {
                props[str] = value;
                SaveFile();
            }
        }

        public string this[string str, string def] {
            get {
                try {
                    return props[str];
                } catch (Exception) {
                    return def;
                }
            }
        }
    }
}
