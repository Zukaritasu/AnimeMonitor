using AnimeMonitor.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AnimeMonitor.IO {

    /// <summary>
    /// Write and read control of files that can be accessed from many threads
    /// </summary>
    public class IOController {

        /** there are threads that can possibly access the same file for reading
         * or writing so it must be controlled. */
        private static readonly object _lock = new object();

        public IOController() { }

        public string ReadAllText(string filename) {
            lock (_lock) {
                try {
                    if (!File.Exists(filename))
                        return null;
                    return File.ReadAllText(filename);
                } catch (Exception e) {
                    Logs.CreateReport(e);
                    return null;
                }
            }
        }

        public bool WriteAllText(string filename, string text) {
            lock (_lock) {
                try {
                    File.WriteAllText(filename, text);
                    return true;
                } catch (Exception e) {
                    Logs.CreateReport(e);
                    return false;
                }
            }
        }

        public bool WriteJArray(string filename, JArray jArray) {
            JObject jObject = new JObject {
                ["array"] = jArray
            };
            return WriteAllText(filename, jObject.ToString());
        }

        public JArray ReadJArray(string filename) {
            string text = ReadAllText(filename);
            return text == null ? new JArray() : JObject.Parse(text)["array"].ToObject<JArray>();
        }
    }
}
